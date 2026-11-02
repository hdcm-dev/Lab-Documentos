// Nuevo: prueba con modelo (Category LLM; el arnés no la corre en la fase build). Comprueba contra
// el modelo de embeddings real que la dimensión es la del índice (N3 C2) y que el índice arranca.
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun.AI;

namespace MovilidadUrbana.Asistente.Pruebas;

[Category("LLM")]
public class ConModeloTests
{
    private ConfiguracionDelModelo _config = null!;

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno().Validar();
        var r = await PredicadoDeSalto.EvaluarAsync(_config.EmbedEndpoint, _config.EmbedModelo, PredicadoDeSalto.Embeddings);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task ElModeloDeEmbeddings_DaLaDimensionDelIndice()
    {
        using var generador = FabricaDeClientes.CrearEmbeddingsOllama(_config.EmbedEndpoint, _config.EmbedModelo);
        var v = await generador.GenerateAsync(["Localidad · requisito del código postal: 4 dígitos."]);
        Assert.That(v[0].Vector.Length, Is.EqualTo(FragmentoIndexado.Dimensiones));
    }
}
