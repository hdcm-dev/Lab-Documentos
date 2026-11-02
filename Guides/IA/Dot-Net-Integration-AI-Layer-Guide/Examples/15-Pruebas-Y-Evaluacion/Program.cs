// Nuevo: ejemplo 15 (plan §E.2; guía §10; OA-D9, OA-O3). Evalúa las 16 salidas de 07 y 10 casos de
// calibración: primero los evaluadores deterministas (cita válida y afirmación central de 07), después
// RelevanceEvaluator y GroundednessEvaluator con un juez local, por el reporte con caché en disco.
// Después, aparte y descriptiva (REC-18): EquivalenceEvaluator contra la referencia del oro, sobre las
// salidas de 07 respondibles; no entra en el oráculo «acuerdo-del-juez» ni es compuerta.
//
//   dotnet run -- --fake [--verificar]                  juez guionado, sin red (cableado, no calidad)
//   dotnet run -- --verificar [--plazo-s N]             fase 3b, turno modelo-propio: juez OLLAMA_PROPIO_MODEL
//                                                       (qwen2.5:0.5b) en OLLAMA_PROPIO_ENDPOINT
//   dotnet run -- ... --reporte <carpeta>               reusa un reporte anterior: lo que esté en su caché
//                                                       no vuelve a llamar al juez
//   dotnet run -- --congelar-salidas-07 <registro.jsonl>
//                                                       arma entradas/salidas-07.v2.json desde el registro
//                                                       R-LLM de la última corrida válida de 07
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;

const string Ejemplo = "15-Pruebas-Y-Evaluacion";
var argumentos = ArgumentosDeEjemplo.Leer(args);
string? Valor(string nombre)
{
    var i = argumentos.Resto.ToList().IndexOf(nombre);
    return i >= 0 && i + 1 < argumentos.Resto.Count ? argumentos.Resto[i + 1] : null;
}

var oro = ConjuntoDeOro.Leer();

// ---------------------------------------------------------------- congelar las salidas de 07
if (Valor("--congelar-salidas-07") is { } registro07)
{
    var destino = Path.Combine(Directory.GetCurrentDirectory(), SalidasDe07.Archivo);
    var congeladas = CongeladorDeSalidas.Desde(registro07, oro);
    File.WriteAllText(destino, JsonSerializer.Serialize(congeladas, new JsonSerializerOptions
    {
        WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    }) + "\n");
    Console.WriteLine($"salidas de 07: {congeladas.Salidas.Count} congeladas en {SalidasDe07.Archivo} (turno {congeladas.Procedencia.TurnoId})");
    return congeladas.Salidas.Count == oro.Preguntas.Count ? 0 : 1;
}

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

var salidas = SalidasDe07.Leer();
var calibracion = CalibracionDelJuez.Leer();
var shaSalidas = Hashes.Sha256DeArchivo(SalidasDe07.RutaPorDefecto);
var shaCalibracion = Hashes.Sha256DeArchivo(CalibracionDelJuez.RutaPorDefecto);
var casos = CasoDeEvaluacion.Armar(oro, salidas, shaSalidas, calibracion, shaCalibracion);

// ---------------------------------------------------------------- el juez, siempre por la capa única
GuionChatClient? guion = null;
IChatClient baseDelJuez;
ResultadoDelPredicado? predicado = null;
if (argumentos.Fake)
{
    guion = GuionDelJuez.Crear(casos);
    baseDelJuez = FabricaDeClientes.Envolver(guion, config, Adaptador.OllamaSharp);
}
else
{
    predicado = await PredicadoDeSalto.EvaluarAsync(config.PropioEndpoint, config.PropioModelo, ["completion"]);
    if (predicado.Saltar)
    {
        Console.Error.WriteLine($"No se corre contra el juez: {predicado.Motivo}");
        return 1;
    }
    baseDelJuez = FabricaDeClientes.CrearChatOllama(config.PropioEndpoint, config.PropioModelo, config);
}
using var grabador = new GrabadorDeLlamadas(baseDelJuez);

using var escritor = new EscritorDeRegistro(new ContextoDeCorrida
{
    Ejemplo = Ejemplo,
    Configuracion = config,
    Adaptador = Adaptador.OllamaSharp,
    Modelo = argumentos.Fake ? "guion" : config.PropioModelo,
    Digest = predicado?.Digest,
    OllamaVersion = predicado?.VersionDeOllama,
    EntradasVersion = "v1",
    EntradasHash = shaSalidas,
    PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
    Maquina = argumentos.Fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA")
});

