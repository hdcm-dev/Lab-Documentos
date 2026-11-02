// Nuevo: ejemplo 07 (plan §E.2; guía §7.3–§7.5; OA-D4). RAG con citas validadas en código sobre los
// 12 literales de *Policy: recuperación top-3 en InMemoryVectorStore, contexto con la Fuente de cada
// fragmento, respuesta del modelo, y el código decide qué se muestra.
//
//   dotnet run -- --fake [--verificar]                 guion + embeddings estables, sin red
//   dotnet run -- --precomputar-embeddings             fase 3a, turno modelo-propio (OLLAMA_EMBED_ENDPOINT)
//   dotnet run -- --fake --precomputar-embeddings      la misma vía en seco (generador estable, archivo «guion»)
//   dotnet run -- --verificar [--plazo-s N]            turno modelo-compartido: chat contra OLLAMA_ENDPOINT,
//                                                      embeddings leídos de RLLM_DIR/embeddings/
//   dotnet run -- --verificar --estabilidad            3 repeticiones de r03 (no cuentan para la tasa)
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;

const string Ejemplo = "07-RAG-Con-Citas";
var argumentos = ArgumentosDeEjemplo.Leer(args);
var estabilidad = argumentos.Resto.Contains("--estabilidad");
var precomputar = argumentos.Resto.Contains("--precomputar-embeddings");

ConfiguracionDelModelo config;
try
{
    config = ConfiguracionDelModelo.DesdeEntorno().Validar();
}
catch (ConfiguracionInvalidaException e)
{
    Console.Error.WriteLine($"Configuración rechazada: {e.Message}");
    return 1;
}

var rutaOro = OroDe07.Ruta;   // REC-28: v2 = v1 de 06 + n05
var oro = ConjuntoDeOro.Leer(rutaOro);
var shaOro = Hashes.Sha256DeArchivo(rutaOro);
var rutaEmbeddings = ArchivoDeEmbeddings.Ruta(config.RegistroDir, Ejemplo);

// ---------------------------------------------------------------- fase 3a: sólo embeddings
if (precomputar)
{
    // Con --fake (corrida en seco del arnés): el generador estable, sin red; el archivo queda rotulado
    // «guion» y sólo sirve para probar la vía, nunca para una corrida que cuenta.
    string? digestEmb = "fnv1a-estable", versionEmb = "guion";
    var modeloEmb = argumentos.Fake ? "guion" : config.EmbedModelo;
    if (!argumentos.Fake)
    {
        var predicadoEmb = await PredicadoDeSalto.EvaluarAsync(config.EmbedEndpoint, config.EmbedModelo, PredicadoDeSalto.Embeddings);
        if (predicadoEmb.Saltar)
        {
            Console.Error.WriteLine($"No se precomputan embeddings: {predicadoEmb.Motivo}");
            return 1;
        }
        (digestEmb, versionEmb) = (predicadoEmb.Digest, predicadoEmb.VersionDeOllama);
    }
    using var generador = argumentos.Fake
        ? new GeneradorDeEmbeddingsEstable(FragmentoIndexado.Dimensiones)
        : FabricaDeClientes.CrearEmbeddingsOllama(config.EmbedEndpoint, config.EmbedModelo);
    var textos = CorpusDePolicies.Fragmentos().Select(f => f.Texto).Concat(oro.Preguntas.Select(p => p.Texto)).Distinct(StringComparer.Ordinal).ToList();
    var reloj = EscritorDeRegistro.Cronometro();
    var vectores = await generador.GenerateAsync(textos);
    var archivo = new ArchivoDeEmbeddings
    {
        Pieza = Ejemplo, Plantilla = CorpusDePolicies.Plantilla, Modelo = modeloEmb, Digest = digestEmb,
        OllamaVersion = versionEmb, OroSha256 = shaOro, Dimension = vectores[0].Vector.Length,
        Vectores = textos.Select((t, i) => new VectorGuardado(t, vectores[i].Vector.ToArray())).ToList()
    };
    if (argumentos.Fake) archivo = archivo with { Rotulo = "corrida en seco (--fake): generador estable FNV-1a, sin modelo; no cuenta para ninguna corrida" };
    archivo.Guardar(rutaEmbeddings);
    Console.WriteLine($"embeddings: {textos.Count} textos, dimensión {archivo.Dimension}, {archivo.Modelo}@{archivo.Digest}, Ollama {archivo.OllamaVersion}, {reloj.Elapsed.TotalSeconds:0.0} s");
    Console.WriteLine($"guardados en RLLM_DIR/embeddings/{Path.GetFileName(rutaEmbeddings)}");
    return archivo.Dimension == FragmentoIndexado.Dimensiones ? 0 : 1;
}

