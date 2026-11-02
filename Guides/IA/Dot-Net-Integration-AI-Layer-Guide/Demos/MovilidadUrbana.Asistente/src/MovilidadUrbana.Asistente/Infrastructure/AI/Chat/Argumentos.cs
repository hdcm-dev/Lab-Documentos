// Nuevo. Delta: Agregado por la IA — lectura tolerante de los argumentos de una llamada a herramienta.
using System.Globalization;
using System.Text.Json;

namespace MovilidadUrbana.Asistente.Infrastructure.AI.Chat;

/// <summary>
/// Los argumentos llegan como <see cref="JsonElement" /> desde el proveedor y como objetos desde el
/// cliente guionado. Números con cultura invariante (T6).
/// </summary>
public static class Argumentos
{
    public static string? Texto(IDictionary<string, object?>? argumentos, string nombre) =>
        argumentos is not null && argumentos.TryGetValue(nombre, out var v) ? v switch
        {
            null => null,
            JsonElement { ValueKind: JsonValueKind.String } e => e.GetString(),
            JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } => null,
            JsonElement e => e.GetRawText(),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => v.ToString()
        } : null;

    public static int? Entero(IDictionary<string, object?>? argumentos, string nombre)
    {
        var texto = Texto(argumentos, nombre);
        return int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
    }

    /// <summary>Forma canónica para comparar dos juegos de argumentos (prueba de argumentos alterados).</summary>
    public static string Canonico(IDictionary<string, object?>? argumentos) =>
        string.Join("\u001f", (argumentos ?? new Dictionary<string, object?>())
            .OrderBy(a => a.Key, StringComparer.Ordinal)
            .Select(a => $"{a.Key}={Texto(argumentos, a.Key)}"));
}
