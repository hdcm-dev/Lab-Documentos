// Nuevo: la corrida del ejemplo 08, separada de Program.cs para que la prueba [Category("LLM")] la
// reuse. Corre las entradas, deja una línea R-LLM por llamada al modelo y una por entrada, y
// comprueba las garantías de código, que valen con o sin modelo.
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;

namespace MovilidadUrbana.Asistente;

public sealed record ResultadoDeCorrida(IReadOnlyList<string> InvariantesQueFallan, int Pasan, int Validas, int Total, string? RutaDelRegistro,
    TasasDeExtension? Tasas = null);

/// <summary>
/// Las cuentas de la extensión (plan §E.2, fila 08): k/n legítimas con herramienta invocada y
/// argumentos válidos (al menos una herramienta y ninguna con error), k/n negativas sin invención
/// (pasan el oráculo) y, aparte, las legítimas que fallan por «sin cifra». n es el de las válidas
/// de cada tipo (las excluidas no cuentan); con las 8 entradas sin exclusiones, n = 4.
/// </summary>
public sealed record TasasDeExtension(int ConHerramientaValida, int LegitimasValidas, int NegativasSinInvencion, int NegativasValidas, int SinCifra);

public static class Corrida
{
    private static readonly JsonSerializerOptions Legible = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public const string Ejemplo = "08-Primera-Herramienta-Solo-Lectura";

    public static async Task<ResultadoDeCorrida> CorrerAsync(
        ArgumentosDeEjemplo a, ConfiguracionDelModelo config, TextWriter salida, CancellationToken cancellationToken = default)
    {
        var extension = a.Resto.Contains("--extension");
        // Nuevo (REC-29): --fuera-de-catalogo corre el estímulo «provincia fuera del catálogo» con el
        // oráculo afirma-sin-herramienta en lugar de no-inventa.
        var fueraDeCatalogo = a.Resto.Contains("--fuera-de-catalogo");
        var rutaEntradas = Path.Combine(AppContext.BaseDirectory,
            fueraDeCatalogo ? "entradas.fuera-de-catalogo.v1.json" : extension ? "entradas.extension.v1.json" : "entradas.v1.json");
        var nombreDelOraculo = fueraDeCatalogo ? OraculoAfirmaSinHerramienta.Nombre : OraculoNoInventa.Nombre;
        var entradas = ArchivoDeEntradas.Leer(rutaEntradas).Entradas;
        var invariantes = new List<string>();

        using var bd = await BaseDeEjemplo.CrearAsync(cancellationToken);
        using var propia = bd.AbrirSesion(BaseDeEjemplo.SesionPropia);
        var herramientas = new HerramientasDeLectura(propia.Localidades);

        // Cliente: siempre por la capa única. En --fake, el guion pide herramientas como lo haría el
        // modelo (una válida, una inválida y después la buena) y responde sin inventar.
        var guion = new GuionChatClient(GuionDelEjemplo.Pasos(entradas), "No tengo ese dato.");
        var espia = new ClienteEspia(guion);
        var capa = a.Fake
            ? FabricaDeClientes.Envolver(espia, config, Adaptador.OllamaSharp)
            : FabricaDeClientes.CrearChatOllama(config.Endpoint, config.Modelo, config);

        string? version = null, digest = null;
        if (!a.Fake)
        {
            var p = await PredicadoDeSalto.EvaluarAsync(config.Endpoint, config.Modelo, PredicadoDeSalto.Chat, cancellationToken);
            if (p.Saltar)
            {
                return new ResultadoDeCorrida([$"no se corre contra el modelo: {p.Motivo}"], 0, 0, entradas.Count, null);
            }
            (version, digest) = (p.VersionDeOllama, p.Digest);
        }

        using var reg = new EscritorDeRegistro(new ContextoDeCorrida
        {
            Ejemplo = Ejemplo,
            Configuracion = config,
            Adaptador = Adaptador.OllamaSharp,
            Modelo = a.Fake ? "guion" : config.Modelo,
            Digest = digest,
            OllamaVersion = version,
            EntradasVersion = "v1",
            EntradasHash = Hashes.Sha256DeArchivo(rutaEntradas),
            PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
            Maquina = a.Fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA")
        });

        var asistente = new AsistenteDeConsulta(capa);
        var plazo = a.CrearPlazo();
        int pasan = 0, validas = 0;
        int conHerramientaValida = 0, legitimasValidas = 0, negativasSinInvencion = 0, negativasValidas = 0, sinCifra = 0;

        foreach (var entrada in entradas)
        {
            if (plazo.Vencido)
            {
                reg.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = entrada.Id, Oraculo = nombreDelOraculo, Resultado = ResultadoDeEntrada.Excluida, Motivo = "plazo" });
                salida.WriteLine($"{entrada.Id}: excluida (plazo)");
                continue;
            }

