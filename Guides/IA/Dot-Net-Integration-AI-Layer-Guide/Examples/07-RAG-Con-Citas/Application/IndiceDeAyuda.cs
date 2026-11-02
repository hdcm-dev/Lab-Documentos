// Nuevo (plan §E.2, fila 07; §F.6; guía §7.3). El almacén vectorial en memoria, reconstruido al
// arrancar desde los 12 literales de *Policy (corpus de 06, plantilla.v1). Depende sólo de
// IEmbeddingGenerator: el código no cambia entre el modelo de embeddings real, la tabla
// precomputada en la fase 3a y el generador estable del modo --fake (§F.6).
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>Registro del almacén. <see cref="Fuente" /> es el id calificado por clase: lo que se cita.</summary>
public sealed class FragmentoIndexado
{
    public const int Dimensiones = 1024; // qwen3-embedding:0.6b; el generador estable usa la misma (N3 C2)

    [VectorStoreKey] public string Fuente { get; set; } = "";
    [VectorStoreData] public string Literal { get; set; } = "";
    [VectorStoreData] public string Texto { get; set; } = "";
    [VectorStoreVector(Dimensiones, DistanceFunction = DistanceFunction.CosineSimilarity)] public string Vectorizado { get; set; } = "";
}

public sealed record Recuperacion(string Fuente, string Texto, double Puntaje);

public sealed class IndiceDeAyuda
{
    public const string Coleccion = "ayuda";
    private readonly VectorStoreCollection<string, FragmentoIndexado> _coleccion;

    private IndiceDeAyuda(VectorStoreCollection<string, FragmentoIndexado> coleccion) => _coleccion = coleccion;

    public static async Task<IndiceDeAyuda> CrearAsync(
        IEmbeddingGenerator<string, Embedding<float>> embeddings, CancellationToken cancellationToken = default)
    {
        var dimension = embeddings.GetService<EmbeddingGeneratorMetadata>()?.DefaultModelDimensions;
        if (dimension is { } d && d != FragmentoIndexado.Dimensiones)
        {
            throw new InvalidOperationException($"El generador da vectores de {d}; el índice espera {FragmentoIndexado.Dimensiones}.");
        }
        var store = new InMemoryVectorStore(new() { EmbeddingGenerator = embeddings });
        var coleccion = store.GetCollection<string, FragmentoIndexado>(Coleccion);
        await coleccion.EnsureCollectionExistsAsync(cancellationToken);
        await coleccion.UpsertAsync(CorpusDePolicies.Fragmentos().Select(f => new FragmentoIndexado
        {
            Fuente = f.Id, Literal = f.Literal, Texto = f.Texto, Vectorizado = f.Texto
        }), cancellationToken);
        return new IndiceDeAyuda(coleccion);
    }

    public async Task<IReadOnlyList<Recuperacion>> BuscarAsync(string pregunta, int top, CancellationToken cancellationToken = default)
    {
        var hallados = new List<Recuperacion>();
        await foreach (var r in _coleccion.SearchAsync(pregunta, top, cancellationToken: cancellationToken))
        {
            hallados.Add(new(r.Record.Fuente, r.Record.Texto, Math.Round(r.Score ?? 0, 6)));
        }
        return hallados;
    }
}
