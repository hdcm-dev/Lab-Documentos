using System.Globalization;
using System.Text.RegularExpressions;

namespace FrameworkConcept.Tools;

/// <summary>
/// P1 — Lee el TPR (tiempo al primer render del contenido, desde el inicio del proceso) de un logcat de arranques en
/// frío. Lo usa «scripts/guion.sh tpr» y se puede correr sobre cualquier logcat guardado:
///
///   dotnet FrameworkConcept.Tools.dll tpr --logcat salida/evidencia/tpr-logcat.txt [--umbral 5000]
///
/// Cómo se calcula, por proceso (cada arranque en frío es un proceso nuevo, con su PID):
///   - el origen es el inicio del proceso: la hora de la primera línea «FC» menos su «t=…ms», que el cascarón cuenta
///     desde <c>Process.StartElapsedRealtime</c> (Shell/Bitacora.cs);
///   - el fin es la marca «FC-MARCA primer-render». Si el cascarón la leyó del WebView y la escribió en su bitácora
///     (línea «FC» con «tpr=…ms», Shell/Pages/ContenidoPage), vale ese tpr, contado desde el mismo inicio del
///     proceso; si no, la hora de la línea de consola que escribe puente.js, que el WebView pasa a logcat con la
///     etiqueta «chromium» en el emulador pero no en un teléfono real (moto g42, constancia 047).
///
/// Por qué existe (folio 042): en el cierre del expediente 0001 la marca «no llegó» en 5 arranques y el registro no
/// decía por qué. Era la pantalla apagada: la actividad nunca pasó al frente y el WebView no pidió ni el index.html.
/// Una corrida sin marca ya no se lee como «no llegó» a secas: se informa si el WebView empezó a cargar.
/// </summary>
public static class LectorDeTpr
{
    /// <summary>Texto que puente.js escribe en la consola con la marca (P1).</summary>
    public const string TextoDeLaMarca = "FC-MARCA primer-render";

    /// <summary>Lo que el cascarón registra cuando el WebView pide el runtime de .NET (Shell/Pages/ContenidoPage).</summary>
    public const string TextoDeCargaIniciada = "contenido: carga iniciada";

    // «adb logcat -v time»:       10-27 16:04:01.375 I/chromium( 3867): mensaje
    private static readonly Regex FormatoTime = new(
        @"^(?<hora>\d\d-\d\d \d\d:\d\d:\d\d\.\d{3}) [VDIWEF]/(?<etiqueta>[^(]+?)\s*\(\s*(?<pid>\d+)\):\s?(?<mensaje>.*)$",
        RegexOptions.Compiled);

    // «adb logcat -v threadtime»: 10-27 13:15:47.222 12627 12627 I FC      : mensaje
    private static readonly Regex FormatoThreadtime = new(
        @"^(?<hora>\d\d-\d\d \d\d:\d\d:\d\d\.\d{3})\s+(?<pid>\d+)\s+\d+ [VDIWEF] (?<etiqueta>\S+)\s*:\s?(?<mensaje>.*)$",
        RegexOptions.Compiled);

    private static readonly Regex TiempoDelCascaron = new(@"^t=(?<ms>\d+)ms ", RegexOptions.Compiled);

    private static readonly Regex TprDelCascaron = new(@" tpr=(?<ms>-?\d+)ms", RegexOptions.Compiled);

    /// <summary>Un arranque en frío leído del logcat.</summary>
    /// <param name="TprMs">null si la marca no está en el registro.</param>
    /// <param name="CargaIniciada">Si el WebView llegó a pedir el runtime de .NET.</param>
    public sealed record Corrida(int Pid, long? TprMs, bool CargaIniciada)
    {
        public string Diagnostico => TprMs is not null
            ? "ok"
            : CargaIniciada
                ? "sin primer-render: el contenido empezó a cargar pero no se dibujó en la ventana leída"
                : "sin primer-render: el WebView no pidió el contenido (¿la actividad no llegó al frente? pantalla apagada o bloqueada)";
    }

