// Nuevo. Fase 3a (turno modelo-propio): vectoriza el corpus y la pregunta de PD-2 con el modelo de
// embeddings y los persiste en RLLM_DIR/embeddings/PD-2.v1.json, con el digest. La fase lan los lee
// (una fase no pide un segundo recurso mientras tiene uno).
using System.Text.Json;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Infrastructure.AI.Ayuda;

namespace MovilidadUrbana.Asistente.Tests.ConModelo;

[TestFixture]
[Category("LLM")]
public class EmbeddingsDePd2
{
    private ConfiguracionDelModelo _config = default!;
    private ResultadoDelPredicado _predicado = default!;

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno().Validar();
        _predicado = await PredicadoDeSalto.EvaluarAsync(_config.EmbedEndpoint, _config.EmbedModelo, PredicadoDeSalto.Embeddings);
        if (_predicado.Saltar) Assert.Ignore(_predicado.Motivo);
    }

    [Test]
    [Description("Persiste los 12 fragmentos y la pregunta de PD-2, de 1024 dimensiones")]
    public async Task Persistir()
    {
        var textos = CorpusDeAyuda.Fragmentos().Select(f => f.Texto)
            .Concat(Entradas.Todas().Where(e => e.Paso == "PD-2").Select(e => e.Texto)).ToList();
        using var generador = FabricaDeClientes.CrearEmbeddingsOllama(_config.EmbedEndpoint, _config.EmbedModelo);
        var vectores = await generador.GenerateAsync(textos);

        Assert.That(vectores.Select(v => v.Vector.Length), Is.All.EqualTo(RegistroDeAyuda.Dimensiones));
        var archivo = new EmbeddingsPersistidos(_config.EmbedModelo, _predicado.Digest, CorpusDeAyuda.Plantilla, RegistroDeAyuda.Dimensiones,
            textos.Select((t, i) => new VectorPersistido(t, vectores[i].Vector.ToArray())).ToList());
        var dir = Path.Combine(_config.RegistroDir, "embeddings");
        Directory.CreateDirectory(dir);
        var ruta = Path.Combine(dir, "PD-2.v1.json");
        await File.WriteAllTextAsync(ruta, JsonSerializer.Serialize(archivo));
        TestContext.Out.WriteLine($"embeddings: {textos.Count} textos, {_config.EmbedModelo}@{_predicado.Digest}, Ollama {_predicado.VersionDeOllama}, entradas {Entradas.Hash[..12]}");
    }
}
