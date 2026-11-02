// Nuevo, escrito para el ejemplo: lectura de entradas.v1.json / entradas.ext.v1.json (congeladas
// con su SHA-256 en ejemplo.json; cambiar una crea v2).
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente;

public sealed record Entrada(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("texto")] string Texto,
    [property: JsonPropertyName("trae_habitantes")] bool TraeHabitantes,
    [property: JsonPropertyName("provincia")] string Provincia);

public sealed record ArchivoDeEntradas(
    [property: JsonPropertyName("rotulo")] string Rotulo,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("entradas")] IReadOnlyList<Entrada> Entradas)
{
    public static ArchivoDeEntradas Leer(string ruta) =>
        JsonSerializer.Deserialize<ArchivoDeEntradas>(File.ReadAllText(ruta))
        ?? throw new InvalidDataException($"No se pudo leer {Path.GetFileName(ruta)}.");
}
