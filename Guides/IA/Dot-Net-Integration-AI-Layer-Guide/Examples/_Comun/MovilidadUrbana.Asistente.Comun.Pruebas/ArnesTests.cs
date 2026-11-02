// Nuevo: pruebas de cableado de la guarda de truncado, el generador estable, el registro R-LLM y
// el predicado de salto (plan §E.1, §F.8). Las del arnés de shell están en ArnesTests.Shell.cs.
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Comun.Pruebas.Cableado;

public partial class ArnesTests
{
    [TestCase(1000, 1000, 8192, false)]
    [TestCase(700, 1000, 8192, true)]   // menos del 80 % de lo estimado
    [TestCase(8192, 9000, 8192, true)]  // llegó al tope de la ventana
    public void GuardaDeTruncado_OllamaSharp(long informados, int estimacion, int numCtx, bool esperado) =>
        Assert.That(GuardaDeTruncado.Evaluar(informados, estimacion, numCtx, Adaptador.OllamaSharp), Is.EqualTo(esperado));

    [Test]
    public void GuardaDeTruncado_ViaV1_NoVerificable() =>
        Assert.That(GuardaDeTruncado.Evaluar(10, 1000, null, Adaptador.OpenAIv1), Is.Null);

    [Test]
    public void Fnv1a_ValoresConocidos()
    {
        Assert.That(GeneradorDeEmbeddingsEstable.Fnv1a(string.Empty), Is.EqualTo(0x811c9dc5u));
        Assert.That(GeneradorDeEmbeddingsEstable.Fnv1a("a"), Is.EqualTo(0xe40c292cu));
    }

    [Test]
    public async Task Embeddings_EstablesYConLaDimensionDeclarada()
    {
        var gen = new GeneradorDeEmbeddingsEstable(1024);
        var a = await gen.GenerateAsync(["El código postal lleva cuatro dígitos."]);
        var b = await new GeneradorDeEmbeddingsEstable(1024).GenerateAsync(["El código postal lleva cuatro dígitos."]);

        Assert.That(a[0].Vector.Length, Is.EqualTo(1024));
        Assert.That(a[0].Vector.ToArray(), Is.EqualTo(b[0].Vector.ToArray()));
        Assert.That(gen.GetService<EmbeddingGeneratorMetadata>()!.DefaultModelDimensions, Is.EqualTo(1024));
    }

    [Test]
    public async Task Registro_EscribeLlamadaYEntrada_ConElEsquemaYSinDirecciones()
    {
        var dir = Path.Combine(Path.GetTempPath(), "rllm-" + Guid.NewGuid().ToString("n"));
        var config = new ConfiguracionDelModelo { RegistroDir = dir, Endpoint = "http://servidor.invalid:11434" };
        var cliente = FabricaDeClientes.Envolver(new GuionChatClient("Nombre, provincia, código postal y habitantes."), config, Adaptador.OllamaSharp);
        var espia = new ClienteEspia(cliente);

        List<ChatMessage> enviados = [new(ChatRole.User, "¿Qué pide el alta?")];
        var reloj = EscritorDeRegistro.Cronometro();
        var respuesta = await espia.GetResponseAsync(enviados, new ChatOptions { Instructions = "Respondé en castellano." });

        string ruta;
        using (var registro = new EscritorDeRegistro(new ContextoDeCorrida
        {
            Ejemplo = "prueba",
            Configuracion = config,
            Adaptador = Adaptador.OllamaSharp,
            EntradasVersion = "v1",
            EntradasHash = Hashes.Sha256("entradas"),
            PaquetesHash = Hashes.DePaquetes(TestContext.CurrentContext.TestDirectory),
            Maquina = "guion"
        }))
        {
            // Las opciones efectivas son las que salieron por debajo de la capa: se toman del guion.
            var guion = (GuionChatClient)cliente.GetService(typeof(GuionChatClient))!;
            registro.Escribir(registro.Llamada("e01", 1, enviados, guion.Llamadas[0].Opciones, respuesta, reloj.Elapsed));
            registro.Escribir(new RegistroDeEntrada { Ejemplo = "prueba", EntradaId = "e01", Oraculo = "no-vacia", Resultado = ResultadoDeEntrada.Pasa });
            ruta = registro.Ruta;
        }

        var lineas = File.ReadAllLines(ruta);
        Assert.That(lineas, Has.Length.EqualTo(2));
        using var llamada = JsonDocument.Parse(lineas[0]);
        string[] campos =
        [
            "tipo", "fecha", "ejemplo", "paso", "entrada_id", "llamada_n", "entrada", "salida", "modelo", "digest", "adaptador",
            "servidor", "ollama_version", "paquetes_hash", "hash_instrucciones", "hash_herramientas", "entradas_version",
            "entradas_hash", "temperature", "seed", "num_ctx", "num_ctx_origen", "razonamiento", "herramientas", "recuperados",
            "contexto", "prompt_eval_count", "eval_count", "load_duration_ms", "prompt_eval_duration_ms", "eval_duration_ms",
            "total_duration_ms", "duracion_reloj_ms", "turno_id", "estado_cache", "truncado_posible", "maquina", "carga_1min"
        ];
        Assert.That(campos.Where(c => !llamada.RootElement.TryGetProperty(c, out _)), Is.Empty);
        Assert.That(llamada.RootElement.GetProperty("razonamiento").GetString(), Is.EqualTo("None"));
        Assert.That(llamada.RootElement.GetProperty("num_ctx").GetInt32(), Is.EqualTo(8192));
        Assert.That(llamada.RootElement.GetProperty("num_ctx_origen").GetString(), Is.EqualTo("default"));
        Assert.That(llamada.RootElement.GetProperty("paquetes_hash").GetString(), Is.Not.Null.And.Length.EqualTo(64));
        Assert.That(File.ReadAllText(ruta), Does.Not.Contain("servidor.invalid").And.Not.Contain("http"));
        using var entrada = JsonDocument.Parse(lineas[1]);
        Assert.That(entrada.RootElement.GetProperty("entradas_version").GetString(), Is.EqualTo("v1"));

        Directory.Delete(dir, true);
    }

    [Test]
    public async Task PredicadoDeSalto_ServidorCerrado_Salta()
    {
        // Puerto 9 (discard) en loopback: no hay nada escuchando en el contenedor de build.
        var resultado = await PredicadoDeSalto.EvaluarAsync("http://127.0.0.1:9", "qwen3.5:4b", PredicadoDeSalto.Chat);

        Assert.That(resultado.Saltar, Is.True);
        Assert.That(resultado.Motivo, Does.Contain("/api/version").And.Not.Contain("127.0.0.1"));
    }
}
