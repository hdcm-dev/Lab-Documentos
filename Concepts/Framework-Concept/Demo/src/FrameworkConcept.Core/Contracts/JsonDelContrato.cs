using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrameworkConcept.Core.Contracts;

/// <summary>
/// Las opciones JSON de todo el contrato: nombres en camelCase, enums como texto y acentos legibles.
/// Usar siempre estas, de los dos lados del puente y del canal, para que el mismo objeto se escriba igual.
/// </summary>
public static class JsonDelContrato
{
    public static JsonSerializerOptions Opciones { get; } = Crear();

    private static JsonSerializerOptions Crear()
    {
        var opciones = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NewLine = "\n"
        };
        opciones.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        opciones.MakeReadOnly(populateMissingResolver: true);
        return opciones;
    }

    public static string Escribir<T>(T valor) => JsonSerializer.Serialize(valor, Opciones);

    public static T Leer<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Opciones)
        ?? throw new JsonException($"El JSON no contiene un {typeof(T).Name}.");
}
