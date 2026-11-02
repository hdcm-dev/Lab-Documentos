// Nuevo (REC-22; procedencia en un registro de trabajo no publicado; ver el Anexo H, PR-02). Portado de la línea archivada
// del 2026-10-28: las tres pruebas del lector sobre un NDJSON fijo. Las del detector que lanza no se
// traen (REC-21: ganó C, sin el detector). Se suman las de M-I (el registro guarda contadores, nunca el
// cuerpo) y M-F (la guarda estima con el modelo de la llamada); las que corren la compuerta de shell
// están en ArnesTests.Lector.cs. Sin modelo: un HttpMessageHandler falso.
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using OllamaSharp;

namespace MovilidadUrbana.Asistente.Comun.Pruebas.Cableado;

[TestFixture, Category("Cableado")]
public sealed class LecturaDelUsoTests
{
    /// <summary>Las líneas que manda /api/chat en streaming: fragmentos y la línea final con los conteos.</summary>
    internal const string NdjsonFijo =
        "{\"model\":\"modelo-falso\",\"created_at\":\"2026-10-28T12:00:00Z\",\"message\":{\"role\":\"assistant\",\"content\":\"Hay \"},\"done\":false}\n" +
        "{\"model\":\"modelo-falso\",\"created_at\":\"2026-10-28T12:00:00Z\",\"message\":{\"role\":\"assistant\",\"content\":\"1.\"},\"done\":false}\n" +
        "{\"model\":\"modelo-falso\",\"created_at\":\"2026-10-28T12:00:01Z\",\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"done\":true,\"done_reason\":\"stop\"," +
        "\"total_duration\":1000,\"load_duration\":10,\"prompt_eval_count\":710,\"prompt_eval_cached_count\":706,\"prompt_eval_duration\":100,\"eval_count\":2,\"eval_duration\":50}\n";

    [Test]
    public async Task ElHandlerLeePromptEvalCachedCount_QueOllamaSharpNoExpone()
    {
        var (chat, servidor) = ClienteContraServidorFalso(NdjsonFijo);
        var lectura = LectorDePrefijoCacheado.Abrir();

        var r = await chat.GetResponseAsync("¿Cuántas localidades hay cargadas en Corrientes?");

        Assert.Multiple(() =>
        {
            Assert.That(servidor.Rutas, Is.EqualTo(new[] { "/api/chat" }));
            Assert.That(r.Text, Is.EqualTo("Hay 1."));
            Assert.That(r.Usage?.InputTokenCount, Is.EqualTo(710));
            Assert.That(r.Usage?.CachedInputTokenCount, Is.Null, "OllamaSharp 5.5.0 no lo expone");
            Assert.That(lectura.TokensDeEntrada, Is.EqualTo(710));
            Assert.That(lectura.TokensCacheados, Is.EqualTo(706), "el handler sí");
        });
    }

    [Test]
    public async Task ElHandlerTambienLeePorStreaming_SinCambiarLosFragmentos()
    {
        var (chat, _) = ClienteContraServidorFalso(NdjsonFijo);
        var lectura = LectorDePrefijoCacheado.Abrir();

        var texto = new StringBuilder();
        await foreach (var u in chat.GetStreamingResponseAsync("¿Cuántas localidades hay cargadas en Corrientes?")) texto.Append(u.Text);

        Assert.That(texto.ToString(), Is.EqualTo("Hay 1."));
        Assert.That(lectura.TokensCacheados, Is.EqualTo(706));
    }

    [Test]
    public async Task SinElCampoEnElRenglonFinal_LaLecturaQuedaNula()
    {
        var (chat, _) = ClienteContraServidorFalso(NdjsonFijo.Replace("\"prompt_eval_cached_count\":706,", ""));
        var lectura = LectorDePrefijoCacheado.Abrir();

        await chat.GetResponseAsync("Hola");

        Assert.That((lectura.TokensDeEntrada, lectura.TokensCacheados), Is.EqualTo(((int?)710, (int?)null)));
    }