            string? frio = null;
            if (!a.Fake && config.Servidor == TipoDeServidor.Compartido &&
                await PredicadoDeSalto.ModeloCargadoAsync(config.Endpoint, config.Modelo, cancellationToken) == false)
            {
                frio = "frio";
            }

            var respuesta = await asistente.PreguntarAsync(entrada.Texto, herramientas, cancellationToken);

            // Resultados de las herramientas, tal como volvieron al modelo (están en la respuesta del bucle).
            var resultados = respuesta.Messages.SelectMany(m => m.Contents).OfType<FunctionResultContent>()
                .GroupBy(r => r.CallId).ToDictionary(g => g.Key, g => g.Last());
            var lineas = new List<RegistroDeLlamada>();
            var grabadas = asistente.Grabador.Llamadas;
            for (var i = 0; i < grabadas.Count; i++)
            {
                var g = grabadas[i];
                var efectivas = g.Opciones?.Clone() ?? new ChatOptions();
                OpcionesDelProveedor.Aplicar(efectivas, config, Adaptador.OllamaSharp);
                var pedidas = g.Respuesta.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>()
                    .Select(c => resultados.TryGetValue(c.CallId, out var r)
                        ? new HerramientaInvocada(c.Name, c.Arguments, r.Result, r.Exception?.Message)
                        : new HerramientaInvocada(c.Name, c.Arguments, null, "sin ejecutar (corte del bucle)"))
                    .ToList();
                var linea = reg.Llamada(entrada.Id, i + 1, g.Mensajes, efectivas, g.Respuesta, g.Reloj,
                    paso: pedidas.Count > 0 ? "pedir-herramienta" : "responder", herramientas: pedidas,
                    estadoCacheForzado: i == 0 ? frio : null);
                reg.Escribir(linea);
                lineas.Add(linea);
                if (linea.Razonamiento != "None" || linea.NumCtx is not > 0)
                {
                    invariantes.Add($"{entrada.Id}, llamada {i + 1}: no salió con razonamiento apagado y num_ctx explícito");
                }
            }

            var invocadas = lineas.Sum(l => l.Herramientas.Count);
            if (grabadas.Count > AsistenteDeConsulta.MaximoDeIteraciones + 1)
            {
                invariantes.Add($"{entrada.Id}: {grabadas.Count} llamadas al modelo, más que el tope del bucle");
            }
            // Un nombre que no está montado no se ejecuta: el bucle le devuelve al modelo
            // «Error: Requested function "x" not found.». Eso no viola «sólo lectura» (lo cuenta el oráculo).
            var noDeLectura = lineas.SelectMany(l => l.Herramientas)
                .Where(h => h.Nombre is not (HerramientasDeLectura.Listar or HerramientasDeLectura.Contar))
                .Where(h => !(h.Resultado is string texto && texto.StartsWith("Error: Requested function", StringComparison.Ordinal)))
                .Select(h => h.Nombre).ToList();
            if (noDeLectura.Count > 0) invariantes.Add($"{entrada.Id}: se ejecutó algo que no es de lectura ({string.Join(", ", noDeLectura)})");

            var todasLasHerramientas = JsonSerializer.SerializeToElement(lineas.SelectMany(l => l.Herramientas).ToList());
            var truncado = lineas.Any(l => l.TruncadoPosible == true);
            var veredicto = fueraDeCatalogo
                ? OraculoAfirmaSinHerramienta.Evaluar(lineas.Count > 0 ? lineas[^1].Salida : null, todasLasHerramientas, truncado)
                : OraculoNoInventa.Evaluar(entrada.Id, lineas.Count > 0 ? lineas[^1].Salida : null, todasLasHerramientas, truncado);
            if (lineas.Count == 0) veredicto = new VeredictoDelOraculo(ResultadoDeEntrada.Excluida, "sin llamadas");
            reg.Escribir(new RegistroDeEntrada
            {
                Ejemplo = Ejemplo,
                EntradaId = entrada.Id,
                Oraculo = nombreDelOraculo,
                Resultado = veredicto.Resultado,
                Motivo = veredicto.Motivo
            });
            if (veredicto.Resultado != ResultadoDeEntrada.Excluida)
            {
                validas++;
                if (OraculoNoInventa.EsLegitima(entrada.Id))
                {
                    legitimasValidas++;
                    if (invocadas > 0 && !OraculoNoInventa.HuboError(todasLasHerramientas)) conHerramientaValida++;
                    if (veredicto.Motivo == "sin cifra") sinCifra++;
                }
                else
                {
                    negativasValidas++;
                    if (veredicto.Resultado == ResultadoDeEntrada.Pasa) negativasSinInvencion++;
                }
            }
            if (veredicto.Resultado == ResultadoDeEntrada.Pasa) pasan++;

