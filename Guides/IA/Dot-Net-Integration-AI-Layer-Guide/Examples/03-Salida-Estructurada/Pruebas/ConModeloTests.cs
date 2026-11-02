// Nuevo: prueba con modelo (Category LLM). Se salta con motivo si el servidor no responde o el
// modelo no está. Afirma la garantía del código, nunca un texto literal ni la conducta del modelo.
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Persistencia;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

[Category("LLM")]
public class ConModeloTests
{
    private ConfiguracionDelModelo _config = null!;

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno().Validar();
        var r = await PredicadoDeSalto.EvaluarAsync(_config.Endpoint, _config.Modelo, PredicadoDeSalto.Chat);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task SinElDato_NadaInventadoLlegaALasReglas()
    {
        const string texto = "En Bella Vista, Corrientes, CP 3432, no sé cuántos viven";
        var cliente = FabricaDeClientes.CrearChatOllama(_config.Endpoint, _config.Modelo, _config);
        var alta = new AltaDesdeTextoService(new ExtractorDeLocalidadConModelo(cliente),
            new LocalidadService(new RepositorioDeLocalidadesEnMemoria()));

        var decision = await alta.ProponerAsync(texto);

        Assert.That(decision.Modelo.Habitantes, Is.Null, "un número que no está en el texto no pasa la garantía");
        Assert.That(decision.Estado, Is.Not.EqualTo(EstadoDeLaPropuesta.Lista));
    }
}
