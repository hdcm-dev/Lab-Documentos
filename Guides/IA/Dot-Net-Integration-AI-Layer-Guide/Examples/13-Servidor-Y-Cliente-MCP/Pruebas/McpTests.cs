// Nuevo: pruebas del ejemplo 13. Las de cableado corren sin servidor de modelos (el servidor MCP es
// del propio proceso, en loopback); la de [Category("LLM")] se salta con el motivo del predicado.
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Mcp;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

public class McpTests
{
    private static readonly ConfiguracionDelModelo Config = new() { RegistroDir = Path.GetTempPath() };

    [Test]
    public async Task Mcp_HostYOrigenAjenos_Rechaza()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        await using var servidor = await ServidorMcp.IniciarAsync(bd);
        var (fallas, c) = await Garantias.HostYOrigenAjenosSeRechazanAsync(servidor);
        Assert.That(fallas, Is.Empty);
        Assert.That(c.HostAjeno, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(c.OrigenAjeno, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(servidor.RechazosPorOrigen, Is.EqualTo(2));
    }

    [Test]
    public async Task Servidor_EscuchaSoloEnLoopback()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        await using var servidor = await ServidorMcp.IniciarAsync(bd);
        Assert.That(Garantias.SoloLoopback(servidor), Is.Empty);
    }

    [TestCase(null, true)]
    [TestCase("", true)]
    [TestCase("http://localhost:5000", true)]
    [TestCase("http://127.0.0.1", true)]
    [TestCase("http://[::1]:8080", true)]
    [TestCase("http://ajeno.example", false)]
    [TestCase("https://localhost.ajeno.example", false)]
    [TestCase("null", false)]
    [TestCase("file://localhost", false)]
    public void OrigenPermitido(string? origen, bool esperado) =>
        Assert.That(ServidorMcp.OrigenPermitido(origen), Is.EqualTo(esperado));

    [Test]
    public async Task Listado_SoloLecturas_YLaSesionFijaNoVeLaApi()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        await using var servidor = await ServidorMcp.IniciarAsync(bd);
        await using var cliente = await ClienteMcp.ConectarAsync(servidor.Endpoint);
        var anunciadas = await cliente.ListToolsAsync();

