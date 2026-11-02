// Nuevo: prueba con modelo (Category LLM; el arnés no la corre en la fase build). Una sola
// llamada con el prefijo: comprueba que num_ctx llega (el servidor informa todo el prefijo).
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Registro;
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
        var r = await PredicadoDeSalto.EvaluarAsync(_config.PropioEndpoint, _config.PropioModelo, ["completion"]);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task ElServidorInformaElPrefijoEntero()
    {
        var cliente = FabricaDeClientes.CrearChatOllama(_config.PropioEndpoint, _config.PropioModelo, _config);
        var prefijo = PrefijoEstable.Construir(PadronDeLocalidades.ATexto(PadronDeLocalidades.Generar()));
        List<ChatMessage> mensajes = [new(ChatRole.System, prefijo), new(ChatRole.User, "¿En qué provincia está la Localidad 23?")];

        var respuesta = await cliente.GetResponseAsync(mensajes);

        var metricas = MetricasDeOllama.De(respuesta);
        Assert.That(metricas.PromptEvalCount, Is.GreaterThanOrEqualTo(GuardaDeTruncado.Estimar(mensajes) * GuardaDeTruncado.FraccionMinima));
        Assert.That(metricas.PromptEvalCount, Is.LessThan(_config.NumCtx));
    }
}
