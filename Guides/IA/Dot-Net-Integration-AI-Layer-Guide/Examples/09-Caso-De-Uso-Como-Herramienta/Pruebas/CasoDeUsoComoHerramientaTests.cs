// Nuevo: pruebas del ejemplo 09. Las de cableado corren sin red, con la persistencia real sobre
// SQLite en un archivo temporal; la de Category("LLM") se salta si el servidor no responde o el
// modelo no declara herramientas.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Aprobacion;
using MovilidadUrbana.Asistente.Application.Herramientas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Infrastructure.AI;

namespace MovilidadUrbana.Asistente.Pruebas;

public class CasoDeUsoComoHerramientaTests
{
    private static readonly ConfiguracionDelModelo Config = new() { RegistroDir = Path.Combine(Path.GetTempPath(), "rllm-09-pruebas") };
    private string _db = string.Empty;
    private ServiceProvider _servicios = null!;

    [SetUp]
    public void Preparar()
    {
        _db = Path.Combine(Path.GetTempPath(), $"ejemplo-09-pruebas-{Guid.NewGuid():n}.db");
        _servicios = Corrida.CrearServicios(_db);
    }

    [TearDown]
    public void Limpiar()
    {
        _servicios.Dispose();
        foreach (var f in new[] { _db, _db + "-wal", _db + "-shm" }) if (File.Exists(f)) File.Delete(f);
    }

    private sealed record Armado(AsyncServiceScope Sesion, LocalidadService Localidades, HerramientasDeLocalidades Herramientas,
        ServidorDeAprobaciones Servidor, GuionChatClient Guion, ClienteEspia Espia, IChatClient Asistente);

    private Armado Armar(string sesion, params PasoDelGuion[] pasos)
    {
        var alcance = Corrida.AbrirSesion(_servicios, sesion);
        var localidades = alcance.ServiceProvider.GetRequiredService<LocalidadService>();
        var guion = new GuionChatClient(pasos, "Listo.");
        var espia = new ClienteEspia(guion);
        var conCapa = FabricaDeClientes.Envolver(espia, Config, Adaptador.OllamaSharp);
        return new Armado(alcance, localidades, new HerramientasDeLocalidades(localidades), new ServidorDeAprobaciones(localidades),
            guion, espia, Corrida.ArmarAsistente(new GrabadorDeLlamadas(conCapa)));
    }

    private static Dictionary<string, object?> Alta(string cp) => new()
    {
        ["nombre"] = "Goya", ["provincia"] = "Corrientes", ["codigoPostal"] = cp, ["habitantes"] = 90000
    };

