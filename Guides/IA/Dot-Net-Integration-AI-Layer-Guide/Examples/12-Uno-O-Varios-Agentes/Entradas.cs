// Nuevo: lectura de las entradas congeladas (entradas.v1.json, «escrito para el ejemplo»; su SHA-256
// está en ejemplo.json).
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente;

/// <summary>Un pedido de resumen y los medios (claves de Catalogos) cuyo porcentaje tiene que aparecer.</summary>
public sealed record Entrada(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("medios")] List<string> Medios,
    [property: JsonPropertyName("texto")] string Texto);

public sealed record ArchivoDeEntradas(
    [property: JsonPropertyName("rotulo")] string Rotulo,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("entradas")] List<Entrada> Entradas)
{
    public static ArchivoDeEntradas Leer(string ruta) =>
        JsonSerializer.Deserialize<ArchivoDeEntradas>(File.ReadAllText(ruta))
        ?? throw new InvalidDataException($"{Path.GetFileName(ruta)} vacío");
}
