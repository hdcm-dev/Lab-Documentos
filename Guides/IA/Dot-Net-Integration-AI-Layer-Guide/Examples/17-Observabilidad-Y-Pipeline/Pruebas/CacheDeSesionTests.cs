// Nuevo (recuperación 2026-10-28, REC-12; patrón de la línea archivada del 2026-10-28, ver Procedencia).
// Casos rojos de la caché de respuestas con la clave por defecto de UseDistributedCache, y la misma
// secuencia con la clave nueva (sesión y versión de los datos). Todo sin servidor: cliente guionado
// envuelto en la capa única, caché en memoria compartida como la compartiría una caché distribuida.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Infrastructure;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

[NonParallelizable]
public class CacheDeSesionTests
{
    private const string Pregunta = "¿Cuántas localidades hay cargadas en Corrientes?";
    private const string RespuestaParaA = "Hay 1 localidad cargada en Corrientes: Corrientes.";
    private const string RespuestaParaB = "Hay 2 localidades cargadas en Corrientes: Corrientes y Goya.";

    private string _dir = string.Empty;

    [SetUp]
    public void CrearDirectorio() => _dir = Directory.CreateTempSubdirectory("ej17-cache-").FullName;

    [TearDown]
    public void BorrarDirectorio() => Directory.Delete(_dir, recursive: true);

    private IChatClient ConCapaUnica(GuionChatClient guion) =>
        FabricaDeClientes.Envolver(guion, new ConfiguracionDelModelo { RegistroDir = _dir }, Adaptador.OllamaSharp);

    /// <summary>La clave por defecto de M.E.AI: mensajes y opciones, nada de la sesión ni de los datos.</summary>
    private static IChatClient ConClavePorDefecto(IChatClient capa, IDistributedCache cache) =>
        new ChatClientBuilder(capa).UseDistributedCache(cache).Build();

    /// <summary>El pipeline del ejemplo (caché más afuera), con la clave nueva.</summary>
    private static IChatClient ConClaveNueva(IChatClient capa, IDistributedCache cache, Func<object?[]> valores) =>
        PipelineObservado.Construir(capa, OrdenDeCapas.CacheAfuera, cache, new LogsEnMemoria(LogLevel.Debug),
            Ambientes.Produccion, new RegistradorDeLlamadas(capa), valores);

    [Test]
    public async Task CasoRojo_ClavePorDefecto_DosSesiones_BRecibeLaRespuestaDeA()
    {
        var guion = new GuionChatClient(RespuestaParaA, RespuestaParaB);
        var capa = ConCapaUnica(guion);
        var cache = PipelineObservado.CacheEnMemoria();   // compartida por las dos sesiones
        var deA = ConClavePorDefecto(capa, cache);
        var deB = ConClavePorDefecto(capa, cache);

        var a = await deA.GetResponseAsync(Pregunta);
        var b = await deB.GetResponseAsync(Pregunta);

        Assert.Multiple(() =>
        {
            Assert.That(guion.Llamadas, Has.Count.EqualTo(1), "B no llegó al modelo: acertó con lo que guardó A");
            Assert.That(b.Text, Is.EqualTo(a.Text).And.EqualTo(RespuestaParaA), "B recibe la respuesta de A");
        });
    }