    [Test]
    public async Task CpInvalido_Aprobado_ElCasoDeUsoRechaza_YElResultVuelveAlModelo()
    {
        var a = Armar("p-cp", PasoDelGuion.Herramienta("c1", HerramientasDeLocalidades.Agregar, Alta("34")), PasoDelGuion.Texto("No se pudo."));
        await using var _ = a.Sesion;

        var c = await Corrida.ConversarAsync(a.Asistente, a.Herramientas.Crear(), a.Servidor, "Agregá Goya, Corrientes, CP 34, 90000 habitantes.", _ => true, TextWriter.Null);

        Assert.That(c.Mostrados, Has.Count.EqualTo(1));
        Assert.That(a.Herramientas.Ejecuciones.Single().Resultado.Errores, Contains.Key("codigoPostal"));
        Assert.That(a.Guion.ResultadosVistos.Single(), Does.StartWith("c1=").And.Contain("codigoPostal"));
        Assert.That((await a.Localidades.GetAllAsync()).Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes", "Resistencia" }));
    }

    [Test]
    public async Task AltaAprobada_EscribeLoQueMostroElPedido_EnDosPeticiones()
    {
        var a = Armar("p-alta", PasoDelGuion.Herramienta("c2", HerramientasDeLocalidades.Agregar, Alta("3450")), PasoDelGuion.Texto("Listo."));
        await using var _ = a.Sesion;

        var c = await Corrida.ConversarAsync(a.Asistente, a.Herramientas.Crear(), a.Servidor, "Agregá Goya, Corrientes, CP 3450, 90000 habitantes.", _ => true, TextWriter.Null);

        Assert.That(a.Guion.Llamadas, Has.Count.EqualTo(2), "una llamada por petición: el pedido y la que sigue a la aprobación");
        Assert.That(c.Mostrados.Single().Muestra, Does.Contain("Goya").And.Contain("3450").And.Contain("90000"));
        var goya = (await a.Localidades.GetAllAsync()).Single(l => l.Nombre == "Goya");
        Assert.That((goya.Provincia, goya.CodigoPostal, goya.Habitantes), Is.EqualTo(("Corrientes", "3450", 90000)));
        Assert.That(await Garantias.ComprobarAsync("t", a.Herramientas, a.Servidor, c, a.Localidades), Is.Empty);
        Assert.That(Corrida.Veredicto("alta", a.Herramientas.Ejecuciones), Is.True);
    }

    [Test]
    public async Task SinAprobacion_NoSeEjecuta()
    {
        var a = Armar("p-rechazo", PasoDelGuion.Herramienta("c3", HerramientasDeLocalidades.Agregar, Alta("3450")), PasoDelGuion.Texto("Entendido."));
        await using var _ = a.Sesion;

        await Corrida.ConversarAsync(a.Asistente, a.Herramientas.Crear(), a.Servidor, "Agregá Goya.", _ => false, TextWriter.Null);

        Assert.That(a.Herramientas.Ejecuciones, Is.Empty);
        Assert.That((await a.Localidades.GetAllAsync()).Any(l => l.Nombre == "Goya"), Is.False);
    }

    [Test]
    public async Task Aprobacion_ArgumentosAlterados_SeRechaza()
    {
        var a = Armar("p-alterada");
        await using var _ = a.Sesion;
        var pedido = new ToolApprovalRequestContent("c4", new FunctionCallContent("c4", HerramientasDeLocalidades.Agregar, Alta("3450")));
        var pendientes = await a.Servidor.RegistrarAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, [pedido])));

        var r = a.Servidor.Responder(pendientes.Single().Id, true, new FunctionCallContent("c4", HerramientasDeLocalidades.Agregar, Alta("3400")));

        Assert.That(r.Approved, Is.False);
        Assert.That(a.Servidor.Aprobados, Is.Empty);
        Assert.That(Garantias.AprobacionAlterada(), Is.Empty);
    }

    [Test]
    public async Task Eliminar_Inexistente_SeRechazaSinPreguntar()
    {
        var a = Armar("p-baja-no",
            PasoDelGuion.Herramienta("c5", HerramientasDeLocalidades.Eliminar, new Dictionary<string, object?> { ["nombre"] = "Goya", ["provincia"] = "Corrientes" }),
            PasoDelGuion.Texto("No está."));
        await using var _ = a.Sesion;

        var c = await Corrida.ConversarAsync(a.Asistente, a.Herramientas.Crear(), a.Servidor, "Borrá Goya.", _ => true, TextWriter.Null);

        Assert.That(c.Mostrados, Is.Empty, "no hay nada que mostrar: la baja no resuelve en la sesión");
        Assert.That(a.Servidor.RechazadosSinPreguntar.Single().Motivo, Does.Contain("No hay una localidad"));
        Assert.That(a.Herramientas.Ejecuciones, Is.Empty);
    }

    [Test]
    public async Task Eliminar_Ambiguo_SinProvincia_SeRechazaSinPreguntar()
    {
        var a = Armar("p-baja-ambigua",
            PasoDelGuion.Herramienta("c6", HerramientasDeLocalidades.Eliminar, new Dictionary<string, object?> { ["nombre"] = "Corrientes", ["provincia"] = "" }),
            PasoDelGuion.Texto("¿Cuál?"));
        await using var _ = a.Sesion;
        // Escrito para el ejemplo: una segunda «Corrientes» en otra provincia, con los valores de la siembra de Chaco.
        Assert.That((await a.Localidades.SaveAsync(new LocalidadModel { Nombre = "Corrientes", Provincia = "Chaco", CodigoPostal = "3500", Habitantes = 291720 })).IsSuccess);

        await Corrida.ConversarAsync(a.Asistente, a.Herramientas.Crear(), a.Servidor, "Borrá Corrientes.", _ => true, TextWriter.Null);

        Assert.That(a.Servidor.RechazadosSinPreguntar.Single().Motivo, Does.Contain("más de una"));
        Assert.That((await a.Localidades.GetAllAsync()).Count(l => l.Nombre == "Corrientes"), Is.EqualTo(2));
    }

    [Test]
    public async Task Eliminar_Unica_PideAprobacion_YBorraTrasAprobar()
    {
        var a = Armar("p-baja",
            PasoDelGuion.Herramienta("c7", HerramientasDeLocalidades.Eliminar, new Dictionary<string, object?> { ["nombre"] = "Resistencia", ["provincia"] = "Chaco" }),
            PasoDelGuion.Texto("Listo."));
        await using var _ = a.Sesion;

        var c = await Corrida.ConversarAsync(a.Asistente, a.Herramientas.Crear(), a.Servidor, "Borrá Resistencia, Chaco.", _ => true, TextWriter.Null);

        Assert.That(c.Mostrados.Single().Muestra, Does.Contain("BAJA").And.Contain("Resistencia (Chaco, CP 3500"));
        Assert.That((await a.Localidades.GetAllAsync()).Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes" }));
    }

    [Test]
    public async Task LasDosHerramientas_SonAprobables_YTodaLlamadaSaleConLaCapa()
    {
        var a = Armar("p-capa", PasoDelGuion.Herramienta("c8", HerramientasDeLocalidades.Agregar, Alta("3450")), PasoDelGuion.Texto("Listo."));
        await using var _ = a.Sesion;
        var herramientas = a.Herramientas.Crear();

        await Corrida.ConversarAsync(a.Asistente, herramientas, a.Servidor, "Agregá Goya.", _ => true, TextWriter.Null);

        Assert.That(herramientas, Has.All.InstanceOf<ApprovalRequiredAIFunction>());
        Assert.That(a.Espia.OpcionesVistas, Has.Count.EqualTo(2));
        Assert.That(a.Espia.OpcionesVistas.Select(o => o?.Reasoning?.Effort), Has.All.EqualTo(ReasoningEffort.None));
        Assert.That(a.Espia.OpcionesVistas.Select(OpcionesDelProveedor.NumCtxDe), Has.All.EqualTo(Config.NumCtx));
    }

    [Test]
    public void Estimulo_RotuladoYConValoresDelCaso()
    {
        var datos = ArchivoDeEntradas.Leer(Path.Combine(AppContext.BaseDirectory, "entradas.v1.json"));
        Assert.That(datos.Rotulo, Is.EqualTo("escrito para el ejemplo"));
        Assert.That(datos.Entradas.Select(e => e.Esperado), Is.EqualTo(new[] { "rechazo", "alta" }));
        foreach (var e in datos.Entradas)
        {
            // (i) de §E.1: la provincia nombrada es de Catalogos.
            Assert.That(Catalogos.Provincias.Any(p => e.Texto.Contains(p, StringComparison.Ordinal)), e.Id);
        }
    }

    [Test]
    public async Task CorridaFalsa_CumpleInvariantes_YDaLosVeredictosEsperados()
    {
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--verificar"]), Config, TextWriter.Null);
        Assert.That(r.InvariantesQueFallan, Is.Empty);
        Assert.That(r.Veredictos, Is.EquivalentTo(new Dictionary<string, string> { ["e01"] = "pasa", ["e02"] = "pasa" }));
    }
}