// ---------------------------------------------------------------- embeddings de la corrida
IEmbeddingGenerator<string, Embedding<float>> embeddings;
string rotuloEmbeddings;
if (argumentos.Fake || config.EmbPlan == "EMB-C")
{
    embeddings = new GeneradorDeEmbeddingsEstable(FragmentoIndexado.Dimensiones);
    rotuloEmbeddings = argumentos.Fake ? "emb=fnv1a-estable" : "emb=fnv1a-estable;EMB-C: no ejecutado contra un modelo de embeddings";
}
else
{
    if (!File.Exists(rutaEmbeddings))
    {
        Console.Error.WriteLine($"Faltan los embeddings precomputados (RLLM_DIR/embeddings/{Path.GetFileName(rutaEmbeddings)}): correr antes la fase 3a con --precomputar-embeddings.");
        return 1;
    }
    var archivo = ArchivoDeEmbeddings.Leer(rutaEmbeddings);
    if (archivo.OroSha256 != shaOro || archivo.Plantilla != CorpusDePolicies.Plantilla)
    {
        Console.Error.WriteLine("Los embeddings precomputados son de otro oro o de otra plantilla: volver a correr la fase 3a.");
        return 1;
    }
    if (archivo.Modelo == "guion")
    {
        Console.Error.WriteLine("Los embeddings precomputados salieron de una corrida en seco (--fake): volver a correr la fase 3a contra el modelo.");
        return 1;
    }
    embeddings = new TablaDeEmbeddings(archivo);
    rotuloEmbeddings = $"emb={archivo.Modelo}@{archivo.Digest};ollama={archivo.OllamaVersion}";
}

// ---------------------------------------------------------------- chat, siempre por la capa única
var preguntas = estabilidad
    ? Enumerable.Range(1, 3).Select(i => ($"r03#{i}", oro.Preguntas.Single(p => p.Id == "r03"))).ToList()
    : oro.Preguntas.Select(p => (p.Id, p)).ToList();
// El caso guionado E5 va sólo en modo falso, después de las 17 del oro v2, y no entra en las tasas.
if (argumentos.Fake && !estabilidad) preguntas.Add(("e5", GuionDelEjemplo.PreguntaE5));
var ejemplo = estabilidad ? $"{Ejemplo}.estabilidad" : Ejemplo;

GuionChatClient? guion = null;
IChatClient cliente;
ResultadoDelPredicado? predicado = null;
if (argumentos.Fake)
{
    guion = GuionDelEjemplo.Crear(preguntas.Select(p => p.Item1));
    cliente = FabricaDeClientes.Envolver(guion, config, Adaptador.OllamaSharp);
}
else
{
    predicado = await PredicadoDeSalto.EvaluarAsync(config.Endpoint, config.Modelo, ["completion"]);
    if (predicado.Saltar)
    {
        Console.Error.WriteLine($"No se corre contra el modelo: {predicado.Motivo}");
        return 1;
    }
    cliente = FabricaDeClientes.CrearChatOllama(config.Endpoint, config.Modelo, config);
}

using var registro = new EscritorDeRegistro(new ContextoDeCorrida
{
    Ejemplo = ejemplo,
    Configuracion = config,
    Adaptador = Adaptador.OllamaSharp,
    Modelo = argumentos.Fake ? "guion" : config.Modelo,
    Digest = predicado?.Digest,
    OllamaVersion = predicado?.VersionDeOllama,
    EntradasVersion = OroDe07.Version,
    EntradasHash = shaOro,
    PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
    Maquina = argumentos.Fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA")
});

ChatOptions? Efectivas(ChatOptions pedidas)
{
    if (guion is not null) return guion.Llamadas[^1].Opciones;
    var copia = pedidas.Clone();
    OpcionesDelProveedor.Aplicar(copia, config, Adaptador.OllamaSharp);
    return copia;
}

// En el servidor compartido, antes de cada entrada: si el modelo ya no está cargado, la llamada es «frio».
Func<CancellationToken, Task<string?>>? estadoAntes = !argumentos.Fake && config.Servidor == TipoDeServidor.Compartido
    ? async ct => await PredicadoDeSalto.ModeloCargadoAsync(config.Endpoint, config.Modelo, ct) == false ? "frio" : null
    : null;

var indice = await IndiceDeAyuda.CrearAsync(embeddings);
var asistente = new AsistenteDeAyuda(cliente, indice);
var corrida = new CorridaDeAyuda(asistente, registro, Efectivas, ejemplo, $"rag top{asistente.Top};{rotuloEmbeddings}", estadoAntes);
var resultado = await corrida.CorrerAsync(preguntas, argumentos.CrearPlazo());

