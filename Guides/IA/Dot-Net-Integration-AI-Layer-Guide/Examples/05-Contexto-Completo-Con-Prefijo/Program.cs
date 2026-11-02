// Nuevo: ejemplo 05 (plan §E.2; guía §6.3–§6.5; OA-D3). Contexto completo con prefijo estable:
// las mismas 6 preguntas sobre el mismo padrón, cada una «sin-prefijo» (nonce distinto al
// principio) y «con-prefijo» (prefijo idéntico byte a byte), más una vuelta a frío. Mide sólo
// duraciones y conteos del servidor; la calidad de la respuesta NO se mide (README).
//
//   dotnet run -- --fake [--verificar] [--orden alternado|calentamiento]
//   dotnet run -- --verificar                      (contra el Ollama propio: OLLAMA_PROPIO_ENDPOINT)
//   dotnet run -- --generar-padron corpus/padron-de-localidades.v1.json   (sólo para congelar v1)
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;

const string Ejemplo = "05-Contexto-Completo-Con-Prefijo";
var argumentos = ArgumentosDeEjemplo.Leer(args);

var generar = argumentos.Resto.ToList().IndexOf("--generar-padron");
if (generar >= 0)
{
    var destino = generar + 1 < argumentos.Resto.Count ? argumentos.Resto[generar + 1] : "corpus/padron-de-localidades.v1.json";
    File.WriteAllText(destino, PadronDeLocalidades.Serializar(PadronDeLocalidades.Generar()));
    Console.WriteLine($"padrón escrito: {destino} (congelalo con su SHA-256 en ejemplo.json)");
    return 0;
}

var iOrden = argumentos.Resto.ToList().IndexOf("--orden");
var orden = iOrden >= 0 && iOrden + 1 < argumentos.Resto.Count && argumentos.Resto[iOrden + 1] == "calentamiento"
    ? OrdenDeConsultas.AlternadoConCalentamiento : OrdenDeConsultas.Alternado;
// El registro de la variante con calentamiento (desvío) va a su propio archivo, para no mezclarse con el del plan.
var ejemplo = orden == OrdenDeConsultas.AlternadoConCalentamiento ? $"{Ejemplo}.calentamiento" : Ejemplo;

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

var rutaEntradas = Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");
var rutaPadron = Path.Combine(AppContext.BaseDirectory, "corpus", "padron-de-localidades.v1.json");
List<Pregunta> preguntas;
using (var doc = JsonDocument.Parse(File.ReadAllText(rutaEntradas)))
{
    preguntas = doc.RootElement.GetProperty("entradas").EnumerateArray()
        .Select(e => new Pregunta(e.GetProperty("id").GetString()!, e.GetProperty("texto").GetString()!)).ToList();
}
var prefijo = PrefijoEstable.Construir(PadronDeLocalidades.ATexto(PadronDeLocalidades.Leer(rutaPadron)));
var plan = PlanDeConsultas.Armar(preguntas, prefijo, orden, () => Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(8)));

// Cliente: siempre por la capa única. En --fake, el guion debajo de un simulador de caché de dos
// ranuras (duraciones inventadas, con la forma de lo observado en Ollama 0.40.11); en real, el Ollama propio (el compartido no se toca: R-04).
GuionChatClient? guion = null;
IChatClient cliente;
ResultadoDelPredicado? predicado = null;
if (argumentos.Fake)
{
    guion = new GuionChatClient([], "(respuesta guionada: en este ejemplo la calidad no se mide)");
    cliente = FabricaDeClientes.Envolver(new SimuladorDeCacheDePrefijo(guion, ranuras: 2), config, Adaptador.OllamaSharp);
}
else
{
    predicado = await PredicadoDeSalto.EvaluarAsync(config.PropioEndpoint, config.PropioModelo, ["completion"]);
    if (predicado.Saltar)
    {
        Console.Error.WriteLine($"No se corre contra el modelo: {predicado.Motivo}");
        return 1;
    }
    cliente = FabricaDeClientes.CrearChatOllama(config.PropioEndpoint, config.PropioModelo, config);
    // Si el modelo ya estaba cargado, la caché puede traer prefijos de una corrida anterior (dentro
    // del keep_alive): la vuelta a frío y el primer par dejan de ser «en frío». Se avisa; no se corrige.
    if (await PredicadoDeSalto.ModeloCargadoAsync(config.PropioEndpoint, config.PropioModelo) == true)
    {
        Console.WriteLine("aviso: el modelo ya estaba cargado al empezar; la caché puede traer prefijos de corridas anteriores.");
    }
}

using var registro = new EscritorDeRegistro(new ContextoDeCorrida
{
    Ejemplo = ejemplo,
    Configuracion = config,
    Adaptador = Adaptador.OllamaSharp,
    Modelo = argumentos.Fake ? "guion" : config.PropioModelo,
    Digest = predicado?.Digest,
    OllamaVersion = predicado?.VersionDeOllama,
    EntradasVersion = "v1",
    EntradasHash = Hashes.Sha256DeArchivo(rutaEntradas),
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

var corrida = new CorridaDelPrefijo(cliente, registro, Efectivas, config, ejemplo);
var resultado = await corrida.CorrerAsync(preguntas, prefijo, plan, argumentos.CrearPlazo());

Console.WriteLine($"05 · orden {(orden == OrdenDeConsultas.Alternado ? "alternado (plan)" : "alternado con calentamiento (desvío)")} · {(argumentos.Fake ? "modo falso: duraciones SIMULADAS" : $"{config.PropioModelo}, Ollama {predicado?.VersionDeOllama}")}");
Console.WriteLine($"padrón: {PadronDeLocalidades.Cantidad} registros, sha256 {Hashes.Sha256DeArchivo(rutaPadron)[..12]}; prefijo {prefijo.Length} caracteres");
Console.WriteLine("entrada | paso          | prompt_eval_count | prompt_eval_ms | load_ms | estado_cache");
foreach (var l in resultado.Llamadas)
{
    Console.WriteLine($"{l.EntradaId,-7} | {l.Paso,-13} | {l.PromptEvalCount,17} | {l.PromptEvalDurationMs,14} | {l.LoadDurationMs,7} | {l.EstadoCache}");
}
var validas = resultado.Veredictos.Values.Where(v => v.Resultado != ResultadoDeEntrada.Excluida).ToList();
var aFavor = validas.Count(v => v.Resultado == ResultadoDeEntrada.Pasa);
foreach (var (id, v) in resultado.Veredictos)
{
    Console.WriteLine($"par {id}: {v.Resultado}{(v.Razon is { } r ? $" (con/sin = {r:0.000})" : "")}{(v.Motivo is null ? "" : $" — {v.Motivo}")}");
}
Console.WriteLine($"pares a favor: {aFavor} de {validas.Count} válidos ({preguntas.Count - validas.Count} excluidos). " +
    (aFavor == preguntas.Count ? "«con-prefijo ≪ sin-prefijo»: 6 de 6 a favor." : "No se afirma «con-prefijo ≪ sin-prefijo» (hace falta 6 de 6, §10.4)."));
Console.WriteLine($"registro: {registro.RenglonesEscritos} renglones en {Path.GetFileName(registro.Ruta)}");

foreach (var f in resultado.InvariantesQueFallan) Console.Error.WriteLine($"invariante que falla: {f}");
return argumentos.Verificar && resultado.InvariantesQueFallan.Count > 0 ? 1 : 0;
