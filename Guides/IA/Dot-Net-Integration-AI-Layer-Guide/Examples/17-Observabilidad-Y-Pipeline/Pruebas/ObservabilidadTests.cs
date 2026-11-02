// Nuevo: pruebas del ejemplo 17. Las de cableado corren sin servidor (cliente guionado y exportador
// en memoria); la de [Category("LLM")] se salta con el motivo del predicado si no hay modelo.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

// Los spans se capturan desde un ActivitySource global: estas pruebas no corren en paralelo.
[NonParallelizable]
public class ObservabilidadTests
{
    private string _dir = string.Empty;

    [SetUp]
    public void CrearDirectorio() => _dir = Directory.CreateTempSubdirectory("ej17-").FullName;

    [TearDown]
    public void BorrarDirectorio() => Directory.Delete(_dir, recursive: true);

    private ConfiguracionDelModelo Config() => new() { RegistroDir = _dir };

    private static EntradaDeEjemplo Entrada() =>
        EntradaDeEjemplo.Cargar(Path.Combine(AppContext.BaseDirectory, "entradas.v1.json")).Single();

    private IChatClient Guionado(GuionChatClient? guion = null) =>
        FabricaDeClientes.Envolver(guion ?? Corrida.CrearGuion(), Config(), Adaptador.OllamaSharp);

    [Test]
    public async Task Fake_CorridaCompleta_CumpleTodosLosInvariantes()
    {
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--verificar"]), Config(), TextWriter.Null);

        Assert.That(r.InvariantesQueFallan, Is.Empty);
        Assert.That(r.Veredictos, Is.EqualTo(new Dictionary<string, string> { ["e01"] = "pasa" }));
        Assert.That(File.ReadAllLines(r.RutaDelRegistro), Has.Length.EqualTo(3), "dos llamadas y una entrada");
    }

    [Test]
    public async Task CacheAfuera_TresPedidos_DosLlamadasYDosSpans_ElAciertoNoApareceEnLaTelemetria()
    {
        var obs = await Corrida.ObservarAsync(Guionado(), OrdenDeCapas.CacheAfuera, Ambientes.Produccion, LogLevel.Debug, Entrada().Pedidos);

        Assert.That(obs.Llamadas, Has.Count.EqualTo(2));
        Assert.That(obs.PedidoDeCadaLlamada, Is.EqualTo(new[] { "p1", "p3" }));
        Assert.That(obs.Aciertos, Is.EqualTo(new[] { "p2" }));
        Assert.That(obs.Spans, Has.Count.EqualTo(2));
        // La respuesta del acierto es la misma que la del primer pedido.
        Assert.That(obs.Respuestas[1].Text, Is.EqualTo(obs.Respuestas[0].Text));
    }

    [Test]
    public async Task TelemetriaAfuera_TresPedidos_DosLlamadasYTresSpans_ElAciertoTraeTokens()
    {
        var obs = await Corrida.ObservarAsync(Guionado(), OrdenDeCapas.TelemetriaAfuera, Ambientes.Produccion, LogLevel.Debug, Entrada().Pedidos);

        Assert.That(obs.Llamadas, Has.Count.EqualTo(2));
        Assert.That(obs.Spans, Has.Count.EqualTo(3));
        // Para medir costo real hay que contar las llamadas que llegan al modelo, no los spans (N5 V3).
        Assert.That(CapturaDeTelemetria.Atributo(obs.Spans[1], "gen_ai.usage.input_tokens"), Is.Not.Null);
    }