[Category("LLM")]
public class CasoDeUsoConModeloTests
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
    public async Task ConElModelo_NuncaQuedaUnCpInvalido_NiUnaEscrituraSinAprobacion()
    {
        var db = Path.Combine(Path.GetTempPath(), $"ejemplo-09-llm-{Guid.NewGuid():n}.db");
        try
        {
            await using var servicios = Corrida.CrearServicios(db);
            await using var sesion = Corrida.AbrirSesion(servicios, "p-llm");
            var localidades = sesion.ServiceProvider.GetRequiredService<LocalidadService>();
            var conCapa = FabricaDeClientes.CrearChatOllama(_config.Endpoint, _config.Modelo, _config.Validar());
            var herramientas = new HerramientasDeLocalidades(localidades);
            var servidor = new ServidorDeAprobaciones(localidades);

            var c = await Corrida.ConversarAsync(Corrida.ArmarAsistente(new GrabadorDeLlamadas(conCapa)), herramientas.Crear(), servidor,
                "Agregá Goya, Corrientes, CP 34, 90000 habitantes.", _ => true, TextWriter.Null);

            // Propiedad, no texto literal: lo diga como lo diga el modelo, la base no tiene un CP inválido.
            Assert.That(await Garantias.ComprobarAsync("llm", herramientas, servidor, c, localidades), Is.Empty);
        }
        finally
        {
            foreach (var f in new[] { db, db + "-wal", db + "-shm" }) if (File.Exists(f)) File.Delete(f);
        }
    }
}
