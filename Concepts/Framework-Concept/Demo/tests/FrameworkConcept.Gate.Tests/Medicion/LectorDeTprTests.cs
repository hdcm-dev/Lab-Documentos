using FrameworkConcept.Gate.Tests.Infraestructura;
using FrameworkConcept.Tools;

namespace FrameworkConcept.Gate.Tests.Medicion;

/// <summary>
/// P1 — La lectura del TPR (folio 042). En el cierre del expediente 0001 la marca «primer-render» no llegó en 5
/// arranques en frío y el registro no decía por qué: la pantalla del emulador estaba apagada, la actividad no pasó
/// al frente y el WebView no pidió el contenido. Estas pruebas fijan:
///   - el cálculo: inicio del proceso (primera línea FC menos su t=) → línea de consola de la marca, por PID;
///   - que una corrida sin marca se distinga según el WebView haya empezado a cargar o no;
///   - que el texto que se busca siga siendo el que escriben puente.js y el cascarón.
/// Las líneas son copias de logcat del emulador de referencia (2026-10-27), con los mensajes recortados.
/// </summary>
[TestFixture]
public class LectorDeTprTests
{
    // Arranque con la pantalla encendida: t=3693ms a las 16:02:41.799 → el proceso arrancó a las 16:02:38.106.
    private static readonly string[] ConPantallaEncendida =
    [
        "10-27 16:02:41.799 I/FC      ( 3053): t=3693ms arranque: pantalla visible",
        "10-27 16:02:50.203 I/FC      ( 3053): t=12104ms arranque: ContenidoListo 1.0.0+5d5c2b4",
        "10-27 16:03:03.100 I/FC      ( 3053): t=24997ms contenido: carga iniciada (1.0.0+5d5c2b4)",
        "10-27 16:03:05.106 I/chromium( 3053): [INFO:CONSOLE:83] \"FC-MARCA primer-render contenido=1.0.0+5d5c2b4 perf.now=3764ms\", source: https://0.0.0.1/js/puente.js (83)",
        "10-27 16:03:05.200 I/FC      ( 3053): t=27094ms puente Listo(1.0.0+5d5c2b4)"
    ];

    // Arranque con la pantalla apagada (lo que pasó en el cierre): ni un pedido del WebView, sólo el temporizador de 10 s.
    private static readonly string[] ConPantallaApagada =
    [
        "10-27 16:03:14.381 I/FC      ( 3867): t=396ms arranque: pantalla visible",
        "10-27 16:03:14.473 W/ActivityTaskManager(  507): Activity pause timeout for ActivityRecord{b0fe11f u0 com.frameworkconcept.demo/.MainActivity}",
        "10-27 16:03:18.816 I/FC      ( 3867): t=4840ms contenido: hybridwebview agregado (1.0.0+5d5c2b4)",
        "10-27 16:03:28.828 I/FC      ( 3867): t=14851ms contenido: consulta del arranque en frío (sin Listo a los 10 s)"
    ];

    [Test]
    [Description("P1 — TPR = hora de la marca menos el inicio del proceso (hora de la primera línea FC menos su t=)")]
    public void CalculaElTprDesdeElInicioDelProceso()
    {
        var corridas = LectorDeTpr.Leer(ConPantallaEncendida);

        Assert.That(corridas, Has.Count.EqualTo(1));
        Assert.That(corridas[0].Pid, Is.EqualTo(3053));
        Assert.That(corridas[0].TprMs, Is.EqualTo(27000));   // 16:03:05.106 − 16:02:38.106
        Assert.That(corridas[0].Diagnostico, Is.EqualTo("ok"));
    }

    [Test]
    [Description("P1 — Una corrida sin marca y sin pedidos del WebView no se lee como «no llegó»: dice que no cargó")]
    public void SinMarcaNiCargaDiceQueElWebViewNoPidioElContenido()
    {
        var corridas = LectorDeTpr.Leer(ConPantallaApagada);

        Assert.That(corridas, Has.Count.EqualTo(1));
        Assert.That(corridas[0].TprMs, Is.Null);
        Assert.That(corridas[0].CargaIniciada, Is.False);
        Assert.That(corridas[0].Diagnostico, Does.Contain("el WebView no pidió el contenido"));
    }

    [Test]
    [Description("P1 — Varias corridas en un mismo registro se separan por PID; también lee «-v threadtime»")]
    public void SeparaLasCorridasPorPidYLeeLosDosFormatos()
    {
        string[] threadtime =
        [
            "10-27 13:15:47.222 12627 12627 I FC      : t=746ms arranque: pantalla visible",
            "10-27 13:15:49.900 12627 12648 I FC      : t=3424ms contenido: carga iniciada (1.0.0+5d5c2b4)",
            "10-27 13:15:50.476 12627 12627 I chromium: [INFO:CONSOLE:83] \"FC-MARCA primer-render contenido=1.0.0+5d5c2b4 perf.now=900ms\", source: https://0.0.0.1/js/puente.js (83)"
        ];

        var corridas = LectorDeTpr.Leer([.. ConPantallaApagada, .. ConPantallaEncendida, .. threadtime]);

        Assert.That(corridas.Select(c => c.Pid), Is.EqualTo(new[] { 3867, 3053, 12627 }));
        Assert.That(corridas.Select(c => c.TprMs), Is.EqualTo(new long?[] { null, 27000, 4000 }));
    }

    [Test]
    [Description("P1 — La marca que se busca es la que escribe puente.js, y la carga iniciada la que registra el cascarón")]
    public void LosTextosBuscadosSonLosQueSeEscriben()
    {
        var puente = File.ReadAllText(Repositorio.Ruta("src/FrameworkConcept.Content/wwwroot/js/puente.js"));
        var pantalla = File.ReadAllText(Repositorio.Ruta("src/FrameworkConcept.Shell/Pages/ContenidoPage.xaml.cs"));

        Assert.That(puente, Does.Contain($"'{LectorDeTpr.TextoDeLaMarca} contenido='"),
            "puente.js dejó de escribir la marca que lee «tpr»: cambiá los dos juntos (P1).");
        Assert.That(pantalla, Does.Contain(LectorDeTpr.TextoDeCargaIniciada),
            "ContenidoPage dejó de registrar la carga iniciada que lee «tpr»: cambiá los dos juntos (P1).");
    }
}