    /// <summary>Las corridas del registro, en el orden en que aparecen. Sólo cuentan los procesos con líneas «FC».</summary>
    public static IReadOnlyList<Corrida> Leer(IEnumerable<string> lineas)
    {
        var orden = new List<int>();
        var origen = new Dictionary<int, DateTime>();
        var marca = new Dictionary<int, DateTime>();
        var tprLeido = new Dictionary<int, long>();
        var carga = new HashSet<int>();

        foreach (var linea in lineas)
        {
            var m = FormatoTime.Match(linea.TrimEnd('\r'));
            if (!m.Success) m = FormatoThreadtime.Match(linea.TrimEnd('\r'));
            if (!m.Success) continue;

            var hora = DateTime.ParseExact("2000-" + m.Groups["hora"].Value, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var pid = int.Parse(m.Groups["pid"].Value, CultureInfo.InvariantCulture);
            var etiqueta = m.Groups["etiqueta"].Value.Trim();
            var mensaje = m.Groups["mensaje"].Value;

            if (etiqueta == "FC" && TiempoDelCascaron.Match(mensaje) is { Success: true } t)
            {
                if (!origen.ContainsKey(pid))
                {
                    origen[pid] = hora.AddMilliseconds(-long.Parse(t.Groups["ms"].Value, CultureInfo.InvariantCulture));
                    orden.Add(pid);
                }
                if (mensaje.Contains(TextoDeCargaIniciada, StringComparison.Ordinal)) carga.Add(pid);
                if (mensaje.Contains(TextoDeLaMarca, StringComparison.Ordinal)
                    && TprDelCascaron.Match(mensaje) is { Success: true } tpr && !tprLeido.ContainsKey(pid))
                    tprLeido[pid] = long.Parse(tpr.Groups["ms"].Value, CultureInfo.InvariantCulture);
            }
            else if (mensaje.Contains(TextoDeLaMarca, StringComparison.Ordinal) && !marca.ContainsKey(pid))
            {
                marca[pid] = hora;
            }
        }

        return orden.Select(pid => new Corrida(
            pid,
            tprLeido.TryGetValue(pid, out var leido) ? leido
                : marca.TryGetValue(pid, out var fin) ? (long)Math.Round((fin - origen[pid]).TotalMilliseconds) : null,
            carga.Contains(pid))).ToList();
    }

    /// <summary>
    /// Comando «tpr»: imprime una fila por corrida y el máximo. Sale 0 si todas tienen marca y el máximo no pasa el
    /// umbral; 1 si no.
    /// </summary>
    public static int Comando(string logcat, long umbralMs)
    {
        if (!File.Exists(logcat)) throw new ErrorDeUso($"no existe el logcat «{logcat}»");
        var corridas = Leer(File.ReadLines(logcat));
        if (corridas.Count == 0)
        {
            Console.WriteLine("tpr: no hay líneas «FC» del cascarón en el registro");
            return 1;
        }

        for (var i = 0; i < corridas.Count; i++)
        {
            var c = corridas[i];
            Console.WriteLine($"corrida {i + 1}  pid {c.Pid}  TPR {(c.TprMs is { } ms ? $"{ms} ms" : "—")}  {c.Diagnostico}");
        }

        var medidas = corridas.Where(c => c.TprMs is not null).Select(c => c.TprMs!.Value).ToList();
        var completas = medidas.Count == corridas.Count;
        if (medidas.Count == 0)
        {
            Console.WriteLine("tpr: NO OBTENIDO (ninguna corrida tiene primer-render)");
            return 1;
        }

        var maximo = medidas.Max();
        Console.WriteLine($"tpr: máximo {maximo} ms de {medidas.Count}/{corridas.Count} corridas; umbral {umbralMs} ms → "
                          + (!completas ? "INCOMPLETO" : maximo <= umbralMs ? "CUMPLE" : "NO CUMPLE"));
        return completas && maximo <= umbralMs ? 0 : 1;
    }
}
