// Nuevo: la corrida entera en modo falso (guion + simulador de caché, duraciones inventadas).
// El simulador de dos ranuras reproduce la FORMA de lo observado en Ollama 0.40.11 (README); las
// cifras son inventadas.
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

public class CorridaFalsaTests
{
    private string _dir = null!;

    [SetUp]
    public void CrearDirectorio() => _dir = Directory.CreateTempSubdirectory("ej05-").FullName;

    [TearDown]
    public void BorrarDirectorio() => Directory.Delete(_dir, recursive: true);

    private async Task<(ResultadoDeLaCorrida Resultado, ClienteEspia Espia)> CorrerAsync(OrdenDeConsultas orden, int ranuras, Plazo? plazo = null)
    {
        var config = new ConfiguracionDelModelo { RegistroDir = _dir };
        var guion = new GuionChatClient([], "respuesta");
        var espia = new ClienteEspia(new SimuladorDeCacheDePrefijo(guion, ranuras));
        var cliente = FabricaDeClientes.Envolver(espia, config, Adaptador.OllamaSharp);
        List<Pregunta> preguntas = [.. Enumerable.Range(1, 6).Select(i => new Pregunta($"p0{i}", $"¿Qué código postal tiene la Localidad 0{i}?"))];
        var prefijo = PrefijoEstable.Construir(PadronDeLocalidades.ATexto(PadronDeLocalidades.Generar()));
        var n = 0;
        var plan = PlanDeConsultas.Armar(preguntas, prefijo, orden, () => $"{++n:x16}");
        using var registro = new EscritorDeRegistro(new ContextoDeCorrida { Ejemplo = "05-prueba", Configuracion = config, Adaptador = Adaptador.OllamaSharp, Modelo = "guion" });
        var corrida = new CorridaDelPrefijo(cliente, registro, _ => guion.Llamadas[^1].Opciones, config, "05-prueba");
        return (await corrida.CorrerAsync(preguntas, prefijo, plan, plazo ?? new Plazo(null)), espia);
    }

    [Test]
    public async Task TodaLlamada_PasaPorLaCapaUnica()
    {
        var (resultado, espia) = await CorrerAsync(OrdenDeConsultas.Alternado, 2);

        Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(13));
        Assert.That(espia.OpcionesVistas, Has.All.Matches<ChatOptions?>(o => o?.Reasoning?.Effort == ReasoningEffort.None));
        Assert.That(espia.OpcionesVistas.Select(OpcionesDelProveedor.NumCtxDe), Has.All.EqualTo(8192));
        Assert.That(resultado.InvariantesQueFallan, Is.Empty);
    }

    [Test]
    public async Task Alternado_ElPrimerParNoTieneNadaCacheado()
    {
        // El prefijo sobrevive a la consulta sin-prefijo intercalada, pero el primer con-prefijo no
        // encuentra nada en la caché: con el orden del plan, 6 de 6 es inalcanzable (README).
        var (resultado, _) = await CorrerAsync(OrdenDeConsultas.Alternado, 2);
        Assert.That(resultado.Veredictos["p01"].Resultado, Is.EqualTo(ResultadoDeEntrada.Falla));
        Assert.That(resultado.Veredictos.Where(kv => kv.Key != "p01").Select(kv => kv.Value.Resultado), Has.All.EqualTo(ResultadoDeEntrada.Pasa));
    }

    [Test]
    public async Task ConCalentamiento_SeisDeSeisAFavor_YLaVueltaVuelveAFrio()
    {
        var (resultado, _) = await CorrerAsync(OrdenDeConsultas.AlternadoConCalentamiento, 2);

        Assert.That(resultado.Veredictos.Values.Select(v => v.Resultado), Has.All.EqualTo(ResultadoDeEntrada.Pasa));
        var vuelta = resultado.Llamadas.Single(l => l.Paso == Pasos.VueltaAFrio);
        var conP06 = resultado.Llamadas.Single(l => l.Paso == Pasos.ConPrefijo && l.EntradaId == "p06");
        Assert.That(vuelta.PromptEvalDurationMs, Is.GreaterThan(10 * conP06.PromptEvalDurationMs!.Value));
        Assert.That(resultado.InvariantesQueFallan, Is.Empty);
    }

    [Test]
    public async Task EstadoCache_SaleDelValorObservado()
    {
        // REC-22: la composición de 05 (Envolver, con LecturaDelServidorEnLaRespuesta) lleva el
        // prompt_eval_cached_count al registro y estado_cache sale de ese valor, no de la deducción por
        // duración. En modo falso el valor lo informa el simulador (INVENTADO).
        var (resultado, _) = await CorrerAsync(OrdenDeConsultas.Alternado, 2);

        Assert.That(resultado.Llamadas, Has.Count.EqualTo(13));
        Assert.That(resultado.Llamadas.Select(l => l.PromptEvalCachedCount), Has.None.Null);
        Assert.That(resultado.Llamadas.Select(l => l.EstadoCache), Has.None.EqualTo("desconocido"));
        Assert.That(resultado.Llamadas.Where(l => l.Paso is Pasos.SinPrefijo or Pasos.VueltaAFrio).Select(l => l.EstadoCache), Has.All.EqualTo("frio"));
        var conPrefijo = resultado.Llamadas.Where(l => l.Paso == Pasos.ConPrefijo).ToDictionary(l => l.EntradaId, l => l.EstadoCache);
        Assert.That(conPrefijo["p01"], Is.EqualTo("frio"), "el primer con-prefijo no encuentra nada (README)");
        Assert.That(conPrefijo.Where(kv => kv.Key != "p01").Select(kv => kv.Value), Has.All.EqualTo("cacheado"));
    }

    [Test]
    public async Task PlazoVencido_LasEntradasQuedanExcluidasPorPlazo()
    {
        var (resultado, _) = await CorrerAsync(OrdenDeConsultas.Alternado, 2, new Plazo(DateTimeOffset.UtcNow.AddSeconds(-1)));

        Assert.That(resultado.Llamadas, Is.Empty);
        Assert.That(resultado.Veredictos.Values, Has.All.Matches<VeredictoDelPar>(v => v.Resultado == ResultadoDeEntrada.Excluida && v.Motivo == "plazo"));
    }

    [Test]
    public void Oraculo_TruncadoExcluye_YSinDuracionesExcluye()
    {
        var baseLlamada = new RegistroDeLlamada { Ejemplo = "x", EntradaId = "p01", Paso = Pasos.SinPrefijo, PromptEvalDurationMs = 1000 };
        var con = baseLlamada with { Paso = Pasos.ConPrefijo, PromptEvalDurationMs = 10 };

        Assert.That(OraculoDelPar.Evaluar([baseLlamada, con]).Resultado, Is.EqualTo(ResultadoDeEntrada.Pasa));
        Assert.That(OraculoDelPar.Evaluar([baseLlamada, con with { TruncadoPosible = true }]).Resultado, Is.EqualTo(ResultadoDeEntrada.Excluida));
        Assert.That(OraculoDelPar.Evaluar([baseLlamada with { PromptEvalDurationMs = null }, con]).Motivo, Is.EqualTo("sin duraciones del servidor"));
        Assert.That(OraculoDelPar.Evaluar([baseLlamada, con with { PromptEvalDurationMs = 600 }]).Resultado, Is.EqualTo(ResultadoDeEntrada.Falla));
    }
}
