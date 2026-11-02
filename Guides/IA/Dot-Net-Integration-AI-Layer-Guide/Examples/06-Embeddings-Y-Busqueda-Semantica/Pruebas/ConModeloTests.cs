// Nuevo: prueba con el modelo de embeddings. Se salta con su motivo si el Ollama propio no responde,
// no tiene el modelo o el modelo no declara la capacidad «embedding» (el 501 del compartido).
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun.AI;

namespace MovilidadUrbana.Asistente.Pruebas;

[Category("LLM")]
public class ConModeloTests
{
    private static readonly ConfiguracionDelModelo Config = ConfiguracionDelModelo.DesdeEntorno();

    [OneTimeSetUp]
    public async Task Predicado()
    {
        var r = await PredicadoDeSalto.EvaluarAsync(Config.EmbedEndpoint, Config.EmbedModelo, PredicadoDeSalto.Embeddings);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task DimensionDelModelo_CoincideConLaDelAtributo()
    {
        using var generador = FabricaDeClientes.CrearEmbeddingsOllama(Config.EmbedEndpoint, Config.EmbedModelo);
        var e = await generador.GenerateAsync([CorpusDePolicies.Fragmentos()[0].Texto]);
        Assert.That(e[0].Vector.Length, Is.EqualTo(CorridaDeBusqueda.Dimension));
    }
}