Console.WriteLine($"07 · {(argumentos.Fake ? "modo falso: guion + embeddings estables (cableado, no calidad)" : $"{config.Modelo}@{predicado?.Digest}, Ollama {predicado?.VersionDeOllama}, servidor {config.ServidorTexto}, num_ctx {config.NumCtx} ({config.NumCtxOrigen})")}");
Console.WriteLine($"embeddings: {rotuloEmbeddings} · oro sha256 {shaOro[..12]} · top-{asistente.Top}");
foreach (var r in resultado.Resultados)
{
    var c = r.Consulta;
    Console.WriteLine($"{r.EntradaId}: «{r.Pregunta.Texto}»");
    if (c is not null)
    {
        Console.WriteLine($"  recuperados: {string.Join(", ", c.Recuperados.Select(x => $"{x.Fuente} ({x.Puntaje:0.000})"))}");
        Console.WriteLine($"  modelo: {c.Validada.Texto.ReplaceLineEndings(" ")}");
        Console.WriteLine($"  código: {c.Validada.Veredicto}{(c.Validada.Mostrada == c.Validada.Texto ? "" : " → se muestra el aviso de rechazo")}");
    }
    Console.WriteLine($"  oráculo {OraculoDeCitas.Nombre}: {r.Evaluacion.Resultado}{(r.Evaluacion.Motivo is null ? "" : $" ({r.Evaluacion.Motivo})")}");
}

// Tasas descriptivas (§10.4; estímulo del mismo autor). El intervalo lo da tools/verificar.sh tasa k m.
if (!estabilidad)
{
    var delOro = resultado.Resultados.Where(r => r.EntradaId != "e5").ToList();
    var validos = delOro.Where(r => r.Evaluacion.Resultado != ResultadoDeEntrada.Excluida).ToList();
    string Tasa(Func<ResultadoDePregunta, bool> grupo, Func<ResultadoDePregunta, bool> exito)
    {
        var total = delOro.Count(grupo);
        var g = validos.Where(grupo).ToList();
        return $"{g.Count(exito)}/{g.Count} (excluidas {total - g.Count})";
    }
    bool ConValor(ResultadoDePregunta r) => r.Pregunta.Respondible && !r.Pregunta.SinValorVerificable;
    bool SinValor(ResultadoDePregunta r) => r.Pregunta.Respondible && r.Pregunta.SinValorVerificable;
    Console.WriteLine($"afirmación correcta (con valor verificable): {Tasa(ConValor, r => r.Evaluacion.Resultado == ResultadoDeEntrada.Pasa)}");
    Console.WriteLine($"cita válida (sin valor verificable):        {Tasa(SinValor, r => r.Evaluacion.CitaValida)}");
    Console.WriteLine($"«No sé» (no respondibles):                  {Tasa(r => !r.Pregunta.Respondible, r => r.Evaluacion.NoSe)}");
    Console.WriteLine($"cita y contradice (respondibles):           {Tasa(r => r.Pregunta.Respondible, r => r.Evaluacion.CitaYContradice)}");
}
else
{
    var textos = resultado.Resultados.Select(r => r.Consulta?.Validada.Texto).Distinct().Count();
    Console.WriteLine($"estabilidad: {resultado.Resultados.Count} repeticiones, {textos} texto(s) distinto(s) (no cuenta para la tasa)");
}
Console.WriteLine($"registro: {registro.RenglonesEscritos} renglones en {Path.GetFileName(registro.Ruta)}");

var fallan = resultado.InvariantesQueFallan.ToList();
if (argumentos.Fake && !estabilidad)
{
    // En modo falso el guion trae tres respuestas malas: el código y el oráculo tienen que verlas.
    var por = resultado.Resultados.ToDictionary(r => r.EntradaId);
    if (por["e5"].Consulta?.Validada.Veredicto != Veredicto.Aceptada) fallan.Add("e5: el caso E5 no llegó con cita válida (el índice no recuperó sus fuentes)");
    if (por["e5"].Evaluacion.Resultado != ResultadoDeEntrada.Falla) fallan.Add("e5: el oráculo no marcó el caso E5");
    if (por["r09"].Consulta?.Validada.Veredicto != Veredicto.RechazadaCitaNoRecuperada) fallan.Add("r09: la cita inventada no se rechazó");
    if (por["n02"].Consulta?.Validada.Veredicto != Veredicto.RechazadaSinCita) fallan.Add("n02: la respuesta sin cita no se rechazó");
}
foreach (var f in fallan) Console.Error.WriteLine($"invariante que falla: {f}");
Console.WriteLine(fallan.Count == 0 ? "invariantes de código: se cumplen" : $"invariantes de código: fallan {fallan.Count}");
return argumentos.Verificar && fallan.Count > 0 ? 1 : 0;
