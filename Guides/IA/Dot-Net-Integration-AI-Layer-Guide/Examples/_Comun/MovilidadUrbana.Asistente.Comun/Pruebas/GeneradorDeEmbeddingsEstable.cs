// Nuevo. Reemplaza al HashEmbeddingGenerator de N6 E1, que usaba string.GetHashCode(): ese hash
// cambia entre corridas (N6 D5, F19). FNV-1a de 32 bits sobre UTF-8 da el mismo vector siempre.
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Comun.Pruebas;

/// <summary>
/// Embeddings de bolsa de palabras con hash estable. Sirve para probar cableado y métricas, NO
/// calidad de recuperación: dos textos se parecen sólo si comparten palabras.
/// </summary>
public sealed partial class GeneradorDeEmbeddingsEstable(int dimensiones = 1024) : IEmbeddingGenerator<string, Embedding<float>>
{
    public int Dimensiones { get; } = dimensiones;

    public static uint Fnv1a(string texto)
    {
        const uint base32 = 2166136261;
        const uint primo = 16777619;
        var hash = base32;
        foreach (var b in Encoding.UTF8.GetBytes(texto))
        {
            hash ^= b;
            hash *= primo;
        }
        return hash;
    }

    public float[] Vector(string texto)
    {
        var vector = new float[Dimensiones];
        foreach (var palabra in Palabras().Split(texto.ToLowerInvariant()).Where(p => p.Length > 2))
        {
            vector[(int)(Fnv1a(palabra) % (uint)Dimensiones)] += 1;
        }
        var norma = MathF.Sqrt(vector.Sum(x => x * x));
        if (norma > 0)
        {
            for (var i = 0; i < vector.Length; i++) vector[i] /= norma;
        }
        return vector;
    }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(values.Select(v => new Embedding<float>(Vector(v)) { ModelId = "fnv1a-estable" })));

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType == typeof(EmbeddingGeneratorMetadata)
            ? new EmbeddingGeneratorMetadata("fnv1a-estable", null, "fnv1a-estable", Dimensiones)
            : serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }

    [GeneratedRegex(@"\W+")]
    private static partial Regex Palabras();
}
