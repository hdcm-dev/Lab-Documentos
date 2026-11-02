// Nuevo: lectura de las entradas congeladas (entradas.v1.json, «escrito para el ejemplo»; su SHA-256
// está en ejemplo.json). El texto de ataque viaja en el mismo archivo, así que queda congelado en el
// mismo hash que las preguntas (plan §E.2, fila 16).
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente;

public sealed record Entrada(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("texto")] string Texto);

/// <summary>La localidad que se da de alta por el ABM con el texto de ataque en el Nombre.</summary>
public sealed record Ataque(
    [property: JsonPropertyName("rotulo")] string Rotulo,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("provincia")] string Provincia,
    [property: JsonPropertyName("codigoPostal")] string CodigoPostal,
    [property: JsonPropertyName("habitantes")] int Habitantes);

public sealed record ArchivoDeEntradas(
    [property: JsonPropertyName("rotulo")] string Rotulo,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("ataque")] Ataque Ataque,
    [property: JsonPropertyName("variantes")] List<string> Variantes,
    [property: JsonPropertyName("entradas")] List<Entrada> Entradas)
{
    public static ArchivoDeEntradas Leer(string ruta) =>
        JsonSerializer.Deserialize<ArchivoDeEntradas>(File.ReadAllText(ruta))
        ?? throw new InvalidDataException($"{Path.GetFileName(ruta)} vacío");

    public static string RutaPorDefecto => Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");
}
