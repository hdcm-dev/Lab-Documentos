// Nuevo: pruebas del ejemplo 02. Las de cableado corren sin servidor; la de [Category("LLM")] se
// salta con su motivo si el Ollama propio no responde o no tiene el modelo.
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;

namespace MovilidadUrbana.Asistente.Pruebas;

public class CambioDeAdaptadorTests
{
    private static readonly ConfiguracionDelModelo Config = new();

    private static async Task<ChatOptions?> OpcionesQueSalenPor(Adaptador adaptador)
    {
        var espia = new ClienteEspia(new GuionChatClient("Nombre, provincia, código postal y habitantes."));
        var asistente = new AsistenteDeAlta(FabricaDeClientes.Envolver(espia, Config, adaptador));
        await asistente.ResponderAsync(AsistenteDeAlta.Mensajes("¿Qué pide el alta?"));
        return espia.OpcionesVistas.Single();
    }

    [Test]
    public async Task ViaNativa_LlevaNumCtxYRazonamientoApagado()
    {
        var opciones = await OpcionesQueSalenPor(Adaptador.OllamaSharp);
        Assert.That(OpcionesDelProveedor.NumCtxDe(opciones), Is.EqualTo(ConfiguracionDelModelo.NumCtxPorDefecto));
        Assert.That(opciones!.Reasoning?.Effort, Is.EqualTo(ReasoningEffort.None));
        Assert.That(opciones.Instructions, Is.EqualTo(AsistenteDeAlta.Instrucciones));
    }

    [Test]
    public async Task ViaV1_NoLlevaNumCtx_PeroSiRazonamientoApagado()
    {
        var opciones = await OpcionesQueSalenPor(Adaptador.OpenAIv1);
        Assert.That(OpcionesDelProveedor.NumCtxDe(opciones), Is.Null, "por /v1 num_ctx no llega (H2-016)");
        Assert.That(opciones!.Reasoning?.Effort, Is.EqualTo(ReasoningEffort.None));
    }

    /// <summary>Con el modelo, el registro calcula las opciones efectivas; tienen que ser las que la capa manda.</summary>
    [TestCase(Adaptador.OllamaSharp)]
    [TestCase(Adaptador.OpenAIv1)]
    public async Task LasOpcionesDelRegistro_SonLasQueVeElEspia(Adaptador adaptador)
    {
        var vistas = await OpcionesQueSalenPor(adaptador);
        var calculadas = RaizDeComposicion.OpcionesEfectivas(adaptador, Config);
        Assert.That(OpcionesDelProveedor.NumCtxDe(calculadas), Is.EqualTo(OpcionesDelProveedor.NumCtxDe(vistas)));
        Assert.That(calculadas.Reasoning?.Effort, Is.EqualTo(vistas!.Reasoning?.Effort));
        Assert.That(calculadas.Temperature, Is.EqualTo(vistas.Temperature));
        Assert.That(calculadas.Seed, Is.EqualTo(vistas.Seed));
        Assert.That(calculadas.Instructions, Is.EqualTo(vistas.Instructions));
    }

    [Test]
    public void LosDosAdaptadores_InformanProveedoresDistintos_SinRed()
    {
        using var nativo = RaizDeComposicion.Crear(Adaptador.OllamaSharp, Config);
        using var v1 = RaizDeComposicion.Crear(Adaptador.OpenAIv1, Config);
        var a = nativo.GetService<ChatClientMetadata>();
        var b = v1.GetService<ChatClientMetadata>();
        Assert.That(a?.ProviderName, Is.Not.Null.And.Not.Empty);
        Assert.That(b?.ProviderName, Is.Not.Null.And.Not.Empty);
        Assert.That(a!.ProviderName, Is.Not.EqualTo(b!.ProviderName));
        Assert.That(b.DefaultModelId, Is.EqualTo(Config.PropioModelo));
    }

    [Test]
    public void ElModoFalso_ConservaLosMetadatosDelAdaptadorElegido()
    {
        using var guionado = RaizDeComposicion.Guionado(Adaptador.OpenAIv1, Config);
        using var real = RaizDeComposicion.Crear(Adaptador.OpenAIv1, Config);
        Assert.That(guionado.GetService<ChatClientMetadata>()?.ProviderName,
            Is.EqualTo(real.GetService<ChatClientMetadata>()?.ProviderName));
    }

    [TestCase(new string[0], 2)]
    [TestCase(new[] { "--adaptador", "ollamasharp" }, 1)]
    [TestCase(new[] { "--adaptador", "openai-v1" }, 1)]
    public void LaVia_SeEligePorArgumento(string[] resto, int cantidad) =>
        Assert.That(RaizDeComposicion.ViasPedidas(resto), Has.Count.EqualTo(cantidad));

    [Test]
    public void UnaViaDesconocida_SeRechaza() =>
        Assert.That(RaizDeComposicion.ViasPedidas(["--adaptador", "otra"]), Is.Null);

    /// <summary>El criterio de OA-D1: sólo la raíz de composición nombra los adaptadores (H-007).</summary>
    [Test]
    public void SoloProgramCs_NombraLosAdaptadores()
    {
        var carpeta = Path.GetDirectoryName(Path.GetDirectoryName(EsteArchivo()))!;
        var patron = new Regex("OllamaSharp|using OpenAI");
        var quienes = Directory.GetFiles(carpeta, "*.cs")
            .Where(f => patron.IsMatch(File.ReadAllText(f)))
            .Select(Path.GetFileName)
            .ToList();
        Assert.That(quienes, Is.EqualTo(new[] { "Program.cs" }));
    }

    private static string EsteArchivo([CallerFilePath] string ruta = "") => ruta;
}

[Category("LLM")]
public class CambioDeAdaptadorConModeloTests
{
    private ConfiguracionDelModelo _config = null!;

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno();
        var r = await PredicadoDeSalto.EvaluarAsync(_config.PropioEndpoint, _config.PropioModelo, PredicadoDeSalto.Chat);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task LasDosVias_Responden_ConProveedoresDistintos()
    {
        var proveedores = new List<string?>();
        foreach (var adaptador in new[] { Adaptador.OllamaSharp, Adaptador.OpenAIv1 })
        {
            using var cliente = RaizDeComposicion.Crear(adaptador, _config);
            var asistente = new AsistenteDeAlta(cliente);
            var respuesta = await asistente.ResponderAsync(AsistenteDeAlta.Mensajes("¿Qué datos pide el alta de una localidad?"));
            Assert.That(respuesta.Text, Is.Not.Empty);
            proveedores.Add(asistente.Metadatos?.ProviderName);
        }
        Assert.That(proveedores.Distinct().Count(), Is.EqualTo(2));
    }
}
