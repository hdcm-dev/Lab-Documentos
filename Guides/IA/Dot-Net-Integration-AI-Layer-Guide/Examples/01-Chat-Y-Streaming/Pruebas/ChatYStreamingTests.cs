// Nuevo: pruebas del ejemplo 01. Las de cableado corren sin servidor; la de [Category("LLM")] se
// salta con el motivo del predicado si no hay modelo.
using System.Reflection;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Infrastructure;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

public class ChatYStreamingTests
{
    private string _dir = string.Empty;

    [SetUp]
    public void CrearDirectorio() =>
        _dir = Directory.CreateTempSubdirectory("ej01-").FullName;

    [TearDown]
    public void BorrarDirectorio() => Directory.Delete(_dir, recursive: true);

    private ConfiguracionDelModelo Config() => new() { RegistroDir = _dir };

    private static IReadOnlyList<EntradaDeEjemplo> Entradas() =>
        OraculoDeCampos.Cargar(Path.Combine(AppContext.BaseDirectory, "entradas.v1.json"));

    [Test]
    public async Task Fake_CorridaCompleta_CumpleTodosLosInvariantes()
    {
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--verificar"]), Config(), TextWriter.Null);

        Assert.That(r.InvariantesQueFallan, Is.Empty);
        Assert.That(r.Veredictos.Values, Has.All.EqualTo("pasa"));
        var lineas = File.ReadAllLines(r.RutaDelRegistro);
        Assert.That(lineas, Has.Length.EqualTo(4), "dos llamadas y dos entradas");
    }

    [Test]
    public async Task Historial_LaSegundaVueltaReenviaLaConversacionEntera()
    {
        var guion = Corrida.CrearGuion();
        var asistente = new AsistenteDeAyuda(FabricaDeClientes.Envolver(guion, Config(), Adaptador.OllamaSharp));

        var v1 = await asistente.PreguntarAsync("primera", _ => { });
        await asistente.PreguntarAsync("segunda", _ => { });

        var recibidos = guion.Llamadas[1].Mensajes;
        Assert.That(recibidos.Select(m => m.Role), Is.EqualTo(new[] { ChatRole.User, ChatRole.Assistant, ChatRole.User }));
        Assert.That(recibidos[1].Text, Is.EqualTo(v1.Respuesta.Text));
        Assert.That(asistente.Historial, Has.Count.EqualTo(4));
    }

    [Test]
    public async Task Streaming_LlegaEnCincoFragmentos_QueArmanLaRespuesta()
    {
        var asistente = new AsistenteDeAyuda(FabricaDeClientes.Envolver(Corrida.CrearGuion(), Config(), Adaptador.OllamaSharp));
        var vistos = new List<string>();

        var v = await asistente.PreguntarAsync("¿Qué pide el alta?", vistos.Add);

        Assert.That(vistos, Has.Count.EqualTo(5));
        Assert.That(string.Concat(vistos), Is.EqualTo(v.Respuesta.Text));
        Assert.That(v.Respuesta.FinishReason, Is.EqualTo(ChatFinishReason.Stop));
    }

    [Test]
    public async Task CapaDeOpciones_ApagaElRazonamientoYFijaNumCtx_DebajoDeLaAplicacion()
    {
        var espia = new ClienteEspia(Corrida.CrearGuion());
        var asistente = new AsistenteDeAyuda(FabricaDeClientes.Envolver(espia, Config(), Adaptador.OllamaSharp));

        await asistente.PreguntarAsync("uno", _ => { });
        await asistente.PreguntarAsync("dos", _ => { });

        Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(2));
        Assert.That(espia.OpcionesVistas, Has.All.Matches<ChatOptions?>(o => o?.Reasoning?.Effort == ReasoningEffort.None));
        Assert.That(espia.OpcionesVistas.Select(OpcionesDelProveedor.NumCtxDe), Has.All.EqualTo(ConfiguracionDelModelo.NumCtxPorDefecto));
        Assert.That(espia.OpcionesVistas, Has.All.Matches<ChatOptions?>(o => o?.Instructions == AsistenteDeAyuda.Instrucciones));
    }

    [Test]
    public async Task VarianteNegativa_SinLaCapa_TextVacioYFinishLength()
    {
        Assert.That(await Corrida.VarianteNegativaAsync(Config(), TextWriter.Null), Is.Empty);

        var sinCapa = await new ModeloQueRazonaPorDefecto("x").GetResponseAsync("hola", new ChatOptions { MaxOutputTokens = 200 });
        Assert.That(sinCapa.Text, Is.Empty);
        Assert.That(sinCapa.FinishReason, Is.EqualTo(ChatFinishReason.Length));
    }

    [Test]
    public void Oraculo_NormalizaTildesYExigeCadaCampo()
    {
        var alta = Entradas().Single(e => e.Id == "e01");

        Assert.That(OraculoDeCampos.Faltantes("Nombre, PROVINCIA, Código Postal y habitantes.", alta), Is.Empty);
        Assert.That(OraculoDeCampos.Faltantes("Nombre, provincia, CP y habitantes.", alta), Is.Empty);
        Assert.That(OraculoDeCampos.Faltantes("Nombre, provincia y habitantes.", alta), Is.EqualTo(new[] { "codigo postal" }));
        // «cp» va como palabra entera: no vale dentro de otra palabra.
        Assert.That(OraculoDeCampos.Faltantes("Nombre, provincia, acpto y habitantes.", alta), Is.EqualTo(new[] { "codigo postal" }));
    }

    [Test]
    public void Entradas_PidenUnCampoPorCadaRequisitoDeLaPolicy()
    {
        static int Requisitos(Type t) => t.GetProperties(BindingFlags.Public | BindingFlags.Static).Count(p => p.Name.StartsWith("Requisito", StringComparison.Ordinal));

        var entradas = Entradas();
        Assert.That(entradas.Single(e => e.Politica == nameof(LocalidadPolicy)).Campos, Has.Count.EqualTo(Requisitos(typeof(LocalidadPolicy))));
        Assert.That(entradas.Single(e => e.Politica == nameof(EncuestaPolicy)).Campos, Has.Count.EqualTo(Requisitos(typeof(EncuestaPolicy))));
    }

    [Test]
    public void Compartido_SinNumCtxDelArnes_Rechaza()
    {
        var entorno = new Dictionary<string, string> { ["OLLAMA_SERVIDOR"] = "compartido" };
        Assert.Throws<ConfiguracionInvalidaException>(() =>
            ConfiguracionDelModelo.DesdeEntorno(n => entorno.GetValueOrDefault(n)).Validar());
    }

    [Test]
    public async Task Plazo_Vencido_ExcluyeLasEntradasSinLlamar()
    {
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--plazo-s", "0"]), Config(), TextWriter.Null);

        Assert.That(r.Veredictos.Values, Has.All.EqualTo("excluida"));
        Assert.That(File.ReadAllLines(r.RutaDelRegistro), Has.All.Contains("\"motivo\":\"plazo\""));
    }
}

[Category("LLM")]
public class ChatYStreamingConModeloTests
{
    private ConfiguracionDelModelo _config = new();

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno() with { RegistroDir = Directory.CreateTempSubdirectory("ej01-llm-").FullName };
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
