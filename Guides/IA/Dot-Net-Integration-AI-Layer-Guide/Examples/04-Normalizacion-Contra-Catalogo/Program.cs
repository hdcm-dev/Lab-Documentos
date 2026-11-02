// Nuevo: ejemplo 04 (§5.3 · OA-D2). El modelo propone texto; el adaptador lo normaliza a claves de
// Catalogos; ValidarPaso decide. Un medio inventado («avion») se rechaza con error, nunca se
// registra como lista vacía (hueco H4 del caso base, cerrado en _Comun).
//
//   dotnet run -- --fake [--verificar]         guion, sin red
//   dotnet run -- --verificar [--plazo-s N]    contra el modelo (OLLAMA_ENDPOINT, OLLAMA_MODEL)
//   dotnet run -- ... --extension              las 8 entradas de la extensión de medición
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente;
using MovilidadUrbana.Asistente.Application.Normalizacion;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using OllamaSharp;

const string Ejemplo = "04-Normalizacion-Contra-Catalogo";

var a = ArgumentosDeEjemplo.Leer(args);
var extension = a.Resto.Contains("--extension");
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

var rutaEntradas = Path.Combine(AppContext.BaseDirectory, extension ? "entradas.extension.v1.json" : "entradas.v1.json");
var entradas = ArchivoDeEntradas.Leer(rutaEntradas).Entradas;
var oraculo = extension ? Oraculos.Esperada : Oraculos.Cerrada;

// Cliente: siempre por la capa única. En --fake, el guion responde lo que haría el modelo; para d02
// propone «avion», que no es un medio del catálogo. En los dos modos el espía queda DEBAJO de la capa,
// pegado al cliente base: lo que va al registro (num_ctx, razonamiento, temperatura) es lo que la
// llamada llevó de verdad, no un recálculo. Contra el modelo es la misma composición que
// FabricaDeClientes.CrearChatOllama, con el espía en el medio.
IChatClient baseDelModelo = a.Fake
    ? new GuionChatClient(entradas.Select(e => PasoDelGuion.Texto(Guion.Para(e, extension))))
    : new OllamaApiClient(FabricaDeClientes.CrearHttp(config.Endpoint), config.Modelo);
var espia = new ClienteEspia(baseDelModelo);
IChatClient cliente = FabricaDeClientes.Envolver(espia, config, Adaptador.OllamaSharp);

string? version = null, digest = null;
if (!a.Fake)
{
    var p = await PredicadoDeSalto.EvaluarAsync(config.Endpoint, config.Modelo, ["completion"]);
    if (p.Saltar)
    {
        Console.Error.WriteLine($"No se corre contra el modelo: {p.Motivo}");
        return 1;
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

var caso = new NormalizacionDeMovilidad(new NormalizadorDeCatalogo());
var plazo = a.CrearPlazo();
var invariantes = new List<string>();
int pasan = 0, validas = 0;

foreach (var entrada in entradas)
{
    if (plazo.Vencido)
    {
        reg.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = entrada.Id, Oraculo = oraculo, Resultado = ResultadoDeEntrada.Excluida, Motivo = "plazo" });
        Console.WriteLine($"{entrada.Id}: excluida (plazo)");
        continue;
    }

    string? frio = null;
    if (!a.Fake && config.Servidor == TipoDeServidor.Compartido &&
        await PredicadoDeSalto.ModeloCargadoAsync(config.Endpoint, config.Modelo) == false)
    {
        frio = "frio";
    }

    List<ChatMessage> mensajes = [new(ChatRole.User, entrada.Texto)];
    var opciones = new ChatOptions { Instructions = Instrucciones.Texto };
    var reloj = EscritorDeRegistro.Cronometro();
    var respuesta = await cliente.GetResponseAsync<PropuestaDeMovilidad>(mensajes, opciones);
    reloj.Stop();

    // Las opciones que vio el espía en esta llamada (observadas, en --fake y contra el modelo).
    var efectivas = espia.OpcionesVistas[^1];
    var linea = reg.Llamada(entrada.Id, 1, mensajes, efectivas, respuesta, reloj.Elapsed, paso: "proponer", estadoCacheForzado: frio);
    reg.Escribir(linea);
    if (linea.Razonamiento != "None" || linea.NumCtx is not > 0)
    {
        invariantes.Add($"{entrada.Id}: la llamada no salió con razonamiento apagado y num_ctx explícito");
    }

    // TryGetResult: si el texto no es el JSON pedido, no hay propuesta (y el paso queda con error).
    var propuesta = respuesta.TryGetResult(out var r) ? r : null;
    var resultado = caso.Decidir(propuesta);
    invariantes.AddRange(Garantia.Comprobar(entrada.Id, resultado));

    var pasa = extension ? Oraculos.CoincideConEsperado(resultado, entrada.Esperado) : resultado.Valido;
    var veredicto = linea.TruncadoPosible == true ? ResultadoDeEntrada.Excluida : pasa ? ResultadoDeEntrada.Pasa : ResultadoDeEntrada.Falla;
    reg.Escribir(new RegistroDeEntrada
    {
        Ejemplo = Ejemplo,
        EntradaId = entrada.Id,
        Oraculo = oraculo,
        Resultado = veredicto,
        Motivo = linea.TruncadoPosible == true ? "truncado_posible" : null
    });
    if (veredicto != ResultadoDeEntrada.Excluida) validas++;
    if (veredicto == ResultadoDeEntrada.Pasa) pasan++;

    Console.WriteLine($"{entrada.Id}: «{entrada.Texto}»");
    Console.WriteLine($"  propuesta {(a.Fake ? "del guion" : "del modelo")}: {respuesta.Text}");
    Console.WriteLine($"  claves: medios [{string.Join(", ", resultado.Modelo.Medios.Order(StringComparer.Ordinal))}] · frecuencia «{resultado.Modelo.Frecuencia}»");
    Console.WriteLine(resultado.Errores.Count == 0
        ? "  ValidarPaso(2): sin errores"
        : $"  ValidarPaso(2): {string.Join(" · ", resultado.Errores.Select(e => $"{e.Key}: {e.Value}"))}");
    Console.WriteLine($"  oráculo {oraculo}: {veredicto}");
}

