// Nuevo (REC-10 y REC-11, recuperación del 2026-10-28): qué llega al servidor por cada vía con las
// mismas opciones. Un servidor falso en memoria (un HttpMessageHandler: sin red y sin modelo) guarda el
// cuerpo del pedido tal como sale de cada adaptador. Se comprueban TopK, MaxOutputTokens, tool_choice,
// el formato del mensaje «tool» y que la credencial del SDK viaje como encabezado HTTP y no en el cuerpo.
// Los cuerpos capturados se escriben en RLLM_DIR/capturas/02-Cambio-De-Adaptador/ y pasan por la compuerta
// «cabeceras = 0» (tools/compuerta-cabeceras.sh).
//
// Este archivo arma las dos vías con un transporte falso, así que nombra los adaptadores. El criterio de
// OA-D1 (`grep -lE "OllamaSharp|using OpenAI" *.cs` en la carpeta del ejemplo) mira el código de la
// aplicación, no Pruebas/: lo sigue cumpliendo (SoloProgramCs_NombraLosAdaptadores).
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun.AI;
using OllamaSharp;
using OpenAI;

namespace MovilidadUrbana.Asistente.Pruebas;

public class CuerpoDelPedidoTests
{
    private const string Pregunta = "¿Cuántos dígitos lleva el código postal?";
    private const string Herramienta = "contar_localidades";
    private const string ResultadoDeLaHerramienta = """{"provincia":"Chaco","cantidad":1}""";
    private static readonly ConfiguracionDelModelo Config = new() { PropioEndpoint = "http://servidor-falso.invalid" };

    /// <summary>Servidor falso: guarda cada pedido (ruta, encabezado de autorización y cuerpo) y responde lo mínimo.</summary>
    private sealed class ServidorFalso : HttpMessageHandler
    {
        public sealed record Pedido(string Ruta, string? Autorizacion, string Cuerpo)
        {
            public JsonObject Json => JsonNode.Parse(Cuerpo)!.AsObject();
        }

