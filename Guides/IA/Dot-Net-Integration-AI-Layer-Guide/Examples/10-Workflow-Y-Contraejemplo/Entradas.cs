// Nuevo (ejemplo 10). Lectura de entradas.v1.json (estímulo «escrito para el ejemplo», congelado con su
// SHA-256 en ejemplo.json).
using System.Text.Json;
using System.Text.Json.Serialization;
using MovilidadUrbana.Asistente.Application.Encuestas;

namespace MovilidadUrbana.Asistente;

public sealed record EntradaDeEncuesta(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("descripcion")] string Descripcion,
    [property: JsonPropertyName("esperado")] string Esperado,
    [property: JsonPropertyName("respuestas")] List<RespuestaDePaso> Respuestas);

public sealed record ArchivoDeEntradas(
    [property: JsonPropertyName("rotulo")] string Rotulo,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("entradas")] List<EntradaDeEncuesta> Entradas);

public static class Entradas
{
    public const string Archivo = "entradas.v1.json";

    public static string Ruta => Path.Combine(AppContext.BaseDirectory, Archivo);

    public static ArchivoDeEntradas Leer(string? ruta = null) =>
        JsonSerializer.Deserialize<ArchivoDeEntradas>(File.ReadAllText(ruta ?? Ruta))
        ?? throw new InvalidDataException($"{Archivo} vacío.");
}
