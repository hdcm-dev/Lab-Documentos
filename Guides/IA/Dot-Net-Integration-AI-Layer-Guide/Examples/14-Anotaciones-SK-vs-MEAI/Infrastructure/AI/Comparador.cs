// Nuevo (ejemplo 14). El único archivo con `using Microsoft.Extensions.AI;` y `using Microsoft.SemanticKernel;`
// a la vez: el caso de quien migra de a poco. Los dos espacios de nombres definen FunctionCallContent
// (y TextContent, FunctionResultContent…), así que sin calificar el nombre la compilación falla con
// CS0104 «referencia ambigua» (N4 V01). Acá va calificado: Microsoft.Extensions.AI.FunctionCallContent.
//
// Qué se compara: (1) la FIRMA que ve el modelo (nombre, descripción y, por parámetro, nombre, tipo JSON,
// descripción y si es obligatorio), leída de los metadatos de cada marco y NO de las constantes del
// código; (2) el esquema JSON del parámetro, tal cual lo arma cada uno; (3) el resultado de invocar las
// dos con la misma FunctionCallContent. El resultado se compara NORMALIZADO (se lee como ConteoDto y se
// vuelve a escribir con las mismas opciones), porque cada marco lo serializa a su manera: M.E.AI lo
// devuelve ya serializado con AIJsonUtilities.DefaultOptions (indentado y sin las propiedades null) y SK
// devuelve el objeto. El texto crudo de cada uno se guarda aparte, para mostrarlo.
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

public sealed record ParametroDeFirma(
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("tipo")] string? Tipo,
    [property: JsonPropertyName("descripcion")] string? Descripcion,
    [property: JsonPropertyName("requerido")] bool Requerido);

/// <summary>Lo que el modelo ve de una herramienta. Igualdad por valor, parámetro por parámetro.</summary>
public sealed record FirmaDeHerramienta(
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("descripcion")] string? Descripcion,
    [property: JsonPropertyName("parametros")] IReadOnlyList<ParametroDeFirma> Parametros)
{
    public bool Equals(FirmaDeHerramienta? otra) =>
        otra is not null && Nombre == otra.Nombre && Descripcion == otra.Descripcion && Parametros.SequenceEqual(otra.Parametros);

    public override int GetHashCode() => HashCode.Combine(Nombre, Descripcion, Parametros.Count);
}

public static class Comparador
{
    private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>Firma de M.E.AI: sale del AIFunction (Name, Description y JsonSchema).</summary>
    public static FirmaDeHerramienta FirmaDe(AIFunction funcion)
    {
        var esquema = funcion.JsonSchema;
        var requeridos = esquema.TryGetProperty("required", out var r) ? r.EnumerateArray().Select(x => x.GetString()).ToHashSet() : [];
        var parametros = esquema.TryGetProperty("properties", out var p)
            ? p.EnumerateObject().Select(x => new ParametroDeFirma(
                x.Name,
                x.Value.TryGetProperty("type", out var t) ? t.GetString() : null,
                x.Value.TryGetProperty("description", out var d) ? d.GetString() : null,
                requeridos.Contains(x.Name))).ToList()
            : [];
        return new FirmaDeHerramienta(funcion.Name, funcion.Description, parametros);
    }

    /// <summary>Firma de SK: sale de KernelFunction.Metadata (lo que SK lee de [KernelFunction] y [Description]).</summary>
    public static FirmaDeHerramienta FirmaDe(KernelFunction funcion) =>
        new(funcion.Metadata.Name, funcion.Metadata.Description,
            [.. funcion.Metadata.Parameters.Select(x => new ParametroDeFirma(
                x.Name,
                x.Schema is { } s && s.RootElement.TryGetProperty("type", out var t) ? t.GetString() : null,
                x.Description,
                x.IsRequired))]);

    /// <summary>El esquema JSON del parámetro, como lo arma M.E.AI.</summary>
    public static JsonElement EsquemaDelParametro(AIFunction funcion, string parametro) =>
        funcion.JsonSchema.GetProperty("properties").GetProperty(parametro).Clone();

    /// <summary>El esquema JSON del parámetro, como lo arma SK.</summary>
    public static JsonElement? EsquemaDelParametro(KernelFunction funcion, string parametro) =>
        funcion.Metadata.Parameters.FirstOrDefault(x => x.Name == parametro)?.Schema?.RootElement.Clone();

    /// <summary>La FunctionCallContent que mandaría el modelo (calificada: también existe la de SK).</summary>
    public static Microsoft.Extensions.AI.FunctionCallContent Llamada(string callId, string nombre, IReadOnlyDictionary<string, string> argumentos) =>
        new(callId, nombre, argumentos.ToDictionary(kv => kv.Key, kv => (object?)kv.Value));

    /// <summary>Invoca la herramienta de M.E.AI con los argumentos de la llamada; devuelve el resultado como JSON.</summary>
    public static async Task<ResultadoDeInvocacion> InvocarAsync(AIFunction funcion, Microsoft.Extensions.AI.FunctionCallContent llamada, CancellationToken cancellationToken = default)
    {
        var resultado = await funcion.InvokeAsync(new AIFunctionArguments(llamada.Arguments), cancellationToken);
        return ResultadoDeInvocacion.De(resultado);
    }

    /// <summary>Invoca la función de SK con los mismos argumentos (KernelArguments); devuelve el resultado como JSON.</summary>
    public static async Task<ResultadoDeInvocacion> InvocarAsync(KernelFunction funcion, Kernel kernel, Microsoft.Extensions.AI.FunctionCallContent llamada, CancellationToken cancellationToken = default)
    {
        var argumentos = new KernelArguments();
        foreach (var (k, v) in llamada.Arguments ?? new Dictionary<string, object?>()) argumentos[k] = v;
        var resultado = await funcion.InvokeAsync(kernel, argumentos, cancellationToken);
        return ResultadoDeInvocacion.De(resultado.GetValue<object>());
    }
}

/// <summary>Lo que devolvió una invocación: el texto crudo (como lo serializó el marco) y el normalizado.</summary>
public sealed record ResultadoDeInvocacion(
    [property: JsonPropertyName("crudo")] string Crudo,
    [property: JsonPropertyName("normalizado")] JsonElement Normalizado)
{
    private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static ResultadoDeInvocacion De(object? valor)
    {
        var crudo = valor switch
        {
            JsonElement e => e.GetRawText(),
            null => "null",
            _ => JsonSerializer.Serialize(valor, valor.GetType(), Json)
        };
        // Normalizar: leerlo como el tipo de dominio y escribirlo siempre igual (con los null explícitos).
        var dto = JsonSerializer.Deserialize<ConteoDto>(crudo);
        return new(crudo, JsonSerializer.SerializeToElement(dto, Json));
    }
}
