// Nuevo (ejemplo 17; plan §E.2: «exportador en memoria»). Registra la fuente y el medidor que emite
// M.E.AI en el SDK de OpenTelemetry, como haría una app con OTLP (N5 §4.1), pero con el exportador en
// memoria: los spans y las métricas quedan en listas que el ejemplo y las pruebas leen.
using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MovilidadUrbana.Asistente.Infrastructure;

public sealed class CapturaDeTelemetria : IDisposable
{
    /// <summary>Nombre del ActivitySource y del Meter de M.E.AI 10.11.0 (verificado en N5 E6).</summary>
    public const string Fuente = "Experimental.Microsoft.Extensions.AI";

    /// <summary>Atributos Opt-In de contenido de la convención GenAI (N5 §2.1, F02).</summary>
    public static readonly string[] AtributosDeContenido =
        ["gen_ai.input.messages", "gen_ai.output.messages", "gen_ai.system_instructions"];

    private readonly TracerProvider _trazas;
    private readonly MeterProvider _metricas;
    private readonly MeterListener _instrumentos = new();
    private readonly List<Metric> _exportadas = [];
    private readonly SortedSet<string> _publicados = new(StringComparer.Ordinal);

    public CapturaDeTelemetria()
    {
        _trazas = Sdk.CreateTracerProviderBuilder().AddSource(Fuente).AddInMemoryExporter(Spans).Build();
        _metricas = Sdk.CreateMeterProviderBuilder().AddMeter(Fuente).AddInMemoryExporter(_exportadas).Build();
        // Los instrumentos se crean con el cliente de telemetría, aunque todavía no midan nada (N5 E6).
        _instrumentos.InstrumentPublished = (instrumento, _) =>
        {
            if (instrumento.Meter.Name == Fuente)
            {
                lock (_publicados) _publicados.Add(instrumento.Name);
            }
        };
        _instrumentos.Start();
    }

    /// <summary>Spans terminados, en el orden en que terminaron.</summary>
    public List<Activity> Spans { get; } = [];

    /// <summary>Instrumentos que publicó el medidor de M.E.AI.</summary>
    public IReadOnlyList<string> InstrumentosPublicados
    {
        get { lock (_publicados) return [.. _publicados]; }
    }

    /// <summary>Métricas que llegaron al exportador con al menos una medición.</summary>
    public IReadOnlyList<string> MetricasConMediciones()
    {
        _metricas.ForceFlush();
        return _exportadas.Select(m => m.Name).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
    }

    public static string? Atributo(Activity span, string clave) =>
        span.TagObjects.FirstOrDefault(t => t.Key == clave).Value switch
        {
            null => null,
            IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            var v => v.ToString()
        };

    /// <summary>¿El span trae algún atributo de contenido (mensajes o instrucciones)?</summary>
    public static bool TraeContenido(Activity span) =>
        span.TagObjects.Any(t => AtributosDeContenido.Contains(t.Key));

    /// <summary>Línea legible de un span: nombre y los atributos gen_ai.* sin contenido.</summary>
    public static string Describir(Activity span)
    {
        var atributos = span.TagObjects
            .Where(t => t.Key.StartsWith("gen_ai.", StringComparison.Ordinal) && !AtributosDeContenido.Contains(t.Key))
            .Select(t => $"{t.Key}={Atributo(span, t.Key)}");
        return $"{span.DisplayName} | {string.Join(", ", atributos)}";
    }

    public void Dispose()
    {
        _instrumentos.Dispose();
        _trazas.Dispose();
        _metricas.Dispose();
    }
}