// Garantía fuera del modelo: una propuesta con «avion» siempre produce el error «medios».
invariantes.AddRange(Garantia.Comprobar("avion", caso.Decidir(new PropuestaDeMovilidad { Medios = ["avion"], Frecuencia = "diaria" }), exigirErrorDeMedios: true));
if (a.Fake && !extension)
{
    // El guion de d02 propone «avion»: tiene que salir con error de medios.
    var d02 = caso.Decidir(new PropuestaDeMovilidad { Medios = ["avion"], Frecuencia = "algunos días" });
    if (!d02.Errores.ContainsKey("medios")) invariantes.Add("d02 (guion): «avion» no produjo error de medios");
}

Console.WriteLine(extension
    ? $"extensión: {pasan}/{validas} válidas con normalización correcta (estímulo del mismo autor; descriptiva, §10.4)"
    : $"demostración: {pasan} de {entradas.Count} entradas con todas sus claves en Catalogos (sin tasa)");
Console.WriteLine($"registro: {reg.RenglonesEscritos} líneas en {Path.GetFileName(reg.Ruta)}");
foreach (var i in invariantes) Console.Error.WriteLine($"INVARIANTE: {i}");
Console.WriteLine(invariantes.Count == 0 ? "invariantes de código: se cumplen" : $"invariantes de código: {invariantes.Count} fallan");
return a.Verificar && invariantes.Count > 0 ? 1 : 0;

namespace MovilidadUrbana.Asistente
{
    public static class Instrucciones
    {
        public const string Texto = """
            Leés la frase de una persona que responde una encuesta de movilidad urbana.
            Devolvé sólo JSON con dos campos:
            "medios": la lista de medios de transporte que la persona nombra, con sus palabras;
            "frecuencia": con qué frecuencia viaja, con sus palabras, o null si no lo dice.
            No agregues medios que la persona no nombre.
            """;
    }

    public static class Oraculos
    {
        /// <summary>Demostración: todas las claves resultantes están en Catalogos y el paso no tiene errores.</summary>
        public const string Cerrada = "normalizacion-cerrada";
        /// <summary>Extensión: las claves resultantes son las esperadas para esa entrada.</summary>
        public const string Esperada = "normalizacion-esperada";

        public static bool CoincideConEsperado(ResultadoDeNormalizacion r, Esperado e) =>
            r.Valido && r.Modelo.Medios.SetEquals(e.Medios) && r.Modelo.Frecuencia == e.Frecuencia;
    }

    /// <summary>Las garantías de código que comprueba --verificar (plan §E.2, fila 04).</summary>
    public static class Garantia
    {
        public static IEnumerable<string> Comprobar(string id, ResultadoDeNormalizacion r, bool exigirErrorDeMedios = false)
        {
            var fuera = r.Modelo.Medios.Where(m => !Catalogos.Medios.Any(c => c.Clave == m)).ToList();
            if (fuera.Count > 0 && !r.Errores.ContainsKey("medios"))
                yield return $"{id}: medios fuera de Catalogos sin error ({string.Join(", ", fuera)})";
            if (r.Modelo.Medios.Count == 0 && !r.Errores.ContainsKey("medios"))
                yield return $"{id}: lista de medios vacía sin error";
            if (r.Modelo.Frecuencia.Length > 0 && !Catalogos.Frecuencias.Any(f => f.Clave == r.Modelo.Frecuencia) && !r.Errores.ContainsKey("frecuencia"))
                yield return $"{id}: frecuencia fuera de Catalogos sin error";
            if (exigirErrorDeMedios && !r.Errores.ContainsKey("medios"))
                yield return $"{id}: se esperaba error de medios";
        }
    }

    public static class Guion
    {
        /// <summary>Lo que «propone» el guion: d01 con sinónimos, d02 con un medio inventado.</summary>
        public static string Para(Entrada e, bool extension) => (e.Id, extension) switch
        {
            ("d01", false) => """{"medios":["bondi","bici"],"frecuencia":"casi todos los días"}""",
            ("d02", false) => """{"medios":["avion"],"frecuencia":"algunos días"}""",
            _ => System.Text.Json.JsonSerializer.Serialize(new PropuestaDeMovilidad
            {
                Medios = [.. e.Esperado.Medios.Select(Catalogos.EtiquetaDeMedio)],
                Frecuencia = Catalogos.EtiquetaDeFrecuencia(e.Esperado.Frecuencia)
            })
        };
    }
}
