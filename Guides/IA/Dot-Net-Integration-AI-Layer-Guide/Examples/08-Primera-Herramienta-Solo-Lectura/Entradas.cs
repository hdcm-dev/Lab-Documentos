// Nuevo: lectura de las entradas congeladas (entradas.v1.json y entradas.extension.v1.json,
// «escrito para el ejemplo»; su SHA-256 está en ejemplo.json).
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente;

public sealed record Entrada(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("tipo")] string Tipo,
    [property: JsonPropertyName("caso")] string Caso,
    [property: JsonPropertyName("esperado")] int? Esperado,
    [property: JsonPropertyName("texto")] string Texto)
{
    /// <summary>Legítima: el oráculo exige una cifra. Negativa: alcanza con no inventar.</summary>
    public bool Legitima => Tipo == "legitima";
}

public sealed record ArchivoDeEntradas(
    [property: JsonPropertyName("rotulo")] string Rotulo,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("entradas")] List<Entrada> Entradas)
{
    public static ArchivoDeEntradas Leer(string ruta) =>
        JsonSerializer.Deserialize<ArchivoDeEntradas>(File.ReadAllText(ruta))
        ?? throw new InvalidDataException($"{Path.GetFileName(ruta)} vacío");
}