ChatOptions? Efectivas(ChatOptions? pedidas)
{
    if (guion is not null) return guion.Llamadas[^1].Opciones;
    var copia = pedidas?.Clone() ?? new ChatOptions();
    OpcionesDelProveedor.Aplicar(copia, config, Adaptador.OllamaSharp);
    return copia;
}

// Reporte en RLLM_DIR (fuera del repo). Una carpeta nueva por corrida, salvo que se pida reusar una.
var ejecucion = $"{(argumentos.Fake ? "fake" : "juez")}-{DateTime.UtcNow:yyyyMMddTHHmmss}";
var almacen = Valor("--reporte") ?? Path.Combine(config.RegistroDir, "evaluacion-15", ejecucion);
var corrida = new CorridaDeEvaluacion(grabador, escritor, Efectivas, Ejemplo, almacen, ejecucion, calibracion);
var plazo = argumentos.CrearPlazo();
var resultado = await corrida.CorrerAsync(casos, plazo);
// REC-18: Equivalence contra la referencia del oro, después y aparte (descriptiva, fuera del oráculo).
var equivalencia = await new CorridaDeEquivalencia(grabador, escritor, Efectivas, almacen, ejecucion, calibracion.Umbral).CorrerAsync(casos, plazo);

// ---------------------------------------------------------------- salida
Console.WriteLine($"15 · {(argumentos.Fake ? "modo falso: juez guionado (cableado, no calidad)" : $"juez {config.PropioModelo}@{predicado?.Digest}, Ollama {predicado?.VersionDeOllama}, servidor {config.ServidorTexto}, num_ctx {config.NumCtx} ({config.NumCtxOrigen})")}");
Console.WriteLine($"salidas de 07: {salidas.Procedencia.Modelo}@{salidas.Procedencia.Digest} (sha256 {shaSalidas[..12]}) · calibración sha256 {shaCalibracion[..12]} · anotador: {calibracion.Anotador}");
Console.WriteLine($"reporte: {Path.GetFileName(almacen)} (caché de respuestas activada) · umbral «alta» = puntaje ≥ {calibracion.Umbral}");
foreach (var r in resultado.Casos)
{
    var l = string.Join(" ", r.Lecturas.Select(x => $"{x.Evaluador}={x.Puntaje?.ToString() ?? "¿?"}"));
    var esperado = r.Caso.Grupo == GrupoDeEntrada.Calibracion
        ? $"anotado: {(r.Caso.Anotacion!.Relevante ? "relevante" : "no relevante")}, {(r.Caso.Anotacion.Fundamentada ? "fundamentada" : "no fundamentada")}"
        : $"oráculo de 07: {r.Caso.Resultado07}";
    Console.WriteLine($"{r.Caso.EntradaId,-8} {(r.Caso.Tipo ?? "salida de 07"),-19} determinista {r.Determinista,-5} · juez {(l.Length == 0 ? "—" : l),-28} · {esperado} → {OraculoDelJuez.Nombre}: {r.Veredicto.Resultado}{(r.Veredicto.Motivo is null ? "" : $" ({r.Veredicto.Motivo})")}");
}