        Assert.That(Garantias.ListadoSoloDeLectura(anunciadas), Is.Empty);
        Assert.That(await Garantias.SesionFijaAisladaDeLaApiAsync(bd, cliente), Is.Empty);
        Assert.That(anunciadas[0], Is.InstanceOf<AIFunction>(), "McpClientTool ya es AIFunction");
    }

    [Test]
    public async Task ServidorGuionadoConBorrarTodo_ElClienteNoLaMonta()
    {
        var (fallas, descartadas) = await Garantias.EscrituraAnunciadaNoSeMontaAsync(Config);
        Assert.That(fallas, Is.Empty);
        Assert.That(descartadas, Is.EqualTo(new[] { HerramientasDelServidorGuionado.BorrarTodo }));
    }

    [Test]
    public async Task Montaje_UsaLaDescripcionDelCliente_YUnaEscrituraPermitidaVaConAprobacion()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        await using var servidor = await ServidorMcp.IniciarAsync(bd, guionado: true);
        await using var cliente = await ClienteMcp.ConectarAsync(servidor.Endpoint);
        var anunciadas = await cliente.ListToolsAsync();

        var delEjemplo = ClienteMcp.Montar(anunciadas, PoliticaDeMontaje.DelEjemplo);
        Assert.That(Garantias.DescripcionDelCliente(delEjemplo), Is.Empty);
        Assert.That(delEjemplo.Montadas.OfType<AIFunctionDeclaration>().Single().Description,
            Is.EqualTo(PoliticaDeMontaje.DelEjemplo.Lecturas[LecturasMcp.Listar]));

        // Si alguna vez se permitiera una escritura, se monta envuelta en la aprobación.
        var conEscritura = PoliticaDeMontaje.DelEjemplo with { Escrituras = new Dictionary<string, string> { [HerramientasDelServidorGuionado.BorrarTodo] = DescripcionDeLaEscritura } };
        var montaje = ClienteMcp.Montar(anunciadas, conEscritura);
        Assert.That(montaje.Montadas.OfType<AIFunctionDeclaration>().Single(t => t.Name == HerramientasDelServidorGuionado.BorrarTodo),
            Is.InstanceOf<ApprovalRequiredAIFunction>());
    }

    private const string DescripcionDeLaEscritura = "Borra todas las localidades de la sesión. La app pide confirmación antes.";

    [Test]
    public async Task Montaje_UnaEscrituraPermitidaLlevaLaDescripcionDelCliente_YLaGarantiaLaCubre()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        await using var servidor = await ServidorMcp.IniciarAsync(bd, guionado: true);
        await using var cliente = await ClienteMcp.ConectarAsync(servidor.Endpoint);
        var anunciadas = await cliente.ListToolsAsync();
        var delServidor = anunciadas.Single(t => t.Name == HerramientasDelServidorGuionado.BorrarTodo);
        Assert.That(delServidor.Description, Is.Not.EqualTo(DescripcionDeLaEscritura), "si coincidieran, la prueba no probaría nada");

        var conEscritura = PoliticaDeMontaje.DelEjemplo with { Escrituras = new Dictionary<string, string> { [HerramientasDelServidorGuionado.BorrarTodo] = DescripcionDeLaEscritura } };
        var montaje = ClienteMcp.Montar(anunciadas, conEscritura);
        var escritura = montaje.Montadas.OfType<AIFunctionDeclaration>().Single(t => t.Name == HerramientasDelServidorGuionado.BorrarTodo);
        Assert.That(escritura.Description, Is.EqualTo(DescripcionDeLaEscritura));
        Assert.That(Garantias.DescripcionDelCliente(montaje, conEscritura), Is.Empty);

        // La garantía extendida detecta las dos maneras de montar mal una escritura: con la
        // descripción del servidor, o sin aprobación.
        var conLaDelServidor = new Montaje([new ApprovalRequiredAIFunction(delServidor)], []);
        Assert.That(Garantias.DescripcionDelCliente(conLaDelServidor, conEscritura),
            Is.EqualTo(new[] { $"{HerramientasDelServidorGuionado.BorrarTodo}: la descripción montada de la escritura no es la del cliente" }));
        var sinAprobacion = new Montaje([delServidor.WithDescription(DescripcionDeLaEscritura)], []);
        Assert.That(Garantias.DescripcionDelCliente(sinAprobacion, conEscritura),
            Is.EqualTo(new[] { $"{HerramientasDelServidorGuionado.BorrarTodo}: la escritura se montó sin aprobación" }));
        // Y con la política del ejemplo (sin escrituras), una escritura montada es una falla.
        Assert.That(Garantias.DescripcionDelCliente(montaje), Has.One.EndsWith("se montó sin estar en la política del cliente"));
    }

    [Test]
    public async Task HerramientaPeligrosaMarcadaReadOnly_NoDecideLaListaBlanca()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        await using var servidor = await ServidorMcp.IniciarAsync(bd, guionado: true);
        await using var cliente = await ClienteMcp.ConectarAsync(servidor.Endpoint);
        var anunciadas = await cliente.ListToolsAsync();
        var borrar = anunciadas.Single(t => t.Name == HerramientasDelServidorGuionado.BorrarTodo);

        // El servidor miente: la anuncia de lectura y no destructiva (si no, la prueba no probaría nada).
        Assert.That(borrar.ProtocolTool.Annotations?.ReadOnlyHint, Is.True);
        Assert.That(borrar.ProtocolTool.Annotations?.DestructiveHint, Is.False);

        // Un filtro por anotación la dejaría pasar; la lista blanca por nombre, no.
        var porAnotacion = anunciadas.Where(t => t.ProtocolTool.Annotations?.ReadOnlyHint == true).Select(t => t.Name);
        Assert.That(porAnotacion, Does.Contain(HerramientasDelServidorGuionado.BorrarTodo));
        var montaje = ClienteMcp.Montar(anunciadas, PoliticaDeMontaje.DelEjemplo);
        Assert.That(montaje.Montadas.OfType<AIFunctionDeclaration>().Select(t => t.Name), Is.EqualTo(new[] { LecturasMcp.Listar }));
        Assert.That(montaje.Descartadas, Is.EqualTo(new[] { HerramientasDelServidorGuionado.BorrarTodo }));
    }

    [Test]
    public async Task ToolsList_EsElEsquemaQueRecibeElBucle()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        await using var servidor = await ServidorMcp.IniciarAsync(bd);
        await using var cliente = await ClienteMcp.ConectarAsync(servidor.Endpoint);
        var anunciadas = await cliente.ListToolsAsync();
        var montaje = ClienteMcp.Montar(anunciadas, PoliticaDeMontaje.DelEjemplo);
        var guion = new GuionChatClient([PasoDelGuion.Texto("Corrientes y Resistencia.")]);
        var espia = new ClienteEspia(guion);
        var asistente = new AsistenteConMcp(FabricaDeClientes.Envolver(espia, Config, Adaptador.OllamaSharp));

        await asistente.PreguntarAsync("¿Qué localidades tengo?", montaje.Montadas);

        // Lo que ve el adaptador (debajo de la capa única) es lo que el servidor publicó en
        // tools/list: el mismo nombre y el mismo esquema de parámetros; la descripción es la del cliente.
        var enElBucle = espia.OpcionesVistas.Single()!.Tools!.OfType<AIFunctionDeclaration>().ToList();
        Assert.That(enElBucle.Select(t => t.Name), Is.EqualTo(anunciadas.Select(t => t.Name)));
        foreach (var t in anunciadas)
        {
            var f = enElBucle.Single(x => x.Name == t.Name);
            Assert.That(JsonSerializer.Serialize(f.JsonSchema), Is.EqualTo(JsonSerializer.Serialize(t.ProtocolTool.InputSchema)), t.Name);
            Assert.That(f.Description, Is.EqualTo(PoliticaDeMontaje.DelEjemplo.Lecturas[t.Name]));
        }
    }

    [Test]
    public async Task Conexion_RegistraLaVersionNegociada()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        await using var servidor = await ServidorMcp.IniciarAsync(bd);
        await using var cliente = await ClienteMcp.ConectarAsync(servidor.Endpoint);
        TestContext.Out.WriteLine($"protocolo negociado: {cliente.NegotiatedProtocolVersion}");
        Assert.That(cliente.NegotiatedProtocolVersion, Does.Match(@"^\d{4}-\d{2}-\d{2}$"));
        Assert.That(Garantias.ProtocoloNegociado(cliente), Is.Empty);
    }

    [Test]
    public async Task Bucle_LaHerramientaMcpPasaPorLaCapaUnica()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        await using var servidor = await ServidorMcp.IniciarAsync(bd);
        await using var cliente = await ClienteMcp.ConectarAsync(servidor.Endpoint);
        var montaje = ClienteMcp.Montar(await cliente.ListToolsAsync(), PoliticaDeMontaje.DelEjemplo);
        var guion = new GuionChatClient([PasoDelGuion.Herramienta("c1", LecturasMcp.Listar), PasoDelGuion.Texto("Corrientes y Resistencia.")]);
        var espia = new ClienteEspia(guion);
        var asistente = new AsistenteConMcp(FabricaDeClientes.Envolver(espia, Config, Adaptador.OllamaSharp));

        var r = await asistente.PreguntarAsync("¿Qué localidades tengo?", montaje.Montadas);

        Assert.That(r.Text, Is.EqualTo("Corrientes y Resistencia."));
        Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(2));
        Assert.That(espia.OpcionesVistas, Has.All.Matches<ChatOptions?>(o =>
            o!.Reasoning!.Effort == ReasoningEffort.None && OpcionesDelProveedor.NumCtxDe(o) == ConfiguracionDelModelo.NumCtxPorDefecto));
        Assert.That(guion.ResultadosVistos.Single(), Does.Contain("Resistencia").And.Not.Contain("Goya"));
    }

    [Test]
    public async Task CorridaFake_CumpleInvariantesYPasaElOraculo()
    {
        var dir = Directory.CreateTempSubdirectory("ej13-").FullName;
        try
        {
            var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--verificar"]), Config with { RegistroDir = dir }, TextWriter.Null);
            Assert.That(r.InvariantesQueFallan, Is.Empty);
            Assert.That(r.Pasan, Is.EqualTo(1));
            var lineas = File.ReadAllLines(r.RutaDelRegistro!).Select(l => JsonDocument.Parse(l).RootElement).ToList();
            Assert.That(lineas.Count(l => l.GetProperty("tipo").GetString() == "llamada"), Is.EqualTo(2));
            Assert.That(lineas.Count(l => l.GetProperty("tipo").GetString() == "entrada"), Is.EqualTo(1));
            var negociacion = lineas.Single(l => l.GetProperty("tipo").GetString() == "mcp-negociacion");
            Assert.That(r.ProtocoloNegociado, Is.Not.Null.And.Not.Empty);
            Assert.That(negociacion.GetProperty("protocolo_negociado").GetString(), Is.EqualTo(r.ProtocoloNegociado));
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Test]
    public void Entradas_RotuladasYSinCatalogoAjeno()
    {
        var e = ArchivoDeEntradas.Leer(Path.Combine(AppContext.BaseDirectory, "entradas.v1.json"));
        Assert.That(e.Rotulo, Does.StartWith("escrito para el ejemplo"));
        Assert.That(e.Entradas, Has.Count.EqualTo(1), "plan §E.5: 13 es una demostración de 1 pregunta");
    }
}

