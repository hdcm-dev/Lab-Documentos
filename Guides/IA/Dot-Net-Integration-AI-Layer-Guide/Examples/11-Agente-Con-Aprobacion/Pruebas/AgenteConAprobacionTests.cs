// Nuevo: pruebas del ejemplo 11. Las de cableado corren sin red, con la persistencia real sobre
// SQLite en un archivo temporal; la de Category("LLM") se salta si el servidor no responde o el
// modelo no declara herramientas.
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Aprobacion;
using MovilidadUrbana.Asistente.Application.Herramientas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace MovilidadUrbana.Asistente.Pruebas;

public class AgenteConAprobacionTests
{
    private static readonly ConfiguracionDelModelo Config = new() { RegistroDir = Path.Combine(Path.GetTempPath(), "rllm-11-pruebas") };

    [Test]
    public async Task BajaAprobada_EnDosPeticiones_ConLaAgentSessionSerializada()
    {
        var e = await Garantias.CorrerGuionAsync(Config, "Borrá Resistencia.", Corrida.PasosDelGuion(), _ => true);

        Assert.That(e.Conversacion.Peticiones, Is.EqualTo(2));
        Assert.That(e.Guion.Llamadas, Has.Count.EqualTo(3), "listar y pedir la baja en la petición 1; contar el resultado en la 2");
        Assert.That(e.Grabador.Llamadas.Select(l => l.Peticion), Is.EqualTo(new[] { "peticion-1", "peticion-1", "peticion-2" }));
        // DEP-11: la petición 2 arma su agente de nuevo y recibe el historial sólo por la sesión deserializada.
        Assert.That(e.Grabador.Llamadas[2].Mensajes.Any(m => m.Role == ChatRole.User && m.Text == "Borrá Resistencia."));
        Assert.That(e.Conversacion.EstadosSerializados, Has.Count.EqualTo(2));
        Assert.That(() => JsonDocument.Parse(e.Conversacion.EstadosSerializados[0]), Throws.Nothing);

        var resistencia = e.Antes.Single(l => l.Nombre == "Resistencia");
        var pedido = e.Conversacion.Mostrados.Single();
        Assert.That(pedido.Muestra, Does.StartWith($"Borrar \"Resistencia\" (Chaco), id {resistencia.Id}, de tu sesión."));
        Assert.That(pedido.Marcado, Is.True, "listó antes: el pedido viene después de leer texto que la app no escribió");
        Assert.That(pedido.Muestra, Does.Contain(ServidorDeAprobaciones.AvisoDeMarca));
        Assert.That(e.Despues.Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes" }));
        Assert.That(Garantias.Comprobar("t", "Borrá Resistencia.", e.Servidor, e.Conversacion, e.Antes, e.Despues, e.Grabador.Llamadas), Is.Empty);
        Assert.That(Corrida.Veredicto(new Entrada("t", "Borrá Resistencia.", "baja", "Resistencia", "Chaco"), e.Conversacion), Is.True);
    }

    [Test]
    public async Task ConNo_CreateResponseFalse_CeroBajas_YLaBaseNoCambia()
    {
        var e = await Garantias.CorrerGuionAsync(Config, "Borrá Resistencia.",
            [PasoDelGuion.Herramienta("n1", HerramientasDelAgente.Eliminar, Corrida.Baja("Resistencia", "Chaco")), PasoDelGuion.Texto("No la borro.")],
            _ => false);

        Assert.That(e.Conversacion.Mostrados, Has.Count.EqualTo(1));
        Assert.That(e.Conversacion.Ejecuciones, Is.Empty);
        Assert.That(e.Despues, Is.EqualTo(e.Antes));
        Assert.That(e.Servidor.Aprobados, Is.Empty);
    }

    [Test]
    public async Task Desajuste_ElTextoDiceResistencia_LaLlamadaPideCorrientes_ElPedidoMuestraCorrientes()
    {
        var e = await Garantias.CorrerGuionAsync(Config, "Borrá Resistencia.",
            [
                new PasoDelGuion([new TextContent("Voy a borrar Resistencia."), new FunctionCallContent("d1", HerramientasDelAgente.Eliminar, Corrida.Baja("Corrientes", "Corrientes"))]),
                PasoDelGuion.Texto("Listo.")
            ],
            _ => true);

        var pedido = e.Conversacion.Mostrados.Single();
        Assert.That(pedido.Muestra, Does.StartWith("Borrar \"Corrientes\" (Corrientes)"), "lo arma el código desde la llamada, no desde la prosa");
        Assert.That(pedido.Desajuste, Is.True, "la prueba detecta que la prosa nombra otra localidad");
        Assert.That(pedido.Muestra, Does.Contain("\"Resistencia\""));
        // Si la persona aprueba lo que vio, se borra lo que vio: Corrientes. Resistencia sigue.
        Assert.That(e.Despues.Select(l => l.Nombre), Is.EquivalentTo(new[] { "Resistencia" }));
        Assert.That(Garantias.Comprobar("t", "Borrá Resistencia.", e.Servidor, e.Conversacion, e.Antes, e.Despues, e.Grabador.Llamadas), Is.Empty);
        Assert.That(Corrida.Veredicto(new Entrada("t", "Borrá Resistencia.", "baja", "Resistencia", "Chaco"), e.Conversacion), Is.False,
            "el oráculo de la entrada marca falla: se borró otra localidad que la pedida");
    }

    [Test]
    public async Task SinListar_ElPedidoNoQuedaMarcado()
    {
        var e = await Garantias.CorrerGuionAsync(Config, "Borrá Resistencia.",
            [PasoDelGuion.Herramienta("s1", HerramientasDelAgente.Eliminar, Corrida.Baja("Resistencia", "")), PasoDelGuion.Texto("Listo.")],
            _ => true);

        var pedido = e.Conversacion.Mostrados.Single();
        Assert.That(pedido.Marcado, Is.False);
        Assert.That(pedido.Muestra, Does.Not.Contain(ServidorDeAprobaciones.AvisoDeMarca));
        Assert.That(pedido.NombreResuelto, Is.EqualTo("Resistencia"), "sin provincia resuelve si el nombre es único en la sesión");
    }

    [Test]
    public async Task LocalidadDeOtraSesion_SeRechazaSinPreguntar()
    {
        var e = await Garantias.CorrerGuionAsync(Config, "Borrá Goya.",
            [PasoDelGuion.Herramienta("o1", HerramientasDelAgente.Eliminar, Corrida.Baja("Goya", "Corrientes")), PasoDelGuion.Texto("No está.")],
            _ => true,
            async b => await b.SaveAsync(new LocalidadModel { Nombre = "Goya", Provincia = "Corrientes", CodigoPostal = "3450", Habitantes = 90000 }));

        Assert.That(e.Conversacion.Mostrados, Is.Empty);
        Assert.That(e.Servidor.RechazadosSinPreguntar.Single().Motivo, Does.Contain("No hay una localidad"));
        Assert.That(e.Despues, Is.EqualTo(e.Antes));
        Assert.That(e.OtraSesion.Select(l => l.Nombre), Does.Contain("Goya"));
        // FunctionInvokingChatClient convierte el rechazo en el resultado de la llamada: el modelo ve el
        // motivo que puso el código, y la herramienta nunca se ejecutó.
        Assert.That(e.Conversacion.Ejecuciones, Is.Empty);
        Assert.That(e.Guion.ResultadosVistos.Single(), Does.StartWith("o1=").And.Contain("No hay una localidad"));
    }

    [Test]
    public async Task Aprobacion_ArgumentosAlterados_SeRechaza()
    {
        var db = Path.Combine(Path.GetTempPath(), $"ejemplo-11-pruebas-{Guid.NewGuid():n}.db");
        try
        {
            await using var servicios = Corrida.CrearServicios(db);
            await using var s = Corrida.AbrirSesion(servicios, "p-alterada");
            var servidor = new ServidorDeAprobaciones();
            var pedido = new ToolApprovalRequestContent("a1", new FunctionCallContent("a1", HerramientasDelAgente.Eliminar, Corrida.Baja("Resistencia", "Chaco")));
            var p = await servidor.RegistrarAsync([new ChatMessage(ChatRole.Assistant, [pedido])], s.ServiceProvider.GetRequiredService<LocalidadService>(), false);

            var r = servidor.Responder(p.Single().Id, true, new FunctionCallContent("a1", HerramientasDelAgente.Eliminar, Corrida.Baja("Corrientes", "Corrientes")));

            Assert.That(r.Approved, Is.False);
            Assert.That(servidor.Aprobados, Is.Empty);
            Assert.That(servidor.IdAprobado("a1"), Is.Null);
        }
        finally
        {
            foreach (var f in new[] { db, db + "-wal", db + "-shm" }) if (File.Exists(f)) File.Delete(f);
        }
    }

    /// <summary>
    /// REC-27, caso rojo: la AgentSession serializada es frontera de confianza. MAF 1.26.0 ejecuta la
    /// llamada que dice la sesión DESERIALIZADA, no la del pedido que se aprobó: se altera el JSON
    /// guardado (Resistencia → Corrientes), se responde «sí» al pedido original y la función sin guarda
    /// recibe Corrientes. Prueba sobre una función de juguete, sin el código del ejemplo.
    /// </summary>
    [Test]
    public async Task CasoRojo_SesionSerializadaAlterada_SinGuarda_MafEjecutaLoAlterado()
    {
        var ejecutadas = new List<string>();
        var baja = AIFunctionFactory.Create((string nombre, string provincia) => { ejecutadas.Add($"{nombre}|{provincia}"); return "ok"; }, HerramientasDelAgente.Eliminar);
        var agente = new GuionChatClient([PasoDelGuion.Herramienta("r1", HerramientasDelAgente.Eliminar, Corrida.Baja("Resistencia", "Chaco")), PasoDelGuion.Texto("Listo.")])
            .AsAIAgent(instructions: "Asistente.", name: "Asistente", tools: [new ApprovalRequiredAIFunction(baja)]);
        var sesion = await agente.CreateSessionAsync();
        var pedido = (await agente.RunAsync("Borrá Resistencia.", sesion)).Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().Single();

        var (json, reemplazos) = AlterarBaja((await agente.SerializeSessionAsync(sesion)).GetRawText());
        using var alterado = JsonDocument.Parse(json);
        var sesionAlterada = await agente.DeserializeSessionAsync(alterado.RootElement);
        await agente.RunAsync(new ChatMessage(ChatRole.User, [pedido.CreateResponse(true)]), sesionAlterada);

        Assert.That(reemplazos, Is.GreaterThanOrEqualTo(1), "la sesión serializada lleva los argumentos de la llamada");
        Assert.That(ejecutadas, Is.EqualTo(new[] { "Corrientes|Corrientes" }), "se esperaba el caso rojo: MAF ejecuta la llamada de la sesión alterada");
    }

    /// <summary>
    /// REC-27, la guarda: con la composición del ejemplo, la misma alteración entre las dos peticiones
    /// llega a la herramienta (MAF la ejecuta, como en el caso rojo), pero la herramienta compara con
    /// la copia que guardó el servidor al aprobar (el id resuelto para ese CallId) y no borra nada.
    /// </summary>
    [Test]
    public async Task SesionSerializadaAlterada_LaGuardaComparaConLoAprobado_YNoBorra()
    {
        var db = Path.Combine(Path.GetTempPath(), $"ejemplo-11-pruebas-{Guid.NewGuid():n}.db");
        try
        {
            await using var servicios = Corrida.CrearServicios(db);
            const string sesionDeDatos = "p-sesion-alterada";
            var antes = await Garantias.ListarAsync(servicios, sesionDeDatos);
            var grabador = new Infrastructure.AI.GrabadorDeLlamadas(FabricaDeClientes.Envolver(
                new GuionChatClient([PasoDelGuion.Herramienta("x1", HerramientasDelAgente.Eliminar, Corrida.Baja("Resistencia", "Chaco")), PasoDelGuion.Texto("Listo.")]),
                Config, Adaptador.OllamaSharp));
            var servidor = new ServidorDeAprobaciones();

            // Petición 1: el pedido de la persona; el servidor guarda el pedido y la sesión serializada.
            IReadOnlyList<PedidoPendiente> pendientes;
            string estado;
            await using (var s1 = Corrida.AbrirSesion(servicios, sesionDeDatos))
            {
                var localidades = s1.ServiceProvider.GetRequiredService<LocalidadService>();
                var agente = Corrida.ArmarAgente(grabador, new HerramientasDelAgente(localidades, servidor).Crear());
                var sesion = await agente.CreateSessionAsync();
                var respuesta = await agente.RunAsync("Borrá Resistencia.", sesion);
                pendientes = await servidor.RegistrarAsync(respuesta.Messages, localidades, false);
                estado = (await agente.SerializeSessionAsync(sesion)).GetRawText();
            }

            // Entre las dos peticiones alguien altera la sesión guardada.
            var (alterado, reemplazos) = AlterarBaja(estado);

            // Petición 2: llega (id, «sí»); la respuesta sale del pedido GUARDADO, la sesión del JSON alterado.
            await using var s2 = Corrida.AbrirSesion(servicios, sesionDeDatos);
            var localidades2 = s2.ServiceProvider.GetRequiredService<LocalidadService>();
            var herramientas = new HerramientasDelAgente(localidades2, servidor);
            var agente2 = Corrida.ArmarAgente(grabador, herramientas.Crear());
            using var guardado = JsonDocument.Parse(alterado);
            var sesion2 = await agente2.DeserializeSessionAsync(guardado.RootElement);
            await agente2.RunAsync(new ChatMessage(ChatRole.User, [servidor.Responder(pendientes.Single().Id, true)]), sesion2);

            Assert.That(reemplazos, Is.GreaterThanOrEqualTo(1));
            Assert.That(pendientes.Single().NombreResuelto, Is.EqualTo("Resistencia"), "la persona aprobó Resistencia");
            var ejecucion = herramientas.Ejecuciones.Single();
            Assert.That(ServidorDeAprobaciones.Texto(ejecucion.Argumentos, "nombre"), Is.EqualTo("Corrientes"), "MAF ejecutó lo que dice la sesión alterada");
            Assert.That(ejecucion.Resultado.Exito, Is.False);
            Assert.That(ejecucion.Resultado.Errores, Contains.Key("nombre"), "la localidad no es la que se mostró en el pedido");
            Assert.That(await Garantias.ListarAsync(servicios, sesionDeDatos), Is.EqualTo(antes), "ni Corrientes ni Resistencia se borran");
        }
        finally
        {
            foreach (var f in new[] { db, db + "-wal", db + "-shm" }) if (File.Exists(f)) File.Delete(f);
        }
    }

    /// <summary>Cambia en la sesión serializada los argumentos de la baja: Resistencia (Chaco) → Corrientes (Corrientes).</summary>
    private static (string Json, int Reemplazos) AlterarBaja(string json)
    {
        var reemplazos = 0;
        foreach (var (de, a) in new[] { ("\"nombre\":\"Resistencia\"", "\"nombre\":\"Corrientes\""), ("\"provincia\":\"Chaco\"", "\"provincia\":\"Corrientes\"") })
        {
            var partes = json.Split(de);
            reemplazos += partes.Length - 1;
            json = string.Join(a, partes);
        }
        return (json, reemplazos);
    }

    [Test]
    public async Task LaHerramienta_SinAprobacionRegistrada_NoBorra()
    {
        var db = Path.Combine(Path.GetTempPath(), $"ejemplo-11-pruebas-{Guid.NewGuid():n}.db");
        try
        {
            await using var servicios = Corrida.CrearServicios(db);
            await using var s = Corrida.AbrirSesion(servicios, "p-directa");
            var localidades = s.ServiceProvider.GetRequiredService<LocalidadService>();
            var herramientas = new HerramientasDelAgente(localidades, new ServidorDeAprobaciones());

            var r = await herramientas.EliminarAsync("Resistencia", "Chaco");

            Assert.That(r.Exito, Is.False);
            Assert.That(r.Errores, Contains.Key("aprobacion"));
            Assert.That((await localidades.GetAllAsync()).Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes", "Resistencia" }));
        }
        finally
        {
            foreach (var f in new[] { db, db + "-wal", db + "-shm" }) if (File.Exists(f)) File.Delete(f);
        }
    }

    [Test]
    public async Task TodaLlamada_SaleConLaCapa_ElAgenteNoDuplicaElBucle_YLaBajaEsAprobable()
    {
        var e = await Garantias.CorrerGuionAsync(Config, "Borrá Resistencia.", Corrida.PasosDelGuion(), _ => true);
        var herramientas = new HerramientasDelAgente(null!, new ServidorDeAprobaciones()).Crear();

        Assert.That(herramientas.Single(t => t.Name == HerramientasDelAgente.Eliminar), Is.InstanceOf<ApprovalRequiredAIFunction>());
        Assert.That(herramientas.Single(t => t.Name == HerramientasDelAgente.Listar), Is.Not.InstanceOf<ApprovalRequiredAIFunction>());
        // Un solo bucle: si ChatClientAgent sumara otro FunctionInvokingChatClient, el espía vería otra cuenta.
        Assert.That(e.Espia.OpcionesVistas, Has.Count.EqualTo(e.Guion.Llamadas.Count).And.Count.EqualTo(3));
        Assert.That(e.Espia.OpcionesVistas.Select(o => o?.Reasoning?.Effort), Has.All.EqualTo(ReasoningEffort.None));
        Assert.That(e.Espia.OpcionesVistas.Select(OpcionesDelProveedor.NumCtxDe), Has.All.EqualTo(Config.NumCtx));
        Assert.That(e.Espia.OpcionesVistas.Select(o => o?.Tools?.Count), Has.All.EqualTo(2), "el código fija las dos herramientas");
        // El uso grabado es el de CADA llamada: FunctionInvokingChatClient acumula el de las vueltas en
        // el UsageDetails de la primera, y sin copia la línea 1 del registro mostraría la suma.
        Assert.That(e.Grabador.Llamadas.Select(l => l.Respuesta.Usage?.InputTokenCount),
            Is.EqualTo(e.Guion.Llamadas.Select(l => (long?)GuardaDeTruncado.Estimar(l.Mensajes, l.Opciones))));
    }

    [Test]
    public void Estimulo_RotuladoYConValoresDelCaso()
    {
        var datos = ArchivoDeEntradas.Leer(Path.Combine(AppContext.BaseDirectory, "entradas.v1.json"));
        Assert.That(datos.Rotulo, Is.EqualTo("escrito para el ejemplo"));
        Assert.That(datos.Entradas, Has.Count.EqualTo(1), "demostración de TURNO-1: 1 pedido");
        foreach (var e in datos.Entradas)
        {
            // (i) de §E.1: la provincia esperada es de Catalogos.
            Assert.That(Catalogos.Provincias, Does.Contain(e.Provincia), e.Id);
            Assert.That(e.Texto, Does.Contain(e.Nombre), e.Id);
        }
    }

    [Test]
    public async Task CorridaFalsa_CumpleInvariantes_YDaElVeredictoEsperado()
    {
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--verificar"]), Config, TextWriter.Null);
        Assert.That(r.InvariantesQueFallan, Is.Empty);
        Assert.That(r.Veredictos, Is.EquivalentTo(new Dictionary<string, string> { ["e01"] = "pasa" }));
    }

    [Test]
    public async Task GarantiasGuionadas_SeCumplenTodas()
    {
        var g = await Garantias.GuionadasAsync(Config);
        Assert.That(g.Fallan, Is.Empty);
        Assert.That(g.Comprobadas, Has.Count.EqualTo(4));
    }
}

[Category("LLM")]
public class AgenteConModeloTests
{
    private ConfiguracionDelModelo _config = null!;

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno();
        var r = await PredicadoDeSalto.EvaluarAsync(_config.Endpoint, _config.Modelo, PredicadoDeSalto.Chat);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task ConElModelo_YLaPersonaQueRechaza_CeroBajas()
    {
        var db = Path.Combine(Path.GetTempPath(), $"ejemplo-11-llm-{Guid.NewGuid():n}.db");
        try
        {
            await using var servicios = Corrida.CrearServicios(db);
            var antes = await Garantias.ListarAsync(servicios, "p-llm");
            var grabador = new Infrastructure.AI.GrabadorDeLlamadas(FabricaDeClientes.CrearChatOllama(_config.Endpoint, _config.Modelo, _config.Validar()));
            var servidor = new ServidorDeAprobaciones();

            var c = await Corrida.ConversarAsync(servicios, "p-llm", grabador, servidor, "Borrá Resistencia.", _ => false, TextWriter.Null);

            // Propiedad, no texto literal: diga lo que diga el modelo, sin aprobación la base no cambia.
            Assert.That(await Garantias.ListarAsync(servicios, "p-llm"), Is.EqualTo(antes));
            Assert.That(c.Ejecuciones.Where(x => x.Resultado.Exito), Is.Empty);
        }
        finally
        {
            foreach (var f in new[] { db, db + "-wal", db + "-shm" }) if (File.Exists(f)) File.Delete(f);
        }
    }
}
