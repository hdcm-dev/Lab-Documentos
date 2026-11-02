// Nuevo. Humo con modelo, un cruce de capas por prueba (plan §F.1): (a) PD-3 controller → caso de uso →
// puerto → adaptador; (b) PD-2 controller → puerto directo + RAG; (c) PD-6 circuito → IChatClient →
// herramientas = casos de uso, con estado entre dos peticiones. Una entrada cada uno (§E.5). Cada
// prueba escribe R-LLM; los invariantes de código se afirman siempre; la conducta del modelo va al
// oráculo del registro (pasa | falla), que tools/recalcular-oraculos.sh recalcula. Cada oráculo es el
// del ejemplo hermano (07, 03, 11; plan §E.5), adaptado al registro de la demo: OraculosDeHumo.cs.
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Contracts;
using MovilidadUrbana.Asistente.Infrastructure.AI.Ayuda;
using MovilidadUrbana.Asistente.Infrastructure.AI.Chat;
using MovilidadUrbana.Asistente.Infrastructure.AI.Extraccion;
using MovilidadUrbana.Asistente.Infrastructure.Sessions;
using MovilidadUrbana.Asistente.Sessions;

namespace MovilidadUrbana.Asistente.Tests.ConModelo;

[TestFixture]
[Category("LLM")]
[NonParallelizable]
public class Humo
{
    public const string Ejemplo = "Demo-MovilidadUrbana.Asistente";

    private ConfiguracionDelModelo _config = default!;
    private ResultadoDelPredicado _predicado = default!;
    private FabricaConModelo _fabrica = default!;
    private EscritorDeRegistro _registro = default!;

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno().Validar();
        _predicado = await PredicadoDeSalto.EvaluarAsync(_config.Endpoint, _config.Modelo, PredicadoDeSalto.Chat);
        if (_predicado.Saltar) Assert.Ignore(_predicado.Motivo);