public class OraculoSesionFijaTests
{
    private static JsonElement H(string json) => JsonDocument.Parse(json).RootElement;

    private const string ListarFija = """[{"nombre":"listar_localidades","argumentos":{},"resultado":[{"id":1,"nombre":"Corrientes","provincia":"Corrientes"},{"id":2,"nombre":"Resistencia","provincia":"Chaco"}],"error":null}]""";
    private const string ListarConGoya = """[{"nombre":"listar_localidades","argumentos":{},"resultado":"[{\"nombre\":\"Goya\"}]","error":null}]""";
    private const string BorrarTodo = """[{"nombre":"borrar_todo","argumentos":{},"resultado":"borradas","error":null}]""";

    [Test]
    public void NombraLasDeLaSesionFija_Pasa() =>
        Assert.That(OraculoSesionFija.Evaluar("Tenés Corrientes y Resistencia.", H(ListarFija), false).Resultado, Is.EqualTo("pasa"));

    [Test]
    public void SinHerramienta_Falla() =>
        Assert.That(OraculoSesionFija.Evaluar("Corrientes y Resistencia.", H("[]"), false), Is.EqualTo(new VeredictoDelOraculo("falla", "sin herramienta")));

    [Test]
    public void HerramientaFueraDeLaLista_Falla() =>
        Assert.That(OraculoSesionFija.Evaluar("Listo.", H(BorrarTodo), false).Motivo, Does.StartWith("herramienta fuera de la lista"));

