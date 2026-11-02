// Nuevo: lectura del conjunto de oro congelado (plan §E.2; casa conceptual: guía §10.7). Escrito para
// el ejemplo; su SHA-256 va en ejemplo.json y en cada línea del registro.
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente.Application;

public sealed record PreguntaDeOro
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    /// <summary><c>respondible</c> | <c>no-respondible</c>.</summary>
    [JsonPropertyName("tipo")] public required string Tipo { get; init; }
    [JsonPropertyName("objetivo")] public string? Objetivo { get; init; }
    [JsonPropertyName("cercania")] public string? Cercania { get; init; }
    [JsonPropertyName("texto")] public required string Texto { get; init; }
    [JsonPropertyName("relevantes")] public IReadOnlyList<string> Relevantes { get; init; } = [];
    [JsonPropertyName("valor_esperado")] public string? ValorEsperado { get; init; }
    [JsonPropertyName("formas_aceptadas")] public IReadOnlyList<IReadOnlyList<string>> FormasAceptadas { get; init; } = [];
    [JsonPropertyName("sin_valor_verificable")] public bool SinValorVerificable { get; init; }
    [JsonPropertyName("valores_de_catalogo")] public IReadOnlyList<string> ValoresDeCatalogo { get; init; } = [];

    public bool Respondible => Tipo == "respondible";
}

public sealed record ConjuntoDeOro
{
    public const string Archivo = "oro/conjunto-de-oro.v1.json";

    [JsonPropertyName("rotulo")] public required string Rotulo { get; init; }
    [JsonPropertyName("version")] public required string Version { get; init; }
    [JsonPropertyName("plantilla")] public required string Plantilla { get; init; }
    [JsonPropertyName("preguntas")] public required IReadOnlyList<PreguntaDeOro> Preguntas { get; init; }

    public static string RutaPorDefecto => Path.Combine(AppContext.BaseDirectory, Archivo);

    public static ConjuntoDeOro Leer(string? ruta = null) =>
        JsonSerializer.Deserialize<ConjuntoDeOro>(File.ReadAllText(ruta ?? RutaPorDefecto))
        ?? throw new InvalidDataException($"{Archivo} vacío");
}
