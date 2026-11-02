// Nuevo: pruebas de cableado de la capa única de opciones y de la configuración (plan §E.1).
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;

namespace MovilidadUrbana.Asistente.Comun.Pruebas.Cableado;

public class CapaDeOpcionesTests
{
    private static readonly ConfiguracionDelModelo Config = new();

    [Test]
    public async Task TodaLlamada_SaleConRazonamientoApagadoYNumCtx()
    {
        var espia = new ClienteEspia(new GuionChatClient("uno", "dos", "tres"));
        var cliente = FabricaDeClientes.Envolver(espia, Config, Adaptador.OllamaSharp);

        await cliente.GetResponseAsync("hola");
        await cliente.GetResponseAsync("hola", new ChatOptions { Temperature = 0.7f });
        await foreach (var _ in cliente.GetStreamingResponseAsync("hola")) { }

        Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(3));
        Assert.That(espia.OpcionesVistas, Has.All.Matches<ChatOptions?>(o => o?.Reasoning?.Effort == ReasoningEffort.None));
        Assert.That(espia.OpcionesVistas.Select(OpcionesDelProveedor.NumCtxDe), Has.All.EqualTo(ConfiguracionDelModelo.NumCtxPorDefecto));
        // Lo que el que llama fija explícitamente se respeta: la capa sólo completa lo que falta.
        Assert.That(espia.OpcionesVistas[1]!.Temperature, Is.EqualTo(0.7f));
        Assert.That(espia.OpcionesVistas[0]!.Temperature, Is.EqualTo(0f));
    }

    [Test]
    public async Task ViaV1_NoMandaNumCtx_PeroSiApagaElRazonamiento()
    {
        var espia = new ClienteEspia(new GuionChatClient("uno"));
        var cliente = FabricaDeClientes.Envolver(espia, Config, Adaptador.OpenAIv1);

        await cliente.GetResponseAsync("hola");

        Assert.That(OpcionesDelProveedor.NumCtxDe(espia.OpcionesVistas[0]), Is.Null);
        Assert.That(espia.OpcionesVistas[0]!.Reasoning?.Effort, Is.EqualTo(ReasoningEffort.None));
    }

    [Test]
    public void Compartido_SinNumCtxDelArnes_Rechaza()
    {
        var config = ConfiguracionDelModelo.DesdeEntorno(n => n == "OLLAMA_SERVIDOR" ? "compartido" : null);

        Assert.That(config.NumCtxOrigen, Is.EqualTo(OrigenDelNumCtx.Default));
        Assert.Throws<ConfiguracionInvalidaException>(() => config.Validar());
    }

    [Test]
    public void Compartido_ConNumCtxDelArnes_Acepta()
    {
        var entorno = new Dictionary<string, string>
        {
            ["OLLAMA_SERVIDOR"] = "compartido",
            ["OLLAMA_NUM_CTX"] = "65536",
            ["OLLAMA_NUM_CTX_ORIGEN"] = OrigenDelNumCtx.ArnesPsTrasCarga
        };
        var config = ConfiguracionDelModelo.DesdeEntorno(n => entorno.GetValueOrDefault(n)).Validar();

        Assert.That(config.NumCtx, Is.EqualTo(65536));
        Assert.That(config.NumCtxOrigen, Is.EqualTo(OrigenDelNumCtx.ArnesPsTrasCarga));
        Assert.That(config.ServidorTexto, Is.EqualTo("compartido"));
    }

    [Test]
    public void Defaults_SonLosDeLaTablaCanonica()
    {
        var config = ConfiguracionDelModelo.DesdeEntorno(_ => null).Validar();

        Assert.Multiple(() =>
        {
            Assert.That(config.Endpoint, Is.EqualTo("http://localhost:11434"));
            Assert.That(config.Modelo, Is.EqualTo("qwen3.5:4b"));
            Assert.That(config.Servidor, Is.EqualTo(TipoDeServidor.Propio));
            Assert.That(config.NumCtx, Is.EqualTo(8192));
            Assert.That(config.PropioModelo, Is.EqualTo("qwen3.5:4b"));
            Assert.That(config.EmbedModelo, Is.EqualTo("qwen3-embedding:0.6b"));
            Assert.That(config.EmbPlan, Is.EqualTo("EMB-B"));
            Assert.That(config.RegistroDir, Is.EqualTo("./registros"));
        });
    }

    [Test]
    public void EmbedEndpoint_PorDefectoEsElPropio_NuncaElDeChat()
    {
        var entorno = new Dictionary<string, string>
        {
            ["OLLAMA_ENDPOINT"] = "http://chat.invalid:11434",
            ["OLLAMA_PROPIO_ENDPOINT"] = "http://propio.invalid:11434"
        };
        var config = ConfiguracionDelModelo.DesdeEntorno(n => entorno.GetValueOrDefault(n));

        Assert.That(config.EmbedEndpoint, Is.EqualTo("http://propio.invalid:11434"));
    }

    [Test]
    public async Task Guion_StreamingEnCincoFragmentos()
    {
        var guion = new GuionChatClient([PasoDelGuion.TextoEnFragmentos("Pide ", "nombre, ", "provincia, ", "código postal ", "y habitantes.")]);
        var cliente = FabricaDeClientes.Envolver(guion, Config, Adaptador.OllamaSharp);

        var fragmentos = new List<string>();
        await foreach (var u in cliente.GetStreamingResponseAsync("¿Qué pide el alta?"))
        {
            if (!string.IsNullOrEmpty(u.Text)) fragmentos.Add(u.Text);
        }

        Assert.That(fragmentos, Has.Count.EqualTo(5));
        Assert.That(string.Concat(fragmentos), Is.EqualTo("Pide nombre, provincia, código postal y habitantes."));
    }
}
