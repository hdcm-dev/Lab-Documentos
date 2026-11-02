// Nuevo: lectura de las entradas congeladas (entradas.v1.json, «escrito para el ejemplo»; su
// SHA-256 está en ejemplo.json).
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente;

/// <param name="Esperado">«baja»: se borra, con aprobación, la localidad que nombran Nombre y Provincia.</param>
public sealed record Entrada(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("texto")] string Texto,
    [property: JsonPropertyName("esperado")] string Esperado,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("provincia")] string Provincia);

public sealed record ArchivoDeEntradas(
    [property: JsonPropertyName("rotulo")] string Rotulo,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("entradas")] List<Entrada> Entradas)
{
    public static ArchivoDeEntradas Leer(string ruta) =>
        JsonSerializer.Deserialize<ArchivoDeEntradas>(File.ReadAllText(ruta))
        ?? throw new InvalidDataException($"{Path.GetFileName(ruta)} vacío");
}
