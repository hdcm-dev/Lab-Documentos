// Nuevo (ejemplo 14). Lectura de entradas.v1.json (estímulo «escrito para el ejemplo», congelado con su
// SHA-256 en ejemplo.json).
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente;

public sealed record ConteoEsperado(
    [property: JsonPropertyName("cantidad")] int? Cantidad,
    [property: JsonPropertyName("error")] bool Error);

public sealed record EntradaDeLlamada(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("descripcion")] string Descripcion,
    [property: JsonPropertyName("argumentos")] Dictionary<string, string> Argumentos,
    [property: JsonPropertyName("esperado")] ConteoEsperado Esperado,
    [property: JsonPropertyName("negativo")] bool Negativo = false);

public sealed record ArchivoDeEntradas(
    [property: JsonPropertyName("rotulo")] string Rotulo,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("entradas")] List<EntradaDeLlamada> Entradas);

public static class Entradas
{
    public const string Archivo = "entradas.v1.json";

    public static string Ruta => Path.Combine(AppContext.BaseDirectory, Archivo);

    public static ArchivoDeEntradas Leer(string? ruta = null) =>
        JsonSerializer.Deserialize<ArchivoDeEntradas>(File.ReadAllText(ruta ?? Ruta))
        ?? throw new InvalidDataException($"{Archivo} vacío.");
}