    /// <summary>
    /// REC-22 + M-I: por la capa única, el registro R-LLM lleva el valor observado y el estado que sale
    /// de él; en la carpeta de registros no queda el NDJSON ni el cuerpo del pedido.
    /// </summary>
    [Test]
    public async Task PorLaCapaUnica_ElRegistroLlevaElValorObservado_YNingunCuerpo()
    {
        var dir = Path.Combine(Path.GetTempPath(), "rllm-lector-" + Guid.NewGuid().ToString("n"));
        try
        {
            var config = new ConfiguracionDelModelo { RegistroDir = dir, Endpoint = "http://servidor-falso" };
            var (baseDelModelo, _) = ClienteContraServidorFalso(NdjsonFijo);
            var espia = new ClienteEspia(baseDelModelo);
            var chat = FabricaDeClientes.Envolver(espia, config, Adaptador.OllamaSharp);
            List<ChatMessage> enviados = [new(ChatRole.User, "¿Cuántas localidades hay cargadas en Corrientes?")];

            var respuesta = await chat.GetResponseAsync(enviados);
            string ruta;
            using (var registro = new EscritorDeRegistro(new ContextoDeCorrida
            {
                Ejemplo = "lector", Configuracion = config, Adaptador = Adaptador.OllamaSharp, Modelo = "modelo-falso", Maquina = "guion"
            }))
            {
                registro.Escribir(registro.Llamada("e01", 1, enviados, espia.OpcionesVistas[^1], respuesta, TimeSpan.Zero));
                ruta = registro.Ruta;
            }

            using var linea = JsonDocument.Parse(File.ReadAllLines(ruta).Single());
            Assert.Multiple(() =>
            {
                Assert.That(LecturaDelServidorEnLaRespuesta.CacheadosDe(respuesta), Is.EqualTo(706));
                Assert.That(linea.RootElement.GetProperty("prompt_eval_cached_count").GetInt64(), Is.EqualTo(706));
                Assert.That(linea.RootElement.GetProperty("estado_cache").GetString(), Is.EqualTo("cacheado"));
                Assert.That(linea.RootElement.GetProperty("prompt_eval_count").GetInt64(), Is.EqualTo(710));
                Assert.That(Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Select(Path.GetFileName), Is.EqualTo(new[] { "lector.jsonl" }));
                var todo = File.ReadAllText(ruta);
                Assert.That(todo, Does.Not.Contain("done_reason").And.Not.Contain("created_at").And.Not.Contain("\"messages\""),
                    "ni el NDJSON de la respuesta ni el cuerpo del pedido");
            });
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Test]
    public void EstadoObservado_MasDeLaMitadEsCacheado_NuloDejaDecidirALaDeduccion()
    {
        Assert.Multiple(() =>
        {
            Assert.That(EstadoDeCache.Observado(706, 710), Is.EqualTo("cacheado"));
            Assert.That(EstadoDeCache.Observado(4, 795), Is.EqualTo("frio"), "el encabezado de la plantilla no hace «cacheado»");
            Assert.That(EstadoDeCache.Observado(0, 795), Is.EqualTo("frio"));
            Assert.That(EstadoDeCache.Observado(null, 795), Is.Null);
            Assert.That(EstadoDeCache.Observado(5, null), Is.Null);
        });
    }

    /// <summary>M-F: la guarda recibe y usa el modelo de la llamada (options.ModelId primero; si no, el del cliente).</summary>
    [Test]
    public void GuardaDeTruncado_EstimaConLaCalibracionDelModeloDeLaLlamada()
    {
        List<ChatMessage> mensajes = [new(ChatRole.User, new string('x', 80))];
        var antes = new Dictionary<string, double>(GuardaDeTruncado.CalibracionPorModelo);
        try
        {
            GuardaDeTruncado.CalibracionPorModelo.Clear();
            GuardaDeTruncado.CalibracionPorModelo["modelo-a"] = 2.0;
            GuardaDeTruncado.CalibracionPorModelo["modelo-b"] = 8.0;
            var general = (int)Math.Ceiling(80 / GuardaDeTruncado.CaracteresPorToken);

            Assert.Multiple(() =>
            {
                Assert.That(GuardaDeTruncado.Estimar(mensajes, null, "modelo-a"), Is.EqualTo(40));
                Assert.That(GuardaDeTruncado.Estimar(mensajes, null, "modelo-b"), Is.EqualTo(10));
                Assert.That(GuardaDeTruncado.Estimar(mensajes, new ChatOptions { ModelId = "modelo-b" }, "modelo-a"), Is.EqualTo(10),
                    "el modelo que fija la llamada le gana al del cliente");
                Assert.That(GuardaDeTruncado.Estimar(mensajes, null, "sin-calibrar"), Is.EqualTo(general));
                Assert.That(GuardaDeTruncado.Estimar(mensajes), Is.EqualTo(general));
            });
        }
        finally
        {
            GuardaDeTruncado.CalibracionPorModelo.Clear();
            foreach (var (k, v) in antes) GuardaDeTruncado.CalibracionPorModelo[k] = v;
        }
    }

    /// <summary>M-F en el registro: <c>tokens_estimados</c> sale de la calibración del modelo de la corrida.</summary>
    [Test]
    public async Task Registro_TokensEstimados_UsaElModeloDeLaCorrida()
    {
        var dir = Path.Combine(Path.GetTempPath(), "rllm-guarda-" + Guid.NewGuid().ToString("n"));
        var antes = new Dictionary<string, double>(GuardaDeTruncado.CalibracionPorModelo);
        try
        {
            GuardaDeTruncado.CalibracionPorModelo["modelo-a"] = 2.0;
            var config = new ConfiguracionDelModelo { RegistroDir = dir, Endpoint = "http://servidor.invalid:11434" };
            var chat = FabricaDeClientes.Envolver(new GuionChatClient("Listo."), config, Adaptador.OllamaSharp);
            List<ChatMessage> enviados = [new(ChatRole.User, new string('x', 80))];
            var respuesta = await chat.GetResponseAsync(enviados);

            using var registro = new EscritorDeRegistro(new ContextoDeCorrida
            {
                Ejemplo = "guarda", Configuracion = config, Adaptador = Adaptador.OllamaSharp, Modelo = "modelo-a", Maquina = "guion"
            });
            var llamada = registro.Llamada("e01", 1, enviados, null, respuesta, TimeSpan.Zero);

            Assert.That(llamada.TokensEstimados, Is.EqualTo(40));
            Assert.That(llamada.PromptEvalCachedCount, Is.Null, "el guion no informa reuso");
        }
        finally
        {
            GuardaDeTruncado.CalibracionPorModelo.Clear();
            foreach (var (k, v) in antes) GuardaDeTruncado.CalibracionPorModelo[k] = v;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    internal static (IChatClient Chat, ServidorFalso Servidor) ClienteContraServidorFalso(string ndjson)
    {
        var servidor = new ServidorFalso(ndjson);
        var http = new HttpClient(new LectorDePrefijoCacheado { InnerHandler = servidor }) { BaseAddress = new Uri("http://servidor-falso") };
        return (new OllamaApiClient(http, "modelo-falso"), servidor);
    }

    internal sealed class ServidorFalso(string ndjson) : HttpMessageHandler
    {
        public List<string> Rutas { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Rutas.Add(request.RequestUri!.AbsolutePath);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ndjson, Encoding.UTF8, "application/x-ndjson"),
            });
        }
    }
}