    [Test]
    public async Task ClaveNueva_DosSesiones_BNoRecibeLaRespuestaDeA()
    {
        var guion = new GuionChatClient(RespuestaParaA, RespuestaParaB);
        var capa = ConCapaUnica(guion);
        var cache = PipelineObservado.CacheEnMemoria();
        var deA = ConClaveNueva(capa, cache, new ContextoDeLaCache("sesion-A").ValoresDeLaClave);
        var deB = ConClaveNueva(capa, cache, new ContextoDeLaCache("sesion-B").ValoresDeLaClave);

        var a = await deA.GetResponseAsync(Pregunta);
        var b = await deB.GetResponseAsync(Pregunta);
        var aOtraVez = await deA.GetResponseAsync(Pregunta);

        Assert.Multiple(() =>
        {
            Assert.That(guion.Llamadas, Has.Count.EqualTo(2), "las dos sesiones llegan al modelo");
            Assert.That(b.Text, Is.EqualTo(RespuestaParaB));
            Assert.That(aOtraVez.Text, Is.EqualTo(a.Text), "dentro de la misma sesión, la caché sigue acertando");
            Assert.That(deA.GetService<CacheConContador>()!.Aciertos, Is.EqualTo(1));
            Assert.That(deB.GetService<CacheConContador>()!.Aciertos, Is.Zero);
        });
    }

    /// <summary>Una sola instancia que lee la sesión del pedido en curso: también aísla.</summary>
    [Test]
    public async Task ClaveNueva_UnaInstancia_LaSesionSeLeeEnCadaPedido()
    {
        var guion = new GuionChatClient(RespuestaParaA, RespuestaParaB);
        var contexto = new ContextoDeLaCache("sesion-A");
        var chat = ConClaveNueva(ConCapaUnica(guion), PipelineObservado.CacheEnMemoria(), contexto.ValoresDeLaClave);

        await chat.GetResponseAsync(Pregunta);
        contexto.SesionId = "sesion-B";
        var b = await chat.GetResponseAsync(Pregunta);

        Assert.That(guion.Llamadas, Has.Count.EqualTo(2));
        Assert.That(b.Text, Is.EqualTo(RespuestaParaB));
    }

    /// <summary>
    /// Casos rojos de la escritura: después de una escritura confirmada, la clave mal armada acierta y sirve
    /// la respuesta de antes. Con datos que cambian, un acierto no vuelve a leerlos.
    /// </summary>
    [TestCase("clave por defecto")]
    [TestCase("versión leída al crear la instancia")]
    public async Task CasoRojo_EscrituraConfirmada_ClaveMalArmada_SirveLaRespuestaVieja(string caso)
    {
        var guion = new GuionChatClient(RespuestaParaA, RespuestaParaB);
        var capa = ConCapaUnica(guion);
        var contexto = new ContextoDeLaCache("sesion-A");
        var fijos = contexto.ValoresDeLaClave();
        var chat = caso == "clave por defecto"
            ? ConClavePorDefecto(capa, PipelineObservado.CacheEnMemoria())
            : ConClaveNueva(capa, PipelineObservado.CacheEnMemoria(), () => fijos);

        var antes = await chat.GetResponseAsync(Pregunta);
        contexto.RegistrarEscritura();   // se dio de alta Goya
        var despues = await chat.GetResponseAsync(Pregunta);

        Assert.Multiple(() =>
        {
            Assert.That(guion.Llamadas, Has.Count.EqualTo(1), $"{caso}: acierto después de la escritura");
            Assert.That(despues.Text, Is.EqualTo(antes.Text).And.EqualTo(RespuestaParaA), "respuesta vieja");
        });
    }

