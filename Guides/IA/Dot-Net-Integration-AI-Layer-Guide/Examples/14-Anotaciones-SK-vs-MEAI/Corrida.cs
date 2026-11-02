// Nuevo (ejemplo 14). Arma las dos definiciones de contar_localidades sobre el MISMO LocalidadService y,
// por entrada, compara firma, esquema del parámetro y resultado de invocarlas con la misma
// FunctionCallContent. Sin modelo: el registro tiene sólo renglones «entrada» y lo comparado viaja
// entero en su motivo, como JSON, para que el oráculo se recalcule leyendo sólo el registro.
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;
using SK = Microsoft.SemanticKernel;

namespace MovilidadUrbana.Asistente;

/// <summary>Lo comparado en una entrada; viaja en el motivo del renglón «entrada».</summary>
public sealed record ComparacionDeEntrada(
    [property: JsonPropertyName("firma_meai")] FirmaDeHerramienta FirmaMeai,
    [property: JsonPropertyName("firma_sk")] FirmaDeHerramienta FirmaSk,
    [property: JsonPropertyName("esquema_parametro_meai")] JsonElement EsquemaParametroMeai,
    [property: JsonPropertyName("esquema_parametro_sk")] JsonElement? EsquemaParametroSk,
    [property: JsonPropertyName("nombre_sk_como_aifunction")] string NombreSkComoAIFunction,
    [property: JsonPropertyName("argumentos")] IReadOnlyDictionary<string, string> Argumentos,
    [property: JsonPropertyName("resultado_meai")] ResultadoDeInvocacion ResultadoMeai,
    [property: JsonPropertyName("resultado_sk")] ResultadoDeInvocacion ResultadoSk,
    [property: JsonPropertyName("esperado")] ConteoEsperado Esperado);

public sealed record ResultadoDeCorrida(IReadOnlyList<string> InvariantesQueFallan, IReadOnlyDictionary<string, string> Veredictos, string RutaDelRegistro);

/// <summary>Las dos definiciones, armadas sobre el mismo caso de uso.</summary>
public sealed record LasDosDefiniciones(AIFunction Meai, SK.KernelFunction Sk, SK.Kernel Kernel)
{
    public static LasDosDefiniciones Crear()
    {
        var localidades = new LocalidadService(new LocalidadesEnMemoria());
        var plugin = new PluginSk(localidades).Crear();
        var kernel = new SK.Kernel();
        kernel.Plugins.Add(plugin);
        return new(new HerramientaMeai(localidades).Crear(), plugin[HerramientaMeai.Nombre], kernel);
    }
}

public static class Corrida
{
    public const string Ejemplo = "14-Anotaciones-SK-vs-MEAI";
    public const string Oraculo = "anotaciones-coinciden";
    public const string Parametro = "provincia";

    private static readonly JsonSerializerOptions JsonMotivo = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static async Task<ComparacionDeEntrada> CompararAsync(LasDosDefiniciones d, EntradaDeLlamada e, CancellationToken cancellationToken = default)
    {
        var llamada = Comparador.Llamada($"call-{e.Id}", HerramientaMeai.Nombre, e.Argumentos);
        return new ComparacionDeEntrada(
            Comparador.FirmaDe(d.Meai), Comparador.FirmaDe(d.Sk),
            Comparador.EsquemaDelParametro(d.Meai, Parametro), Comparador.EsquemaDelParametro(d.Sk, Parametro),
            ((AIFunction)d.Sk).Name,
            e.Argumentos,
            await Comparador.InvocarAsync(d.Meai, llamada, cancellationToken),
            await Comparador.InvocarAsync(d.Sk, d.Kernel, llamada, cancellationToken),
            e.Esperado);
    }

    /// <summary>Los invariantes de código de una entrada (los de --verificar). Lista vacía: se cumplen.</summary>
    public static IReadOnlyList<string> Invariantes(string id, ComparacionDeEntrada c)
    {
        var fallan = new List<string>();
        if (!c.FirmaMeai.Equals(c.FirmaSk))
            fallan.Add($"{id}: las firmas difieren: M.E.AI {JsonSerializer.Serialize(c.FirmaMeai, JsonMotivo)} · SK {JsonSerializer.Serialize(c.FirmaSk, JsonMotivo)}");
        if (c.FirmaMeai.Nombre != HerramientaMeai.Nombre || c.FirmaMeai.Parametros.Count != 1 || c.FirmaMeai.Parametros[0].Nombre != Parametro)
            fallan.Add($"{id}: la firma no es contar_localidades(provincia): {c.FirmaMeai.Nombre}({string.Join(", ", c.FirmaMeai.Parametros.Select(p => p.Nombre))})");
        if (!JsonElement.DeepEquals(c.ResultadoMeai.Normalizado, c.ResultadoSk.Normalizado))
            fallan.Add($"{id}: los resultados difieren: M.E.AI {c.ResultadoMeai.Normalizado.GetRawText()} · SK {c.ResultadoSk.Normalizado.GetRawText()}");
        if (!CoincideConLoEsperado(c.ResultadoMeai.Normalizado, c.Esperado))
            fallan.Add($"{id}: el resultado {c.ResultadoMeai.Normalizado.GetRawText()} no es el esperado (cantidad {c.Esperado.Cantidad?.ToString() ?? "null"}, error {c.Esperado.Error})");
        return fallan;
    }

