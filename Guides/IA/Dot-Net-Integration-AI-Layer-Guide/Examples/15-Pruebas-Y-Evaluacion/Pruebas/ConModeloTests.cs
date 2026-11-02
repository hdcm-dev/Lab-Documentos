// Nuevo: prueba con modelo (Category LLM; el arnés no la corre en la fase build). Contra el juez del
// Ollama propio: el evaluador vuelve SIEMPRE con un puntaje de 1 a 5 o con un diagnóstico de error,
// nunca con una excepción, en una sola llamada. Es una aserción de forma, no de calidad del juez
// (eso lo dice la calibración de la consola).
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;

namespace MovilidadUrbana.Asistente.Pruebas;

[Category("LLM")]
public class ConModeloTests
{
    private ConfiguracionDelModelo _config = null!;

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno().Validar();
        var r = await PredicadoDeSalto.EvaluarAsync(_config.PropioEndpoint, _config.PropioModelo, ["completion"]);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task Juez_DevuelvePuntajeODiagnostico_EnUnaLlamada()
    {
        var calibracion = CalibracionDelJuez.Leer();
        var caso = CasoDeEvaluacion.Armar(ConjuntoDeOro.Leer(), SalidasDe07.Leer(), "-", calibracion, "-").Single(c => c.EntradaId == "c01");
        var espia = new ClienteEspia(FabricaDeClientes.CrearChatOllama(_config.PropioEndpoint, _config.PropioModelo, _config));
        var r = await new RelevanceEvaluator().EvaluateAsync(
            [new ChatMessage(ChatRole.User, caso.Pregunta.Texto)],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, caso.Respuesta)),
            new ChatConfiguration(espia));
        var m = r.Get<NumericMetric>(NombresDeMetricas.Relevance);

        Assert.That(m.Value is >= 1 and <= 5 || m.Diagnostics!.Any(d => d.Severity == EvaluationDiagnosticSeverity.Error), Is.True);
        Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Equivalencia_DevuelvePuntajeODiagnostico_EnUnaLlamada()
    {
        // REC-18: la misma aserción de forma para Equivalence, contra la referencia del oro.
        var caso = CorridaDeEquivalencia.Elegibles(CasoDeEvaluacion.Armar(ConjuntoDeOro.Leer(), SalidasDe07.Leer(), "-", CalibracionDelJuez.Leer(), "-")).First();
        var espia = new ClienteEspia(FabricaDeClientes.CrearChatOllama(_config.PropioEndpoint, _config.PropioModelo, _config));
        var r = await new EquivalenceEvaluator().EvaluateAsync(
            [new ChatMessage(ChatRole.User, caso.Pregunta.Texto)],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, caso.Respuesta)),
            new ChatConfiguration(espia),
            [new EquivalenceEvaluatorContext(ReferenciaDelOro.Para(caso.Pregunta)!)]);
        var m = r.Get<NumericMetric>(NombresDeMetricas.Equivalence);

        Assert.That(m.Value is >= 1 and <= 5 || m.Diagnostics!.Any(d => d.Severity == EvaluationDiagnosticSeverity.Error), Is.True);
        Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(1));
    }
}