            salida.WriteLine($"{entrada.Id} ({entrada.Tipo}: {entrada.Caso}): «{entrada.Texto}»");
            foreach (var h in lineas.SelectMany(l => l.Herramientas))
            {
                salida.WriteLine($"  herramienta {h.Nombre}({JsonSerializer.Serialize(h.Argumentos ?? new Dictionary<string, object?>(), Legible)}) → {JsonSerializer.Serialize(h.Resultado, Legible)}");
            }
            salida.WriteLine($"  respuesta {(a.Fake ? "del guion" : "del modelo")}: {respuesta.Text}");
            salida.WriteLine($"  llamadas al modelo: {grabadas.Count} · herramientas ejecutadas: {invocadas}");
            salida.WriteLine($"  oráculo {nombreDelOraculo}: {veredicto.Resultado}{(veredicto.Motivo is null ? "" : $" ({veredicto.Motivo})")}");
        }

        // Garantías de código: no dependen del modelo y se comprueban en toda corrida.
        invariantes.AddRange(await Garantias.SesionAjenaNuncaSeAlcanzaAsync(bd, cancellationToken));
        var corte = await Garantias.ElBucleSeCortaAsync(config, cancellationToken);
        invariantes.AddRange(corte.Fallas);
        salida.WriteLine($"corte del bucle (guion que pide listar_localidades sin parar): {corte.Ejecutadas} herramientas ejecutadas y {corte.Llamadas} llamadas al «modelo», con MaximumIterationsPerRequest = {AsistenteDeConsulta.MaximoDeIteraciones}");
        invariantes.AddRange(await Garantias.ProvinciaFueraDeCatalogoEsErrorAsync(herramientas, cancellationToken));
        invariantes.AddRange(Garantias.SoloHerramientasDeLectura(herramientas));
        invariantes.AddRange(await Garantias.LaBaseNoCambioAsync(propia, cancellationToken));
        if (a.Fake && espia.OpcionesVistas.Any(o => o?.Reasoning?.Effort != ReasoningEffort.None || OpcionesDelProveedor.NumCtxDe(o) is not > 0))
        {
            invariantes.Add("--fake: el cliente espía vio una llamada sin razonamiento apagado o sin num_ctx");
        }

        var tasas = new TasasDeExtension(conHerramientaValida, legitimasValidas, negativasSinInvencion, negativasValidas, sinCifra);
        if (fueraDeCatalogo)
        {
            salida.WriteLine($"provincia fuera del catálogo (estímulo del mismo autor; sin tasa en --fake): {pasan} de {entradas.Count} entradas pasan el oráculo {OraculoAfirmaSinHerramienta.Nombre}");
        }
        else if (extension)
        {
            salida.WriteLine($"extensión (estímulo del mismo autor; descriptiva, §10.4): herramienta invocada con argumentos válidos {tasas.ConHerramientaValida}/{tasas.LegitimasValidas} legítimas · negativas sin invención {tasas.NegativasSinInvencion}/{tasas.NegativasValidas}");
            salida.WriteLine($"  aparte: {tasas.SinCifra} legítimas «sin cifra» · oráculo {OraculoNoInventa.Nombre}: {pasan}/{validas} válidas pasan · {entradas.Count - validas} excluidas");
        }
        else
        {
            salida.WriteLine($"demostración: {pasan} de {entradas.Count} entradas pasan el oráculo {OraculoNoInventa.Nombre} (sin tasa)");
        }
        salida.WriteLine($"registro: {reg.RenglonesEscritos} líneas en {Path.GetFileName(reg.Ruta)}");
        return new ResultadoDeCorrida(invariantes, pasan, validas, entradas.Count, reg.Ruta, extension ? tasas : null);
    }
}
