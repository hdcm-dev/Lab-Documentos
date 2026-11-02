// Nuevo (REC-22, M-I): el lector del prefijo cacheado cableado por la fábrica real, contra un servidor
// guionado en loopback (sin modelo). Después de un pedido, la carpeta de registros sólo tiene el
// registro R-LLM, con los contadores y sin el cuerpo, y la compuerta de cabeceras no encuentra ningún
// cuerpo capturado que revisar.
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Comun.Pruebas.Cableado;

public partial class ArnesTests
{
    [Test]
    public async Task Lector_PorLaFabrica_RegistraContadores_SinPersistirCuerpos()
    {
        using var servidor = new ServidorGuionado(("/api/chat", LecturaDelUsoTests.NdjsonFijo));
        var dir = Path.Combine(Path.GetTempPath(), "rllm-fabrica-" + Guid.NewGuid().ToString("n"));
        try
        {
            var config = new ConfiguracionDelModelo { RegistroDir = dir, Endpoint = servidor.Url };
            var chat = FabricaDeClientes.CrearChatOllama(servidor.Url, "modelo-falso", config);
            List<ChatMessage> enviados = [new(ChatRole.User, "¿Cuántas localidades hay cargadas en Corrientes?")];
            var opciones = new ChatOptions();

            var respuesta = await chat.GetResponseAsync(enviados, opciones);
            string ruta;
            using (var registro = new EscritorDeRegistro(new ContextoDeCorrida
            {
                Ejemplo = "fabrica", Configuracion = config, Adaptador = Adaptador.OllamaSharp, Modelo = "modelo-falso", Maquina = "guion"
            }))
            {
                registro.Escribir(registro.Llamada("e01", 1, enviados, opciones, respuesta, TimeSpan.Zero));
                ruta = registro.Ruta;
            }

            using var linea = JsonDocument.Parse(File.ReadAllLines(ruta).Single());
            Assert.Multiple(() =>
            {
                Assert.That(respuesta.Text, Is.EqualTo("Hay 1."));
                Assert.That(linea.RootElement.GetProperty("prompt_eval_cached_count").GetInt64(), Is.EqualTo(706));
                Assert.That(linea.RootElement.GetProperty("estado_cache").GetString(), Is.EqualTo("cacheado"));
                Assert.That(Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Select(Path.GetFileName), Is.EqualTo(new[] { "fabrica.jsonl" }));
                Assert.That(File.ReadAllText(ruta), Does.Not.Contain("done_reason").And.Not.Contain("\"messages\"")
                    .And.Not.Contain(servidor.Url).And.Not.Contain("127.0.0.1"));
            });

            // La compuerta de cabeceras sobre la carpeta de registros: no hay cuerpos capturados (rc 2,
            // «ningún cuerpo»), porque el lector no los escribe; si los hubiera, tendría que dar 0.
            var (codigo, salida) = CorrerCompuertaDeCabeceras(dir);
            Assert.That(codigo, Is.EqualTo(2), salida);
            Assert.That(salida, Does.Contain("ningún cuerpo capturado"));
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    private static (int Codigo, string Salida) CorrerCompuertaDeCabeceras(string carpeta)
    {
        var psi = new ProcessStartInfo("bash") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        psi.ArgumentList.Add(Path.Combine(Guia(), "tools", "compuerta-cabeceras.sh"));
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