// Tasas descriptivas (§10.4: M ≤ 12 en cada grupo; estímulo del mismo autor). El intervalo, con tools/verificar.sh tasa k m.
string Tasa(string grupo)
{
    var g = resultado.Casos.Where(r => r.Caso.Grupo == grupo).ToList();
    var validos = g.Where(r => r.Veredicto.Resultado != "excluida").ToList();
    return $"{validos.Count(r => r.Veredicto.Resultado == "pasa")}/{validos.Count} (excluidas {g.Count - validos.Count})";
}
var lecturas = resultado.Casos.SelectMany(r => r.Lecturas).ToList();
var calib = resultado.Casos.Where(r => r.Caso.Grupo == GrupoDeEntrada.Calibracion && r.Veredicto.Resultado != "excluida").ToList();
var aciertos = calib.Count(r => r.Veredicto.Resultado == "pasa");
var noDescartado = CorridaDeEvaluacion.JuezNoDescartado(resultado, calibracion);
var s07 = resultado.Casos.Where(r => r.Caso.Grupo == GrupoDeEntrada.Salidas07).ToList();
Console.WriteLine($"deterministas sobre las 16 de 07: pasan {s07.Count(r => r.Determinista == "pasa")}/16 (coinciden con 07: {s07.Count(r => r.Determinista == r.Caso.Resultado07)}/16)");
var calTodos = resultado.Casos.Where(r => r.Caso.Grupo == GrupoDeEntrada.Calibracion).ToList();
Console.WriteLine($"deterministas sobre la calibración: detectan {calTodos.Count(r => r.Caso.Tipo != TipoDeCaso.Correcta && r.Determinista == "falla")}/5 perturbaciones y aceptan {calTodos.Count(r => r.Caso.Tipo == TipoDeCaso.Correcta && r.Determinista == "pasa")}/5 correctas");
Console.WriteLine($"llamadas al juez: {resultado.LlamadasAlJuez} · salidas interpretables: {lecturas.Count(x => x.Interpretable)}/{lecturas.Count}");
Console.WriteLine($"acuerdo juez-oráculo determinista (16 salidas de 07): {Tasa(GrupoDeEntrada.Salidas07)}");
Console.WriteLine($"acuerdo juez-anotador (10 de calibración, mismo origen que las 16): {Tasa(GrupoDeEntrada.Calibracion)}");
Console.WriteLine(noDescartado
    ? $"juez: no descartado ({aciertos}/{calibracion.Casos.Count} ≥ {calibracion.AciertosMinimos}); con 10 casos no queda validado"
    : $"juez no calibrado: no se interpretan sus puntajes ({aciertos}/{calibracion.Casos.Count} < {calibracion.AciertosMinimos} o con salidas no interpretables); la conclusión se apoya sólo en los deterministas");
var eqValidas = equivalencia.Lecturas.Where(l => l.Exclusion is null).ToList();
Console.WriteLine($"equivalencia con la referencia del oro (salidas de 07 respondibles; descriptiva, no es compuerta): " +
    $"interpretables {eqValidas.Count(l => l.Interpretable)}/{eqValidas.Count}, altas {eqValidas.Count(l => l.Interpretable && l.Puntaje >= calibracion.Umbral)} (excluidas {equivalencia.Lecturas.Count - eqValidas.Count})");
Console.WriteLine($"registro: {escritor.RenglonesEscritos} renglones en {Path.GetFileName(escritor.Ruta)}");

var fallan = resultado.InvariantesQueFallan.Concat(equivalencia.InvariantesQueFallan).ToList();
if (argumentos.Fake && !argumentos.Resto.Contains("--reporte"))
{
    // En modo falso el guion trae un desacuerdo y una salida no interpretable: el oráculo tiene que verlos.
    var oraculo = s07.Count(r => r.Veredicto.Resultado == "pasa");
    if (oraculo != GuionDelJuez.AcuerdoConOraculoEsperado) fallan.Add($"guion: acuerdo con el oráculo {oraculo}, se esperaba {GuionDelJuez.AcuerdoConOraculoEsperado}");
    if (aciertos != GuionDelJuez.AcuerdoConAnotadorEsperado) fallan.Add($"guion: acuerdo con el anotador {aciertos}, se esperaba {GuionDelJuez.AcuerdoConAnotadorEsperado}");
    if (lecturas.Count(x => x.Interpretable) != GuionDelJuez.InterpretablesEsperadas) fallan.Add("guion: la salida sin etiquetas no se marcó como no interpretable");
    if (resultado.LlamadasAlJuez != 2 * casos.Count) fallan.Add($"guion: {resultado.LlamadasAlJuez} llamadas, se esperaban {2 * casos.Count}");
    var elegibles = CorridaDeEquivalencia.Elegibles(casos).Count();
    if (equivalencia.LlamadasAlJuez != elegibles) fallan.Add($"guion: Equivalence hizo {equivalencia.LlamadasAlJuez} llamadas, se esperaban {elegibles}");
    if (equivalencia.Lecturas.Count(l => l.Interpretable) != elegibles) fallan.Add("guion: alguna Equivalence guionada no quedó interpretable");
}
foreach (var f in fallan) Console.Error.WriteLine($"invariante que falla: {f}");
Console.WriteLine(fallan.Count == 0 ? "invariantes de código: se cumplen" : $"invariantes de código: fallan {fallan.Count}");
return argumentos.Verificar && fallan.Count > 0 ? 1 : 0;