    /// <summary>Un JSON en una línea y con los acentos legibles, para la consola.</summary>
    public static string Una(string json) =>
        JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(json), JsonMotivo);

    private static bool CoincideConLoEsperado(JsonElement r, ConteoEsperado esperado)
    {
        if (r.ValueKind != JsonValueKind.Object) return false;
        int? cantidad = r.TryGetProperty("cantidad", out var c) && c.ValueKind == JsonValueKind.Number ? c.GetInt32() : null;
        var hayError = r.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String;
        return cantidad == esperado.Cantidad && hayError == esperado.Error;
    }

    public static async Task<ResultadoDeCorrida> CorrerAsync(ArgumentosDeEjemplo a, ConfiguracionDelModelo config, TextWriter salida)
    {
        var archivo = Entradas.Leer();
        var plazo = a.CrearPlazo();
        var fallan = new List<string>();
        var veredictos = new Dictionary<string, string>();
        var d = LasDosDefiniciones.Crear();

        using var registro = new EscritorDeRegistro(new ContextoDeCorrida
        {
            Ejemplo = Ejemplo,
            Configuracion = config,
            Adaptador = Adaptador.OllamaSharp,   // nominal: este ejemplo no llama a ningún modelo
            Modelo = null,
            EntradasVersion = archivo.Version,
            EntradasHash = Hashes.Sha256DeArchivo(Entradas.Ruta),
            PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
            Maquina = "sin-modelo"
        });

        salida.WriteLine("sin modelo: ninguna llamada a un IChatClient (el --fake no cambia nada en este ejemplo)");
        salida.WriteLine($"M.E.AI · nombre para el modelo «{d.Meai.Name}» · esquema {Una(d.Meai.JsonSchema.GetRawText())}");
        salida.WriteLine($"SK     · plugin «{d.Sk.PluginName}», función «{d.Sk.Metadata.Name}» · como AIFunction se llama «{((AIFunction)d.Sk).Name}» · esquema {Una(((AIFunction)d.Sk).JsonSchema.GetRawText())}");

        foreach (var e in archivo.Entradas)
        {
            if (plazo.Vencido)
            {
                registro.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = e.Id, Oraculo = Oraculo, Resultado = ResultadoDeEntrada.Excluida, Motivo = "plazo" });
                veredictos[e.Id] = ResultadoDeEntrada.Excluida;
                continue;
            }

            salida.WriteLine($"{e.Id}: {e.Descripcion}");
            using var tope = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var c = await CompararAsync(d, e, tope.Token);
            var inv = Invariantes(e.Id, c);
            fallan.AddRange(inv);
            var resultado = inv.Count == 0 ? ResultadoDeEntrada.Pasa : ResultadoDeEntrada.Falla;
            veredictos[e.Id] = resultado;

            salida.WriteLine($"  firma igual: {c.FirmaMeai.Equals(c.FirmaSk)} · esquema del parámetro igual: {c.EsquemaParametroSk is { } s && JsonElement.DeepEquals(c.EsquemaParametroMeai, s)}");
            salida.WriteLine($"  M.E.AI → {Una(c.ResultadoMeai.Crudo)}");
            salida.WriteLine($"  SK     → {Una(c.ResultadoSk.Crudo)}");
            salida.WriteLine($"  normalizados iguales: {JsonElement.DeepEquals(c.ResultadoMeai.Normalizado, c.ResultadoSk.Normalizado)}");
            salida.WriteLine($"  oráculo {Oraculo}: {resultado}");

            registro.Escribir(new RegistroDeEntrada
            {
                Ejemplo = Ejemplo, EntradaId = e.Id, Oraculo = Oraculo, Resultado = resultado,
                Motivo = JsonSerializer.Serialize(c, JsonMotivo)
            });
        }

        salida.WriteLine($"registro: {registro.RenglonesEscritos} renglones en {Path.GetFileName(registro.Ruta)}");
        return new ResultadoDeCorrida(fallan, veredictos, registro.Ruta);
    }
}