    [Test]
    public async Task OtraTemperatura_NoAcierta_PorqueLasOpcionesSonParteDeLaClave()
    {
        var pedido = Entrada().Pedidos[0];
        var obs = await Corrida.ObservarAsync(Guionado(), OrdenDeCapas.CacheAfuera, Ambientes.Produccion, LogLevel.Debug,
            [pedido, pedido with { Id = "p3", Temperatura = 0.7f }]);

        Assert.That(obs.Aciertos, Is.Empty);
        Assert.That(obs.Llamadas, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ContenidoApagado_LosSpansNoTraenMensajes_YEnDesarrolloSi()
    {
        var produccion = await Corrida.ObservarAsync(Guionado(), OrdenDeCapas.CacheAfuera, Ambientes.Produccion, LogLevel.Debug, Entrada().Pedidos);
        var desarrollo = await Corrida.ObservarAsync(Guionado(), OrdenDeCapas.CacheAfuera, Ambientes.Desarrollo, LogLevel.Debug, Entrada().Pedidos);

        Assert.That(produccion.Spans, Has.None.Matches<System.Diagnostics.Activity>(CapturaDeTelemetria.TraeContenido));
        Assert.That(produccion.Spans.SelectMany(s => s.TagObjects).Select(t => t.Value?.ToString() ?? string.Empty),
            Has.None.Contains(Entrada().Pedidos[0].Texto));
        Assert.That(desarrollo.Spans, Has.All.Matches<System.Diagnostics.Activity>(CapturaDeTelemetria.TraeContenido));
    }

    [Test]
    public void Ambiente_SoloDevelopmentEnciendeElContenido()
    {
        Assert.That(Ambientes.ContenidoEnTelemetria("Development"), Is.True);
        Assert.That(Ambientes.ContenidoEnTelemetria("development"), Is.True);
        Assert.That(Ambientes.ContenidoEnTelemetria("Production"), Is.False);
        Assert.That(Ambientes.ContenidoEnTelemetria("Staging"), Is.False);
    }

    [Test]
    public async Task Logs_EnDebugSinTexto_EnTraceConTexto()
    {
        var texto = Entrada().Pedidos[0].Texto;
        var debug = await Corrida.ObservarAsync(Guionado(), OrdenDeCapas.CacheAfuera, Ambientes.Produccion, LogLevel.Debug, Entrada().Pedidos);
        var trace = await Corrida.ObservarAsync(Guionado(), OrdenDeCapas.CacheAfuera, Ambientes.Produccion, LogLevel.Trace, Entrada().Pedidos);

        Assert.That(debug.Logs, Is.Not.Empty);
        Assert.That(debug.Logs.Select(l => l.Texto), Has.None.Contains(texto));
        Assert.That(trace.Logs.Select(l => l.Texto), Has.Some.Contains(texto));
    }

    [Test]
    public async Task Metricas_MEAIPublicaLosNombresDeSemconv141_QueNoSonLosDeLaConvencionVigente()
    {
        var obs = await Corrida.ObservarAsync(Guionado(), OrdenDeCapas.CacheAfuera, Ambientes.Produccion, LogLevel.Debug, Entrada().Pedidos);

        Assert.That(obs.Instrumentos, Is.EquivalentTo(ConvencionGenAi.Semconv141));
        Assert.That(obs.MetricasMedidas, Does.Contain("gen_ai.client.token.usage"));
        Assert.That(ConvencionGenAi.Vigente, Does.Not.Contain("gen_ai.client.token.usage"));
        var enLasDos = ConvencionGenAi.Comparar(obs.Instrumentos).Where(f => f.EnV141 && f.EnVigente).Select(f => f.Nombre);
        Assert.That(enLasDos, Is.EqualTo(new[] { "gen_ai.client.operation.duration" }));
    }

    [Test]
    public async Task CapaDeOpciones_DebajoDelPipeline_ApagaElRazonamientoYFijaNumCtx()
    {
        var espia = new ClienteEspia(Corrida.CrearGuion());
        await Corrida.ObservarAsync(FabricaDeClientes.Envolver(espia, Config(), Adaptador.OllamaSharp),
            OrdenDeCapas.CacheAfuera, Ambientes.Produccion, LogLevel.Debug, Entrada().Pedidos);

        Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(2), "el acierto de caché no llega al adaptador");
        Assert.That(espia.OpcionesVistas, Has.All.Matches<ChatOptions?>(o => o?.Reasoning?.Effort == ReasoningEffort.None));
        Assert.That(espia.OpcionesVistas.Select(OpcionesDelProveedor.NumCtxDe), Has.All.EqualTo(ConfiguracionDelModelo.NumCtxPorDefecto));
        Assert.That(espia.OpcionesVistas.Select(o => o?.Temperature), Is.EqualTo(new float?[] { 0f, 0.7f }));
    }

    [Test]
    public async Task Telemetria_ArribaDeLaCapa_VeLaTemperaturaQuePidioLaAplicacion()
    {
        var obs = await Corrida.ObservarAsync(Guionado(), OrdenDeCapas.CacheAfuera, Ambientes.Produccion, LogLevel.Debug, Entrada().Pedidos);

        Assert.That(CapturaDeTelemetria.Atributo(obs.Spans[0], "gen_ai.request.temperature"), Is.Null);
        Assert.That(CapturaDeTelemetria.Atributo(obs.Spans[1], "gen_ai.request.temperature"), Is.EqualTo("0.7"));
    }

    [Test]
    public void Pipeline_ConUnRegistradorQueNoEnvuelveALaCapa_Rechaza()
    {
        var capa = Guionado();
        Assert.Throws<ArgumentException>(() => PipelineObservado.Construir(capa, OrdenDeCapas.CacheAfuera,
            PipelineObservado.CacheEnMemoria(), new LogsEnMemoria(LogLevel.Debug), Ambientes.Produccion, new RegistradorDeLlamadas(Guionado()),
            new ContextoDeLaCache("sesion-1").ValoresDeLaClave));
    }

    [Test]
    public void Oraculo_DetectaUnAciertoQueLlegaAlModeloYContenidoFueraDeDesarrollo()
    {
        LlamadaObservada L(string pedido, float t, bool contenido = false, string ambiente = "Production") =>
            new(OraculoCacheYTelemetria.Paso(pedido, true, contenido, ambiente), "texto", t, false);

        Assert.That(OraculoCacheYTelemetria.Evaluar([L("p1", 0f), L("p3", 0.7f)]).Resultado, Is.EqualTo(ResultadoDeEntrada.Pasa));
        Assert.That(OraculoCacheYTelemetria.Evaluar([L("p1", 0f), L("p2", 0f), L("p3", 0.7f)]).Resultado, Is.EqualTo(ResultadoDeEntrada.Falla));
        Assert.That(OraculoCacheYTelemetria.Evaluar([L("p1", 0f, contenido: true), L("p3", 0.7f, contenido: true)]).Resultado, Is.EqualTo(ResultadoDeEntrada.Falla));
        Assert.That(OraculoCacheYTelemetria.Evaluar([L("p1", 0f, true, "Development"), L("p3", 0.7f, true, "Development")]).Resultado, Is.EqualTo(ResultadoDeEntrada.Pasa));
        Assert.That(OraculoCacheYTelemetria.Evaluar([L("p1", 0f), L("p3", 0f)]).Resultado, Is.EqualTo(ResultadoDeEntrada.Falla));
        Assert.That(OraculoCacheYTelemetria.Evaluar([]).Resultado, Is.EqualTo(ResultadoDeEntrada.Excluida));
        Assert.That(OraculoCacheYTelemetria.Evaluar([L("p1", 0f) with { TruncadoPosible = true }, L("p3", 0.7f)]).Resultado, Is.EqualTo(ResultadoDeEntrada.Excluida));
    }

    [Test]
    public void Compartido_SinNumCtxDelArnes_Rechaza()
    {
        var entorno = new Dictionary<string, string> { ["OLLAMA_SERVIDOR"] = "compartido" };
        Assert.Throws<ConfiguracionInvalidaException>(() =>
            ConfiguracionDelModelo.DesdeEntorno(n => entorno.GetValueOrDefault(n)).Validar());
    }

    [Test]
    public async Task Plazo_Vencido_ExcluyeLaEntradaSinLlamar()
    {
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--plazo-s", "0"]), Config(), TextWriter.Null);

        Assert.That(r.Veredictos.Values, Has.All.EqualTo("excluida"));
        Assert.That(File.ReadAllLines(r.RutaDelRegistro), Has.Length.EqualTo(1).And.All.Contains("\"motivo\":\"plazo\""));
    }
}

[Category("LLM")]
[NonParallelizable]
public class ObservabilidadConModeloTests
{
    private ConfiguracionDelModelo _config = new();

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno() with { RegistroDir = Directory.CreateTempSubdirectory("ej17-llm-").FullName };
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
