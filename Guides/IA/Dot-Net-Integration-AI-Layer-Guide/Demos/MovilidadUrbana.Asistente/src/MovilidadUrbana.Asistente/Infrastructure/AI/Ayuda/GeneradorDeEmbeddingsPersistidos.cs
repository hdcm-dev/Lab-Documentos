// Nuevo. Delta: Agregado por la IA — embeddings persistidos en la fase 3a para correr PD-2 en la fase lan (plan §E.1, arnés).
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure.AI.Ayuda;

/// <summary>Archivo <c>RLLM_DIR/embeddings/PD-2.v1.json</c>: los vectores del corpus y de las preguntas.</summary>
public sealed record EmbeddingsPersistidos(
    [property: JsonPropertyName("modelo")] string Modelo,
    [property: JsonPropertyName("digest")] string? Digest,
    [property: JsonPropertyName("plantilla")] string Plantilla,
    [property: JsonPropertyName("dimensiones")] int Dimensiones,
    [property: JsonPropertyName("vectores")] IReadOnlyList<VectorPersistido> Vectores);

public sealed record VectorPersistido(
    [property: JsonPropertyName("texto")] string Texto,
    [property: JsonPropertyName("vector")] float[] Vector);

/// <summary>
/// La fase del chat (la fase <c>lan</c> del arnés, nombre histórico) no puede pedir a la vez el servidor
/// de embeddings de EMB-B, que es un servidor aparte (una fase no pide un segundo recurso mientras tiene uno). Por eso la fase 3a vectoriza el corpus y las preguntas
/// con <c>qwen3-embedding:0.6b</c> y los guarda; esta clase los sirve sin red. Un texto que no está en
/// el archivo es un error: nunca se inventa un vector.
/// </summary>
public sealed class GeneradorDeEmbeddingsPersistidos : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly Dictionary<string, float[]> _vectores;

    public GeneradorDeEmbeddingsPersistidos(EmbeddingsPersistidos archivo)
    {
        Archivo = archivo;
        _vectores = archivo.Vectores.ToDictionary(v => v.Texto, v => v.Vector, StringComparer.Ordinal);
    }

    public EmbeddingsPersistidos Archivo { get; }

    public static GeneradorDeEmbeddingsPersistidos Leer(string ruta) =>
        new(JsonSerializer.Deserialize<EmbeddingsPersistidos>(File.ReadAllText(ruta))
            ?? throw new InvalidOperationException($"El archivo de embeddings persistidos está vacío: {Path.GetFileName(ruta)}."));

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var resultado = new GeneratedEmbeddings<Embedding<float>>();
        foreach (var texto in values)
        {
            if (!_vectores.TryGetValue(texto, out var vector))
            {
                throw new InvalidOperationException("El texto no está entre los embeddings persistidos (rehacer la fase 3a con las mismas entradas).");
            }
            resultado.Add(new Embedding<float>(vector) { ModelId = Archivo.Modelo });
        }
        return Task.FromResult(resultado);
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType == typeof(EmbeddingGeneratorMetadata)
            ? new EmbeddingGeneratorMetadata("persistidos", null, Archivo.Modelo, Archivo.Dimensiones)
            : serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }
}
