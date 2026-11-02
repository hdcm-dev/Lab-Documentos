// Nuevo: pruebas del arnés de shell (tools/verificar.sh y la demo), sin servidor de modelos.
// M-A: en todo modo con modelo, OLLAMA_SERVIDOR, RLLM_MAQUINA, RLLM_MAQUINA_PROPIO y RLLM_DIR no tienen
// defecto; si falta una, el arnés sale con 2, lo dice y no llama a nadie (ni siquiera a docker).
// M-K: la fase del servidor compartido compara versión y digests con los esperados antes de medir.
// Un «docker» falso en el PATH registra cada llamada; para M-K ejecuta el curl del pedido contra un
// servidor HTTP local que responde como /api/version y /api/tags.
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MovilidadUrbana.Asistente.Comun.Pruebas.Cableado;

public partial class ArnesTests
{
    private static readonly string[] VariablesSinDefecto = ["OLLAMA_SERVIDOR", "RLLM_MAQUINA", "RLLM_MAQUINA_PROPIO"];

    private static readonly string[][] ModosConModelo =
    [
        ["fase", "lan"], ["fase", "3a"], ["fase", "3b"], ["extension"], ["estabilidad"], ["embeddings"],
        ["--con-modelo"], ["correr", "01-Chat-Y-Streaming", "--con-modelo"],
    ];

    public static IEnumerable<TestCaseData> CasosSinVariable() =>
        from modo in ModosConModelo
        from variable in VariablesSinDefecto
        select new TestCaseData(modo, variable).SetName($"Verificar_{string.Join('_', modo).TrimStart('-')}_sin_{variable}");

    [TestCaseSource(nameof(CasosSinVariable))]
    public void Verificar_ModoConModelo_SinVariable_AbortaSinLlamar(string[] modo, string variable)
    {
        using var entorno = new EntornoDeArnes();
        var r = entorno.Correr(Path.Combine(Guia(), "tools", "verificar.sh"), modo, sin: variable);

        Assert.That(r.Codigo, Is.EqualTo(2), r.Salida);
        Assert.That(r.Salida, Does.Contain(variable).And.Contain("no tienen defecto"));
        Assert.That(entorno.LlamadasADocker(), Is.Empty, "no tiene que llamar a docker ni a ningún servidor");
    }

    [Test]
    public void Verificar_FaseCompartida_SinRllmDir_AbortaSinLlamar()
    {
        using var entorno = new EntornoDeArnes();
        var r = entorno.Correr(Path.Combine(Guia(), "tools", "verificar.sh"), ["fase", "lan"], sin: "RLLM_DIR");

        Assert.That(r.Codigo, Is.EqualTo(2), r.Salida);
        Assert.That(r.Salida, Does.Contain("RLLM_DIR"));
        Assert.That(entorno.LlamadasADocker(), Is.Empty);
    }

    [TestCase("OLLAMA_VERSION_ESPERADA")]
    [TestCase("DIGEST_4B_ESPERADO")]
    [TestCase("DIGEST_EMB_ESPERADO")]
    public void Verificar_FaseCompartida_SinValorEsperado_AbortaSinLlamar(string variable)
    {
        using var entorno = new EntornoDeArnes();
        var r = entorno.Correr(Path.Combine(Guia(), "tools", "verificar.sh"), ["fase", "lan"], sin: variable);

        Assert.That(r.Codigo, Is.EqualTo(2), r.Salida);
        Assert.That(r.Salida, Does.Contain(variable));
        Assert.That(entorno.LlamadasADocker(), Is.Empty);
    }

    [Test]
    public void Verificar_RllmDirDentroDeLaGuia_Aborta()
    {
        using var entorno = new EntornoDeArnes();
        var r = entorno.Correr(Path.Combine(Guia(), "tools", "verificar.sh"), ["fase", "3a"],
            extra: new() { ["RLLM_DIR"] = Path.Combine(Guia(), "Examples", "registros") });

        Assert.That(r.Codigo, Is.EqualTo(2), r.Salida);
        Assert.That(r.Salida, Does.Contain("fuera del árbol"));
        Assert.That(entorno.LlamadasADocker(), Is.Empty);
    }

    public static IEnumerable<TestCaseData> CasosDemoSinVariable() =>
        from accion in new[] { "embeddings", "humo" }
        from variable in VariablesSinDefecto.Append("RLLM_DIR")
        select new TestCaseData(accion, variable).SetName($"Demo_{accion}_sin_{variable}");