    [Test]
    public async Task ClaveNueva_EscrituraConfirmada_NoAcierta_YVuelveAlModelo()
    {
        var guion = new GuionChatClient(RespuestaParaA, RespuestaParaB);
        var contexto = new ContextoDeLaCache("sesion-A");
        var chat = ConClaveNueva(ConCapaUnica(guion), PipelineObservado.CacheEnMemoria(), contexto.ValoresDeLaClave);

        await chat.GetResponseAsync(Pregunta);
        var cacheada = await chat.GetResponseAsync(Pregunta);
        contexto.RegistrarEscritura();
        var despues = await chat.GetResponseAsync(Pregunta);

        Assert.Multiple(() =>
        {
            Assert.That(cacheada.Text, Is.EqualTo(RespuestaParaA), "antes de la escritura, acierto");
            Assert.That(guion.Llamadas, Has.Count.EqualTo(2), "después de la escritura, fallo de caché");
            Assert.That(despues.Text, Is.EqualTo(RespuestaParaB));
            Assert.That(chat.GetService<CacheConContador>()!.Aciertos, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Contador_PorGetResponseAsync_CuentaElAcierto()
    {
        var guion = new GuionChatClient(RespuestaParaA, RespuestaParaB);
        var chat = ConClaveNueva(ConCapaUnica(guion), PipelineObservado.CacheEnMemoria(), new ContextoDeLaCache("sesion-A").ValoresDeLaClave);

        await chat.GetResponseAsync(Pregunta);
        await chat.GetResponseAsync(Pregunta);

        Assert.That(guion.Llamadas, Has.Count.EqualTo(1));
        Assert.That(chat.GetService<CacheConContador>()!.Aciertos, Is.EqualTo(1));
    }

    /// <summary>Streaming: con CoalesceStreamingUpdates por defecto (true) y con false, el contador da 1.</summary>
    [TestCase(null)]
    [TestCase(false)]
    public async Task Contador_PorStreaming_CuentaElAcierto_ConCoalesceStreamingUpdatesEnLosDosValores(bool? coalesce)
    {
        var guion = new GuionChatClient(RespuestaParaA, RespuestaParaB);
        var chat = ConClaveNueva(ConCapaUnica(guion), PipelineObservado.CacheEnMemoria(), new ContextoDeLaCache("sesion-A").ValoresDeLaClave);
        var cache = chat.GetService<CacheConContador>()!;
        Assert.That(cache.CoalesceStreamingUpdates, Is.True, "el valor por defecto de M.E.AI 10.11.0");
        if (coalesce is bool c) cache.CoalesceStreamingUpdates = c;

        var fria = await Unir(chat);
        var caliente = await Unir(chat);

        Assert.Multiple(() =>
        {
            Assert.That(caliente, Is.EqualTo(fria));
            Assert.That(guion.Llamadas, Has.Count.EqualTo(1), "la segunda vuelve de la caché");
            Assert.That(cache.Aciertos, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Caso rojo del contador: una caché que sólo sobrescribe ReadCacheAsync. Con CoalesceStreamingUpdates en
    /// false el acierto por streaming pasa por ReadCacheStreamingAsync y el contador queda en 0, aunque la
    /// caché acertó; con true, da 1. La prueba afirma las dos cosas.
    /// </summary>
    [TestCase(true, 1)]
    [TestCase(false, 0)]
    public async Task CasoRojo_ContadorSoloEnReadCacheAsync_ConCoalesceFalse_CuentaCero(bool coalesce, int aciertosEsperados)
    {
        var guion = new GuionChatClient(RespuestaParaA, RespuestaParaB);
        var cache = new SoloReadCacheAsync(ConCapaUnica(guion), PipelineObservado.CacheEnMemoria()) { CoalesceStreamingUpdates = coalesce };

        await Unir(cache);
        await Unir(cache);

        Assert.Multiple(() =>
        {
            Assert.That(guion.Llamadas, Has.Count.EqualTo(1), "la caché acertó");
            Assert.That(cache.Aciertos, Is.EqualTo(aciertosEsperados));
        });
    }

    private static async Task<string> Unir(IChatClient chat)
    {
        var texto = string.Empty;
        await foreach (var u in chat.GetStreamingResponseAsync(Pregunta)) texto += u.Text;
        return texto;
    }

    private sealed class SoloReadCacheAsync(IChatClient interno, IDistributedCache cache) : DistributedCachingChatClient(interno, cache)
    {
        public int Aciertos { get; private set; }

        protected override async Task<ChatResponse?> ReadCacheAsync(string key, CancellationToken cancellationToken)
        {
            var r = await base.ReadCacheAsync(key, cancellationToken);
            if (r is not null) Aciertos++;
            return r;
        }
    }
}