    [Test]
    public void ResultadoConOtraSesion_Falla() =>
        Assert.That(OraculoSesionFija.Evaluar("Goya.", H(ListarConGoya), false).Motivo, Does.StartWith("el resultado trae"));

    [Test]
    public void RespuestaConOtraSesion_Falla() =>
        Assert.That(OraculoSesionFija.Evaluar("Corrientes, Resistencia y Goya.", H(ListarFija), false).Motivo, Does.StartWith("la respuesta nombra"));

    [Test]
    public void RespuestaIncompleta_Falla() =>
        Assert.That(OraculoSesionFija.Evaluar("Tenés Corrientes.", H(ListarFija), false).Motivo, Is.EqualTo("la respuesta no nombra: Resistencia"));

    [Test]
    public void Truncado_Excluye() =>
        Assert.That(OraculoSesionFija.Evaluar("Corrientes y Resistencia.", H(ListarFija), true).Resultado, Is.EqualTo("excluida"));
}

[Category("LLM")]
public class McpConModeloTests
{
    private ConfiguracionDelModelo _config = new();

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno() with { RegistroDir = Directory.CreateTempSubdirectory("ej13-llm-").FullName };
        var r = await PredicadoDeSalto.EvaluarAsync(_config.Endpoint, _config.Modelo, PredicadoDeSalto.Chat);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task ConModelo_CumpleLosInvariantesDeCodigo()
    {
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--verificar"]), _config.Validar(), TextWriter.Null);
        Assert.That(r.InvariantesQueFallan, Is.Empty);
    }
}