    [TestCaseSource(nameof(CasosDemoSinVariable))]
    public void Demo_ModoConModelo_SinVariable_AbortaSinLlamar(string accion, string variable)
    {
        using var entorno = new EntornoDeArnes();
        var demo = Path.Combine(Guia(), "Demos", "MovilidadUrbana.Asistente", "tools", "demo.sh");
        var r = entorno.Correr(demo, [accion], sin: variable);

        Assert.That(r.Codigo, Is.EqualTo(2), r.Salida);
        Assert.That(r.Salida, Does.Contain(variable).And.Contain("no tienen defecto"));
        Assert.That(entorno.LlamadasADocker(), Is.Empty);
    }

    [TestCase("bbbbbbbbbbbb", false, TestName = "Verificar_Precondicion_OtroDigest_NoMide")]
    [TestCase("aaaaaaaaaaaa", true, TestName = "Verificar_Precondicion_DigestEsperado_Sigue")]
    public void Verificar_PrecondicionDelCompartido(string digestDelServidor, bool coincide)
    {
        using var servidor = new ServidorGuionado(
            ("/api/version", """{"version":"0.0.1-prueba"}"""),
            ("/api/tags", $$$"""{"models":[{"name":"qwen3.5:4b","model":"qwen3.5:4b","size":1,"digest":"{{{digestDelServidor}}}0000","details":{"family":"x"}},{"name":"qwen3-embedding:0.6b","model":"qwen3-embedding:0.6b","size":1,"digest":"cccccccccccc0000","details":{}}]}"""));
        using var entorno = new EntornoDeArnes(dockerEjecutaCurl: true);

        var r = entorno.Correr(Path.Combine(Guia(), "tools", "verificar.sh"), ["_precondicion_compartido"], extra: new()
        {
            ["OLLAMA_ENDPOINT"] = servidor.Url,
            ["OLLAMA_VERSION_ESPERADA"] = "0.0.1-prueba",
            ["DIGEST_4B_ESPERADO"] = "aaaaaaaaaaaa",
            ["DIGEST_EMB_ESPERADO"] = "cccccccccccc",
        });

        var turnos = File.ReadAllText(Path.Combine(entorno.Registros, "turnos.jsonl"));
        Assert.That(turnos, Does.Contain("\"tipo\":\"servidor-compartido\""));
        if (coincide)
        {
            Assert.That(r.Codigo, Is.EqualTo(0), r.Salida);
            Assert.That(turnos, Does.Contain("\"coincide\":true"));
        }
        else
        {
            Assert.That(r.Codigo, Is.Not.EqualTo(0), r.Salida);
            Assert.That(turnos, Does.Contain("\"coincide\":false").And.Contain("\"digest\":\"bbbbbbbbbbbb\""));
            Assert.That(r.Salida, Does.Contain("no medido"));
        }
        Assert.That(turnos, Does.Not.Contain(servidor.Url).And.Not.Contain("127.0.0.1"), "el registro no guarda direcciones");
    }

    // ------------------------------------------------------------------ apoyo

    /// <summary>La carpeta de la guía: ARNES_GUIA (la monta el arnés en el contenedor) o la que contiene tools/verificar.sh.</summary>
    private static string Guia()
    {
        if (Environment.GetEnvironmentVariable("ARNES_GUIA") is { Length: > 0 } g && File.Exists(Path.Combine(g, "tools", "verificar.sh")))
        {
            return g;
        }
        foreach (var inicio in new[] { Directory.GetCurrentDirectory(), TestContext.CurrentContext.TestDirectory })
        {
            for (var d = new DirectoryInfo(inicio); d is not null; d = d.Parent)
            {
                if (File.Exists(Path.Combine(d.FullName, "tools", "verificar.sh"))) return d.FullName;
            }
        }
        Assert.Inconclusive("no se encontró tools/verificar.sh (exportá ARNES_GUIA con la carpeta de la guía)");
        return string.Empty;
    }

    private sealed record Resultado(int Codigo, string Salida);

    /// <summary>Carpeta temporal con un «docker» falso, los registros y un entorno sin variables del laboratorio.</summary>
    private sealed class EntornoDeArnes : IDisposable
    {
        private readonly string _raiz = Path.Combine(Path.GetTempPath(), "arnes-" + Guid.NewGuid().ToString("n"));
        private readonly string _log;