        var persistidos = Environment.GetEnvironmentVariable("EMB_PERSISTIDOS");
        var ajustes = new Dictionary<string, string?>
        {
            ["OLLAMA_ENDPOINT"] = _config.Endpoint, ["OLLAMA_MODEL"] = _config.Modelo, ["OLLAMA_SERVIDOR"] = _config.ServidorTexto,
            ["OLLAMA_NUM_CTX"] = _config.NumCtx.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["OLLAMA_NUM_CTX_ORIGEN"] = _config.NumCtxOrigen,
            // Con los vectores de la fase 3a, PD-2 corre con embeddings reales sin pedir el servidor de embeddings aparte; sin ellos, EMB-C.
            ["EMB_PLAN"] = File.Exists(persistidos) ? "EMB-B" : "EMB-C",
            ["EMB_PERSISTIDOS"] = File.Exists(persistidos) ? persistidos : null
        };
        _fabrica = new FabricaConModelo(_config, ajustes);
        _ = _fabrica.Server;
        _registro = new EscritorDeRegistro(new ContextoDeCorrida
        {
            Ejemplo = Ejemplo, Configuracion = _config, Adaptador = Adaptador.OllamaSharp, Modelo = _config.Modelo, Digest = _predicado.Digest,
            OllamaVersion = _predicado.VersionDeOllama, EntradasVersion = "v1", EntradasHash = Entradas.Hash, PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory)
        });
    }

    [OneTimeTearDown]
    public void Cerrar()
    {
        _registro?.Dispose();
        _fabrica?.Dispose();
    }

    private async Task<string?> EstadoCacheAsync() =>
        _config.Servidor == TipoDeServidor.Compartido && await PredicadoDeSalto.ModeloCargadoAsync(_config.Endpoint, _config.Modelo) == false ? "frio" : null;

    private IReadOnlyList<RegistroDeLlamada> Registrar(string entradaId, string paso, string? frio,
        IReadOnlyList<Recuperado>? recuperados = null, string? contexto = null)
    {
        var lineas = _fabrica.Grabador.Llamadas.Select((l, i) => _registro.Llamada(entradaId, i + 1, l.Mensajes, l.Efectivas, l.Respuesta, l.Reloj, paso,
            recuperados: recuperados, contexto: contexto, estadoCacheForzado: i == 0 ? frio : null)).ToList();
        foreach (var l in lineas) _registro.Escribir(l);
        _fabrica.Grabador.Llamadas.Clear();
        return lineas;
    }

    private void Veredicto(string entradaId, string oraculo, IReadOnlyList<RegistroDeLlamada> llamadas, bool pasa, string? motivo = null)
    {
        var excluida = llamadas.Count == 0 || llamadas.Any(l => l.TruncadoPosible == true);
        _registro.Escribir(new RegistroDeEntrada
        {
            Ejemplo = Ejemplo, EntradaId = entradaId, Oraculo = oraculo,
            Resultado = excluida ? ResultadoDeEntrada.Excluida : pasa ? ResultadoDeEntrada.Pasa : ResultadoDeEntrada.Falla,
            Motivo = excluida ? (llamadas.Count == 0 ? "sin llamadas" : "truncado_posible") : motivo
        });
        TestContext.Out.WriteLine($"{entradaId} ({oraculo}): {(excluida ? "excluida" : pasa ? "pasa" : "falla")} {motivo}");
    }

    private HttpClient Api()
    {
        var c = _fabrica.CreateClient();
        c.Timeout = FabricaDeClientes.TimeoutDelModelo;
        c.DefaultRequestHeaders.Add(SessionHeaderMiddleware.HeaderName, Guid.NewGuid().ToString("n"));
        return c;
    }

    [Test, Order(1)]
    [Description("PD-2 (cruce b): la ayuda responde con cita validada contra lo recuperado")]
    public async Task Pd2_AyudaConCita()
    {
        var texto = Entradas.Texto("pd2-01");
        var frio = await EstadoCacheAsync();
        var r = await (await Api().PostAsJsonAsync("/api/v1/ayuda", new PreguntaRequest(texto))).Content.ReadFromJsonAsync<RespuestaDeAyudaDto>();
        var recuperados = r!.Recuperados.Select(f => new Recuperado(f.Id, f.Puntaje)).ToList();
        var contexto = AyudaConRecuperacion.Contexto(r.Recuperados.Select(f => new Application.Ayuda.FragmentoRecuperado(f.Id, f.Texto, f.Puntaje)).ToList());
        var llamadas = Registrar("pd2-01", "PD-2", frio, recuperados, contexto);

        // Invariantes de código: nunca una respuesta sin fuente; si la cita es válida, la fuente está entre los recuperados.
        Assert.That(r.Fuente, Is.Not.Null);
        if (r.CitaValida) Assert.That(r.Recuperados.Select(f => f.Id), Does.Contain(r.Fuente));
        // Conducta (regla de 07): cita el relevante y afirma lo que fija el literal citado, sin otro valor.
        var literal = r.Recuperados.FirstOrDefault(f => f.Id == r.Fuente)?.Texto;
        var pasa = OraculosDeHumo.Pd2(r.CitaValida, r.Fuente, r.Texto, literal);
        Veredicto("pd2-01", "demo-pd2-cita", llamadas, pasa, $"fuente {r.Fuente}; plan {r.PlanDeEmbeddings}");
    }

    [Test, Order(2)]
    [Description("PD-3 (cruce a): «no sé cuántos viven» → Habitantes null, la regla rechaza y se pide el dato")]
    public async Task Pd3_SinHabitantes()
    {
        var texto = Entradas.Texto("pd3-01");
        var frio = await EstadoCacheAsync();
        var p = await (await Api().PostAsJsonAsync("/api/v1/localidades/desde-texto", new TextoRequest(texto))).Content.ReadFromJsonAsync<PropuestaDeAltaDto>();
        var llamadas = Registrar("pd3-01", "PD-3", frio);

        // Invariante de código (garantía numérica): si el texto no trae el número, Habitantes llega null y falta.
        var crudo = llamadas.Count > 0 ? Habitantes(llamadas[^1].Salida) : null;
        if (crudo is null || GarantiaNumerica.Entero(crudo, texto) is null)
        {
            if (p!.Interpretado)
            {
                Assert.That(p.Propuesta!.Habitantes, Is.Null);
                Assert.That(p.Faltantes, Does.ContainKey("habitantes"));
            }
        }
        // Conducta (regla de 03): la decisión sobre la salida cruda, con la garantía del texto de 03.
        var decision = OraculosDeHumo.DecisionDe03(llamadas.Count > 0 ? llamadas[^1].Salida : null, texto);
        var pasa = decision == OraculosDeHumo.DecisionEsperadaDePd3;
        Veredicto("pd3-01", "demo-pd3-habitantes", llamadas, pasa, $"decision 03 {decision}; habitantes propuesto por el modelo: {(crudo?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null")}");
        // Invariante de código, después del registro: cuando 03 decide «faltan:habitantes», la demo también pide el dato.
        if (pasa) Assert.That(p!.Interpretado && p.Propuesta!.Habitantes is null && p.Faltantes.ContainsKey("habitantes"), Is.True);
    }

    private static int? Habitantes(string? salida)
    {
        try
        {
            using var doc = JsonDocument.Parse(salida ?? "null");
            return doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("habitantes", out var h) && h.ValueKind == JsonValueKind.Number
                ? h.GetInt32() : null;
        }
        catch (JsonException) { return null; }
    }

    [Test, Order(3)]
    [Description("PD-6 (cruce c): «Borrá Resistencia» → pedido armado por el código → sin aprobación no se borra; en dos peticiones")]
    public async Task Pd6_BajaConAprobacion()
    {
        var texto = Entradas.Texto("pd6-01");
        var frio = await EstadoCacheAsync();
        using var scope = _fabrica.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<SessionContext>().Set(Guid.NewGuid().ToString("n"));
        var chat = scope.ServiceProvider.GetRequiredService<SesionDeChat>();
        var localidades = scope.ServiceProvider.GetRequiredService<LocalidadService>();
        var antes = (await localidades.GetAllAsync()).Count;

        await chat.EnviarAsync(texto);                                   // primera petición
        var pedidos = chat.Pendientes.ToList();
        var entreLasDos = (await localidades.GetAllAsync()).Count;
        var prosa = chat.Mensajes.LastOrDefault(m => m.Rol == "asistente")?.Texto;
        foreach (var pedido in pedidos)
        {
            await chat.DecidirAsync(pedido.Id, aprobar: false);          // segunda petición: la persona rechaza
        }
        var despues = (await localidades.GetAllAsync()).Count;
        // El registro se escribe antes de afirmar: una aserción que falla no se lleva la evidencia.
        var llamadas = Registrar("pd6-01", "PD-6", frio);

        var pedidoBaja = pedidos.FirstOrDefault(p => p.Herramienta == HerramientasDelAsistente.Eliminar);
        var herramientas = llamadas.SelectMany(l => l.Herramientas).Where(h => h.Nombre == HerramientasDelAsistente.Eliminar).ToList();
        var pidio = herramientas.Any(h => Regex.IsMatch(Argumentos.Texto(h.Argumentos, "nombre") ?? string.Empty, "resistencia", RegexOptions.IgnoreCase));
        // Conducta (regla de 11, rama sin aprobación): pidió la baja de Resistencia y el pedido que armó el
        // código muestra Resistencia (Chaco). Que no se borre nada es invariante de código (abajo, AC-4).
        var pasa = OraculosDeHumo.Pd6(pidio, pedidoBaja?.Descripcion);
        Veredicto("pd6-01", "demo-pd6-pedido", llamadas, pasa, pedidoBaja?.Descripcion ?? "el modelo no pidió eliminar_localidad");

        Assert.That(entreLasDos, Is.EqualTo(antes), "entre las dos peticiones la base no cambia");
        Assert.That(despues, Is.EqualTo(antes), "sin aprobación no se borra (AC-4)");
        foreach (var pedido in pedidos)
        {
            // El pedido lo arma el código: no repite la prosa del modelo (comparación ordinal; sin prosa no hay nada que buscar).
            if (!string.IsNullOrWhiteSpace(prosa)) Assert.That(pedido.Descripcion.Contains(prosa, StringComparison.Ordinal), Is.False);
        }
        if (pedidoBaja is not null) Assert.That(pedidoBaja.Descripcion, Does.Match(@"^Eliminar la localidad «Resistencia» \(Chaco\), id \d+, de tu sesión\."));
    }
}
