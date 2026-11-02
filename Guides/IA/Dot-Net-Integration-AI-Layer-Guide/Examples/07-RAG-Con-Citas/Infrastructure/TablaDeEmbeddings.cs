// Nuevo (plan §E.1, fase 3a; §E.5 TURNO-3). Los embeddings de 07 se calculan en el Ollama propio,
// en su turno, y se guardan en RLLM_DIR/embeddings/07-RAG-Con-Citas.v1.json con el digest del
// modelo. La corrida de chat (turno modelo-compartido) los lee de ahí: no necesita el Ollama propio al
// mismo tiempo. Este generador devuelve el vector guardado del texto exacto, y falla si no está.
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure;

public sealed record VectorGuardado(
    [property: JsonPropertyName("texto")] string Texto,
    [property: JsonPropertyName("vector")] float[] Vector);

public sealed record ArchivoDeEmbeddings
{
    [JsonPropertyName("rotulo")] public string Rotulo { get; init; } = "precomputado en la fase 3a (Ollama propio); textos: corpus plantilla.v1 y preguntas del oro, escritas para el ejemplo";
    [JsonPropertyName("pieza")] public required string Pieza { get; init; }
    [JsonPropertyName("version")] public string Version { get; init; } = "v1";
    [JsonPropertyName("plantilla")] public required string Plantilla { get; init; }
    [JsonPropertyName("modelo")] public required string Modelo { get; init; }
    [JsonPropertyName("digest")] public string? Digest { get; init; }
    [JsonPropertyName("ollama_version")] public string? OllamaVersion { get; init; }
    [JsonPropertyName("oro_sha256")] public required string OroSha256 { get; init; }
    [JsonPropertyName("dimension")] public int Dimension { get; init; }
    [JsonPropertyName("fecha")] public DateTimeOffset Fecha { get; init; } = DateTimeOffset.UtcNow;
    [JsonPropertyName("vectores")] public required IReadOnlyList<VectorGuardado> Vectores { get; init; }

    public static string Ruta(string registroDir, string pieza) => Path.Combine(registroDir, "embeddings", $"{pieza}.v1.json");

    public void Guardar(string ruta)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        File.WriteAllText(ruta, JsonSerializer.Serialize(this));
    }

    public static ArchivoDeEmbeddings Leer(string ruta) =>
        JsonSerializer.Deserialize<ArchivoDeEmbeddings>(File.ReadAllText(ruta)) ?? throw new InvalidDataException($"{ruta} vacío");
}

public sealed class TablaDeEmbeddings : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly Dictionary<string, float[]> _vectores;
    private readonly ArchivoDeEmbeddings _archivo;

    public TablaDeEmbeddings(ArchivoDeEmbeddings archivo)
    {
        _archivo = archivo;
        _vectores = archivo.Vectores.ToDictionary(v => v.Texto, v => v.Vector, StringComparer.Ordinal);
    }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(values.Select(v =>
            _vectores.TryGetValue(v, out var vector)
                ? new Embedding<float>(vector) { ModelId = _archivo.Modelo }
                : throw new InvalidOperationException($"Texto sin embedding precomputado (correr la fase 3a): «{v}»"))));

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType == typeof(EmbeddingGeneratorMetadata)
            ? new EmbeddingGeneratorMetadata("tabla-precomputada", null, _archivo.Modelo, _archivo.Dimension)
            : serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }
}
