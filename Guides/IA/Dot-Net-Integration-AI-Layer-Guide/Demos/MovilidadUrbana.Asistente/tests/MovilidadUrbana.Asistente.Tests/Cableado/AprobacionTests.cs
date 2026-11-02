// Nuevo. AC-4 y la política de origen (plan §F.4): aprobación en dos peticiones, argumentos
// alterados, escritura después de texto no confiable, y alcance por sesión.
using System.Net.Http.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Contracts;
using MovilidadUrbana.Asistente.Infrastructure.AI.Chat;
using MovilidadUrbana.Asistente.Sessions;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
[NonParallelizable]
public class AprobacionTests : PruebaConHost
{
    private static Dictionary<string, object?> Args(params (string Clave, object? Valor)[] pares) =>
        pares.ToDictionary(p => p.Clave, p => p.Valor);

    [Test]
    [Description("Una respuesta de aprobación con argumentos distintos de los del pedido guardado se rechaza y no ejecuta nada")]
    public async Task Aprobacion_ArgumentosAlterados_SeRechaza()
    {
        using var circuito = NuevoCircuito();
        Fabrica.Guion.Poner(PasoDelGuion.Herramienta("e1", HerramientasDelAsistente.Eliminar, Args(("nombre", "Resistencia"))), PasoDelGuion.Texto("No se borró."));
        await circuito.Chat.EnviarAsync("Borrá Resistencia");
        var pedido = circuito.Chat.Pendientes.Single();

        // Un cliente que fabrica la respuesta y cambia la localidad.
        var alterada = new ToolApprovalResponseContent(pedido.Id, true,
            new FunctionCallContent("e1", HerramientasDelAsistente.Eliminar, Args(("nombre", "Corrientes"))));
        var aceptada = await circuito.Chat.AplicarRespuestaAsync(alterada);

        Assert.That(aceptada, Is.False);
        Assert.That(circuito.Chat.RespuestasRechazadas, Is.EqualTo(1));
        Assert.That((await circuito.Get<LocalidadService>().GetAllAsync()).Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes", "Resistencia" }));
    }

    [Test]
    [Description("Regla 2: después de leer texto que la app no escribió, la escritura pide aprobación y el pedido lo dice")]
    public async Task Escritura_ConResultadoNoConfiable_PideAprobacion()
    {
        using var circuito = NuevoCircuito();
        Fabrica.Guion.Poner(
            PasoDelGuion.Herramienta("l1", HerramientasDelAsistente.Listar),
            PasoDelGuion.Herramienta("e1", HerramientasDelAsistente.Eliminar, Args(("nombre", "Resistencia"))),
            PasoDelGuion.Texto("Hecho."));

        await circuito.Chat.EnviarAsync("Listá mis localidades y borrá la segunda");

        var pedido = circuito.Chat.Pendientes.Single();
        Assert.That(circuito.Chat.Estado.Marcado, Is.True);
        Assert.That(pedido.DespuesDeTextoNoConfiable, Is.True);
        Assert.That(pedido.Descripcion, Does.EndWith(ResolutorDeAprobaciones.AvisoDeOrigen));
        Assert.That((await circuito.Get<LocalidadService>().GetAllAsync()).Count, Is.EqualTo(2), "nada se ejecuta sin la aprobación");

        await circuito.Chat.DecidirAsync(pedido.Id, aprobar: true);
        Assert.That((await circuito.Get<LocalidadService>().GetAllAsync()).Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes" }));
    }

    [Test]
    [Description("Regla 2: una escritura NO aprobable, después de texto no confiable, se rechaza sin ejecutar")]
    public async Task Escritura_NoAprobable_ConResultadoNoConfiable_Rechaza()
    {
        using var circuito = NuevoCircuito();
        var (guion, respuesta) = await CorrerSinAprobacion(circuito, leerAntes: true);

        Assert.That(guion.ResultadosVistos.Last(), Does.EndWith(GuardaDeOrigen.Rechazo));
        Assert.That(respuesta.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>(), Is.Empty);
        Assert.That((await circuito.Get<LocalidadService>().GetAllAsync()).Count, Is.EqualTo(2));
    }

    [Test]
    [Description("Control de la anterior: la misma escritura no aprobable, sin texto no confiable antes, sí ejecuta (la guarda es la causa del rechazo)")]
    public async Task Escritura_NoAprobable_SinTextoNoConfiable_Ejecuta()
    {
        using var circuito = NuevoCircuito();
        await CorrerSinAprobacion(circuito, leerAntes: false);

        Assert.That((await circuito.Get<LocalidadService>().GetAllAsync()).Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes" }));
    }

    private async Task<(GuionChatClient, ChatResponse)> CorrerSinAprobacion(Circuito circuito, bool leerAntes)
    {
        var pasos = new List<PasoDelGuion>();
        if (leerAntes) pasos.Add(PasoDelGuion.Herramienta("l1", HerramientasDelAsistente.Listar));
        pasos.Add(PasoDelGuion.Herramienta("e1", HerramientasDelAsistente.Eliminar, Args(("nombre", "Resistencia"))));
        pasos.Add(PasoDelGuion.Texto("Fin."));
        var guion = new GuionChatClient(pasos);
        var estado = circuito.Get<EstadoDelPedido>();
        estado.Empezar(marcadoPorElHistorial: false);
        var herramientas = circuito.Get<HerramientasDelAsistente>().Crear(escriturasAprobables: false);
        using var cliente = new ChatClientBuilder(guion).UseFunctionInvocation().Build();
        var respuesta = await cliente.GetResponseAsync([new ChatMessage(ChatRole.User, "Borrá Resistencia")], new ChatOptions { Tools = herramientas });
        return (guion, respuesta);
    }

    [Test]
    [Description("Regla 1: con contexto no confiable al armarse, el pedido no tiene herramientas de escritura")]
    public void ContextoNoConfiable_SinHerramientasDeEscritura()
    {
        using var circuito = NuevoCircuito();
        var nombres = circuito.Get<HerramientasDelAsistente>().Crear(conEscrituras: false).OfType<AIFunctionDeclaration>().Select(f => f.Name);

        Assert.That(nombres, Is.EquivalentTo(new[] { HerramientasDelAsistente.Listar, HerramientasDelAsistente.Contar, HerramientasDelAsistente.Resumir }));
    }

    [Test]
    [Description("Toda escritura del pedido normal es aprobable (ApprovalRequiredAIFunction)")]
    public void TodaEscritura_EsAprobable()
    {
        using var circuito = NuevoCircuito();
        var herramientas = circuito.Get<HerramientasDelAsistente>().Crear();

        var escrituras = herramientas.OfType<AIFunctionDeclaration>().Where(f => HerramientasDelAsistente.Escrituras.Contains(f.Name)).ToList();
        Assert.That(escrituras, Has.Count.EqualTo(2).And.All.InstanceOf<ApprovalRequiredAIFunction>());
    }

    [Test]
    [Description("Un pedido que nombra una localidad de otra sesión no la alcanza: se rechaza sin preguntar")]
    public async Task PedidoQueNombraOtraSesion_NoLaAlcanza()
    {
        var api = Fabrica.CreateClient();
        api.DefaultRequestHeaders.Add(SessionHeaderMiddleware.HeaderName, "sesion-ajena-" + Guid.NewGuid().ToString("n")[..8]);
        await api.PostAsJsonAsync("/api/v1/localidades", new LocalidadRequest("Goya", "Corrientes", "3450", 90000));

        using var circuito = NuevoCircuito();
        var guion = Fabrica.Guion.Poner(PasoDelGuion.Herramienta("e1", HerramientasDelAsistente.Eliminar, Args(("nombre", "Goya"))), PasoDelGuion.Texto("No existe."));
        await circuito.Chat.EnviarAsync("Borrá Goya");

        Assert.That(circuito.Chat.Pendientes, Is.Empty);
        Assert.That(guion.ResultadosVistos.Single(), Does.Contain("No hay una localidad con ese nombre en tu sesión."));
    }
}