        public List<Pedido> Pedidos { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct)
        {
            var cuerpo = pedido.Content is null ? "" : await pedido.Content.ReadAsStringAsync(ct);
            var ruta = pedido.RequestUri!.AbsolutePath;
            Pedidos.Add(new(ruta, pedido.Headers.Authorization?.ToString(), cuerpo));

            string json, tipo;
            if (ruta.EndsWith("/api/chat"))
            {
                tipo = "application/x-ndjson";
                json = """{"model":"modelo-de-prueba","created_at":"2026-10-28T00:00:00Z","message":{"role":"assistant","content":"Cuatro dígitos."},"done":true,"done_reason":"stop","total_duration":1000,"prompt_eval_count":120,"eval_count":4,"eval_duration":100}""" + "\n";
            }
            else if (ruta.EndsWith("/v1/chat/completions"))
            {
                tipo = "application/json";
                json = """{"id":"c1","object":"chat.completion","created":1,"model":"modelo-de-prueba","choices":[{"index":0,"message":{"role":"assistant","content":"Cuatro dígitos."},"finish_reason":"stop"}],"usage":{"prompt_tokens":120,"completion_tokens":4,"total_tokens":124}}""";
            }
            else return new HttpResponseMessage(HttpStatusCode.NotFound);

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, tipo) };
        }
    }

    /// <summary>
    /// La vía, como la arma <c>RaizDeComposicion.Crear</c>, pero con el transporte falso: la nativa con el
    /// mismo manejador lector que pone <c>FabricaDeClientes.CrearHttp</c>; la de /v1 con las mismas opciones
    /// del SDK (10 min, cero reintentos, la clave fija). Las dos con la capa única (<c>Envolver</c>).
    /// </summary>
    private static IChatClient Via(Adaptador adaptador, ServidorFalso servidor)
    {
        if (adaptador == Adaptador.OllamaSharp)
        {
            var http = new HttpClient(new LectorDePrefijoCacheado { InnerHandler = servidor })
            {
                BaseAddress = new Uri(Config.PropioEndpoint),
                Timeout = FabricaDeClientes.TimeoutDelModelo
            };
            return FabricaDeClientes.Envolver(new OllamaApiClient(http, Config.PropioModelo), Config, Adaptador.OllamaSharp);
        }
        var opciones = new OpenAIClientOptions
        {
            Endpoint = new Uri(new Uri(Config.PropioEndpoint), "/v1"),
            NetworkTimeout = FabricaDeClientes.TimeoutDelModelo,
            RetryPolicy = new ClientRetryPolicy(maxRetries: 0),
            Transport = new HttpClientPipelineTransport(new HttpClient(servidor, disposeHandler: false))
        };
        var v1 = new OpenAIClient(new ApiKeyCredential("ollama"), opciones).GetChatClient(Config.PropioModelo).AsIChatClient();
        return FabricaDeClientes.Envolver(v1, Config, Adaptador.OpenAIv1);
    }

    /// <summary>Las opciones de la aplicación más TopK y MaxOutputTokens, que la capa no toca.</summary>
    private static ChatOptions OpcionesConMuestreo()
    {
        var opciones = AsistenteDeAlta.Opciones();
        opciones.TopK = 20;
        opciones.MaxOutputTokens = 3;
        return opciones;
    }

    /// <summary>Una vuelta con herramienta ya resuelta: la llamada del asistente y el resultado en un mensaje «tool».</summary>
    private static (List<ChatMessage> Mensajes, ChatOptions Opciones) VueltaConHerramienta()
    {
        var funcion = AIFunctionFactory.Create((string provincia) => 1, Herramienta, "Cuenta las localidades cargadas de una provincia.");
        var opciones = AsistenteDeAlta.Opciones();
        opciones.Tools = [funcion];
        opciones.ToolMode = ChatToolMode.Auto;
        List<ChatMessage> mensajes =
        [
            new(ChatRole.User, "¿Cuántas localidades hay cargadas en Chaco?"),
            new(ChatRole.Assistant, [new FunctionCallContent("call_1", Herramienta, new Dictionary<string, object?> { ["provincia"] = "Chaco" })]),
            new(ChatRole.Tool, [new FunctionResultContent("call_1", JsonNode.Parse(ResultadoDeLaHerramienta))])
        ];
        return (mensajes, opciones);
    }

    private static async Task<ServidorFalso.Pedido> Enviar(Adaptador adaptador, string caso, IList<ChatMessage> mensajes, ChatOptions opciones)
    {
        var servidor = new ServidorFalso();
        using var cliente = Via(adaptador, servidor);
        await cliente.GetResponseAsync(mensajes, opciones);
        var pedido = servidor.Pedidos.Single();
        Capturar($"cuerpo-{RaizDeComposicion.Nombre(adaptador)}-{caso}.json", pedido.Cuerpo);
        return pedido;
    }

    private static Task<ServidorFalso.Pedido> EnviarConMuestreo(Adaptador adaptador) =>
        Enviar(adaptador, "muestreo", AsistenteDeAlta.Mensajes(Pregunta), OpcionesConMuestreo());

    private static Task<ServidorFalso.Pedido> EnviarConHerramienta(Adaptador adaptador)
    {
        var (mensajes, opciones) = VueltaConHerramienta();
        return Enviar(adaptador, "herramienta", mensajes, opciones);
    }

    private static JsonObject MensajeTool(ServidorFalso.Pedido pedido) =>
        pedido.Json["messages"]!.AsArray().Select(m => m!.AsObject()).Single(m => (string?)m["role"] == "tool");

    [Test]
    public async Task ViaNativa_LlevaNumCtxTopKYNumPredict()
    {
        var pedido = await EnviarConMuestreo(Adaptador.OllamaSharp);
        var cuerpo = pedido.Json;
        var options = cuerpo["options"]!.AsObject();
        TestContext.Out.WriteLine($"POST {pedido.Ruta}  options={options.ToJsonString()}  think={cuerpo["think"]?.ToJsonString() ?? "(ausente)"}");

        Assert.Multiple(() =>
        {
            Assert.That(pedido.Ruta, Is.EqualTo("/api/chat"));
            Assert.That((int)options["num_ctx"]!, Is.EqualTo(ConfiguracionDelModelo.NumCtxPorDefecto));
            Assert.That((int)options["top_k"]!, Is.EqualTo(20), "TopK viaja por la vía nativa");
            Assert.That((int)options["num_predict"]!, Is.EqualTo(3), "MaxOutputTokens sale como num_predict");
            Assert.That((bool)cuerpo["think"]!, Is.False);
        });
    }

    [Test]
    public async Task ViaV1_DescartaTopKYNumCtx_YTraduceMaxOutputTokens()
    {
        var pedido = await EnviarConMuestreo(Adaptador.OpenAIv1);
        var sinMensajes = pedido.Json;
        sinMensajes.Remove("messages");
        TestContext.Out.WriteLine($"POST {pedido.Ruta}  cuerpo sin mensajes={sinMensajes.ToJsonString()}");

        Assert.Multiple(() =>
        {
            Assert.That(pedido.Ruta, Is.EqualTo("/v1/chat/completions"));
            Assert.That(pedido.Cuerpo, Does.Not.Contain("top_k"), "TopK se descarta en silencio por /v1");
            Assert.That(pedido.Cuerpo, Does.Not.Contain("num_ctx"), "num_ctx no se fija por pedido por /v1");
            Assert.That((int)sinMensajes["max_completion_tokens"]!, Is.EqualTo(3), "MaxOutputTokens sale como max_completion_tokens");
            Assert.That(sinMensajes.ContainsKey("max_tokens"), Is.False);
            Assert.That((string?)sinMensajes["reasoning_effort"], Is.EqualTo("none"));
        });
    }

    /// <summary>
    /// Lo que el cliente ENVÍA. Por /v1 el modo de herramienta viaja como <c>tool_choice</c>; si el servidor
    /// lo cumple no lo mide esta prueba (no hay servidor). Por la vía nativa no hay campo equivalente.
    /// </summary>
    [Test]
    public async Task ToolChoice_LoEnviaElClientePorV1_YNoExisteEnLaNativa()
    {
        var nativa = await EnviarConHerramienta(Adaptador.OllamaSharp);
        var v1 = await EnviarConHerramienta(Adaptador.OpenAIv1);

        Assert.Multiple(() =>
        {
            Assert.That(nativa.Json["tools"]!.AsArray(), Has.Count.EqualTo(1));
            Assert.That(v1.Json["tools"]!.AsArray(), Has.Count.EqualTo(1));
            Assert.That((string?)v1.Json["tool_choice"], Is.EqualTo("auto"), "ChatToolMode.Auto viaja como tool_choice por /v1");
            Assert.That(nativa.Json.ContainsKey("tool_choice"), Is.False, "la API nativa no tiene tool_choice");
        });
    }

    /// <summary>
    /// El mismo resultado de herramienta llega al modelo con otra forma según la vía. Lo que se ve de cada
    /// una queda en la salida de la prueba (y en los cuerpos capturados).
    /// </summary>
    [Test]
    public async Task ElMensajeTool_ViajaEnElFormatoDeCadaVia()
    {
        var nativa = MensajeTool(await EnviarConHerramienta(Adaptador.OllamaSharp));
        var v1 = MensajeTool(await EnviarConHerramienta(Adaptador.OpenAIv1));
        TestContext.Out.WriteLine($"/api/chat tool={nativa.ToJsonString()}");
        TestContext.Out.WriteLine($"/v1 tool={v1.ToJsonString()}");
        var esperado = JsonNode.Parse(ResultadoDeLaHerramienta)!.ToJsonString();

        // /v1: el contenido es sólo el resultado y el id de la llamada va aparte, en tool_call_id.
        Assert.That(JsonNode.Parse((string)v1["content"]!)!.ToJsonString(), Is.EqualTo(esperado));
        Assert.That((string?)v1["tool_call_id"], Is.EqualTo("call_1"));
        // Nativa: el contenido no es sólo el resultado; lleva el contenido de la función entero (id y resultado).
        var contenidoNativa = JsonNode.Parse((string)nativa["content"]!)!.AsObject();
        Assert.That(contenidoNativa.Select(p => p.Key), Is.EquivalentTo(new[] { "CallId", "Result" }));
        Assert.That(contenidoNativa["Result"]!.ToJsonString(), Is.EqualTo(esperado));
        Assert.That(nativa.ContainsKey("tool_call_id"), Is.False);
    }

    /// <summary>REC-11: la clave que exige el SDK viaja como encabezado HTTP del HttpClient, nunca en el cuerpo.</summary>
    [TestCase(Adaptador.OllamaSharp)]
    [TestCase(Adaptador.OpenAIv1)]
    public async Task LaCredencial_NoViajaEnElCuerpo(Adaptador adaptador)
    {
        var pedido = await EnviarConMuestreo(adaptador);
        TestContext.Out.WriteLine($"{RaizDeComposicion.Nombre(adaptador)}: Authorization={(pedido.Autorizacion is null ? "(ausente)" : "Bearer <clave>")}");

        if (adaptador == Adaptador.OpenAIv1) Assert.That(pedido.Autorizacion, Is.EqualTo("Bearer ollama"));
        else Assert.That(pedido.Autorizacion, Is.Null, "Ollama no autentica: la vía nativa no manda clave");
        // Que no haya cabeceras serializadas con contenido lo decide la compuerta (la prueba de abajo).
        Assert.That(pedido.Cuerpo, Does.Not.Contain("Bearer").And.Not.Contain("Authorization"));
    }

    /// <summary>
    /// Compuerta «cabeceras = 0» sobre los cuerpos capturados de las dos vías. Corre el script del repo; si la
    /// carpeta tools/ no está a mano (p. ej. fuera del arnés), la prueba se salta con su motivo.
    /// </summary>
    [Test]
    public async Task LosCuerposCapturados_PasanLaCompuertaDeCabeceras()
    {
        foreach (var adaptador in new[] { Adaptador.OllamaSharp, Adaptador.OpenAIv1 })
        {
            await EnviarConMuestreo(adaptador);
            await EnviarConHerramienta(adaptador);
        }
        var compuerta = Compuerta();
        if (compuerta is null) Assert.Ignore("no se encontró tools/compuerta-cabeceras.sh (ARNES_GUIA vacía y fuera del árbol de la guía)");

        var (codigo, salida) = Correr(compuerta, CarpetaDeCapturas());
        TestContext.Out.WriteLine(salida);
        Assert.That(codigo, Is.EqualTo(0), salida);
        Assert.That(salida, Does.Contain("cabeceras: 0 en 4 cuerpo(s)"));
    }

    // ------------------------------------------------------------------ capturas y compuerta

    /// <summary>RLLM_DIR/capturas/02-Cambio-De-Adaptador/ (sin RLLM_DIR, una carpeta temporal).</summary>
    private static string CarpetaDeCapturas()
    {
        var raiz = Environment.GetEnvironmentVariable("RLLM_DIR") is { Length: > 0 } r ? r : Path.Combine(Path.GetTempPath(), "guia-ia-registros");
        var carpeta = Path.Combine(raiz, "capturas", "02-Cambio-De-Adaptador");
        Directory.CreateDirectory(carpeta);
        return carpeta;
    }

    private static void Capturar(string nombre, string cuerpo) =>
        File.WriteAllText(Path.Combine(CarpetaDeCapturas(), nombre), cuerpo + "\n");

    private static string? Compuerta()
    {
        var inicios = new List<string>();
        if (Environment.GetEnvironmentVariable("ARNES_GUIA") is { Length: > 0 } g) inicios.Add(g);
        inicios.Add(Directory.GetCurrentDirectory());
        inicios.Add(TestContext.CurrentContext.TestDirectory);
        foreach (var inicio in inicios)
            for (var d = new DirectoryInfo(inicio); d is not null; d = d.Parent)
            {
                var f = Path.Combine(d.FullName, "tools", "compuerta-cabeceras.sh");
                if (File.Exists(f)) return f;
            }
        return null;
    }

    private static (int Codigo, string Salida) Correr(string script, string carpeta)
    {
        var psi = new ProcessStartInfo("bash") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        psi.ArgumentList.Add(script);
        psi.ArgumentList.Add(carpeta);
        using var p = Process.Start(psi)!;
        var salida = p.StandardOutput.ReadToEndAsync();
        var error = p.StandardError.ReadToEndAsync();
        if (!p.WaitForExit(TimeSpan.FromSeconds(60)))
        {
            p.Kill(true);
            Assert.Fail("la compuerta no terminó en 60 s");
        }
        return (p.ExitCode, salida.Result + error.Result);
    }
}