        public EntornoDeArnes(bool dockerEjecutaCurl = false)
        {
            Directory.CreateDirectory(Path.Combine(_raiz, "bin"));
            Registros = Path.Combine(_raiz, "registros");
            Directory.CreateDirectory(Registros);
            _log = Path.Combine(_raiz, "docker.log");
            var docker = Path.Combine(_raiz, "bin", "docker");
            File.WriteAllText(docker, "#!/bin/sh\nprintf '%s\\n' \"$*\" >> \"$DOCKER_LOG\"\n" + (dockerEjecutaCurl
                ? "while [ $# -gt 0 ] && [ \"$1\" != curl ]; do shift; done\n[ $# -gt 0 ] || exit 0\nexec \"$@\"\n"
                : "exit 0\n"));
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(docker, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }
        }

        public string Registros { get; }

        public string[] LlamadasADocker() => File.Exists(_log) ? File.ReadAllLines(_log) : [];

        public Resultado Correr(string script, string[] argumentos, string? sin = null, Dictionary<string, string>? extra = null)
        {
            var psi = new ProcessStartInfo("bash") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            psi.ArgumentList.Add(script);
            foreach (var a in argumentos) psi.ArgumentList.Add(a);
            foreach (var clave in psi.Environment.Keys.ToList())
            {
                if (clave.StartsWith("OLLAMA_") || clave.StartsWith("RLLM_") || clave.StartsWith("DIGEST_") ||
                    clave.StartsWith("TURNO_") || clave is "EMB_PLAN" or "COMPUERTAS_DIR")
                {
                    psi.Environment.Remove(clave);
                }
            }
            var valores = new Dictionary<string, string>
            {
                ["PATH"] = Path.Combine(_raiz, "bin") + ":" + Environment.GetEnvironmentVariable("PATH"),
                ["DOCKER_LOG"] = _log,
                ["TMPDIR"] = _raiz,
                ["OLLAMA_SERVIDOR"] = "compartido",
                ["RLLM_MAQUINA"] = "estacion-1",
                ["RLLM_MAQUINA_PROPIO"] = "estacion-2",
                ["RLLM_DIR"] = Registros,
                ["OLLAMA_VERSION_ESPERADA"] = "0.0.0",
                ["DIGEST_4B_ESPERADO"] = "000000000000",
                ["DIGEST_EMB_ESPERADO"] = "-",
                ["RLLM_TURNO_ID"] = "prueba",
            };
            foreach (var (k, v) in extra ?? new Dictionary<string, string>()) valores[k] = v;
            if (sin is not null) valores.Remove(sin);
            foreach (var (k, v) in valores) psi.Environment[k] = v;

            using var p = Process.Start(psi)!;
            var salida = p.StandardOutput.ReadToEndAsync();
            var error = p.StandardError.ReadToEndAsync();
            if (!p.WaitForExit(TimeSpan.FromSeconds(60)))
            {
                p.Kill(true);
                Assert.Fail("el arnés no terminó en 60 s");
            }
            return new Resultado(p.ExitCode, salida.Result + error.Result);
        }

        public void Dispose()
        {
            try { Directory.Delete(_raiz, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    /// <summary>Un servidor HTTP en loopback que responde rutas fijas con JSON (sin modelo).</summary>
    private sealed class ServidorGuionado : IDisposable
    {
        private readonly HttpListener _escucha = new();
        private readonly Dictionary<string, string> _respuestas;
        private readonly Task _atiende;

        public ServidorGuionado(params (string Ruta, string Json)[] respuestas)
        {
            _respuestas = respuestas.ToDictionary(r => r.Ruta, r => r.Json);
            var socket = new TcpListener(IPAddress.Loopback, 0);
            socket.Start();
            var puerto = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();
            Url = $"http://127.0.0.1:{puerto}";
            _escucha.Prefixes.Add(Url + "/");
            _escucha.Start();
            _atiende = Task.Run(Atender);
        }

        public string Url { get; }

        private async Task Atender()
        {
            while (_escucha.IsListening)
            {
                HttpListenerContext c;
                try { c = await _escucha.GetContextAsync(); }
                catch (HttpListenerException) { return; }
                catch (ObjectDisposedException) { return; }
                var ruta = c.Request.Url?.AbsolutePath ?? string.Empty;
                var conocida = _respuestas.TryGetValue(ruta, out var json);
                var bytes = Encoding.UTF8.GetBytes(conocida ? json! : "{}");
                c.Response.ContentType = "application/json";
                c.Response.StatusCode = conocida ? 200 : 404;
                await c.Response.OutputStream.WriteAsync(bytes);
                c.Response.Close();
            }
        }

        public void Dispose()
        {
            _escucha.Stop();
            _escucha.Close();
            try { _atiende.Wait(TimeSpan.FromSeconds(5)); } catch (AggregateException) { }
        }
    }
}
