// Nuevo: la corrida contra el modelo como prueba. Se salta con el motivo del predicado si no hay
// servidor o el modelo no está (plan §F.8). Sólo afirma las garantías de código: la conducta del
// modelo es descriptiva y queda en el registro R-LLM.
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;

namespace MovilidadUrbana.Asistente.Pruebas;

[Category("LLM")]
public class ConModeloTests
{
    private ConfiguracionDelModelo _config = null!;

    [OneTimeSetUp]
    public async Task Predicado()
    {
        try
        {
            _config = ConfiguracionDelModelo.DesdeEntorno().Validar();
        }
        catch (ConfiguracionInvalidaException e)
        {
            Assert.Ignore(e.Message);
        }
        var r = await PredicadoDeSalto.EvaluarAsync(_config.Endpoint, _config.Modelo, PredicadoDeSalto.Chat);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task ConElModelo_LasDosVariantesCumplenLasGarantias()
    {
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--verificar"]), _config, TestContext.Out);
        Assert.That(r.InvariantesQueFallan, Is.Empty);
        Assert.That(r.Costos, Has.Count.EqualTo(2));
    }
}
