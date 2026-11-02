// Nuevo (plan §E.2, fila 15: «evaluadores deterministas primero»; guía §10.3, §10.5). Tres
// IEvaluator de Microsoft.Extensions.AI.Evaluation que NO llaman a ningún modelo: la misma interfaz
// que RelevanceEvaluator y GroundednessEvaluator, pero la decisión es una cuenta en código.
//   · cita válida y afirmación central: el código de 07 (ValidadorDeCitas, OraculoDeCitas), enlazado;
//   · valor fuera de catálogo: el contrato de la salida estructurada de 04 (§10.3), contra Catalogos.
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>Contexto que necesitan los evaluadores de citas: la pregunta del oro y lo recuperado en 07.</summary>
public sealed class ContextoDeRecuperacion(PreguntaDeOro pregunta, IReadOnlyList<Recuperacion> recuperados)
    : EvaluationContext(Nombre, string.Join("\n", recuperados.Select(r => $"[Fuente: {r.Fuente}] {r.Texto}")))
{
    public const string Nombre = "Recuperación de 07";

    public PreguntaDeOro Pregunta { get; } = pregunta;
    public IReadOnlyList<Recuperacion> Recuperados { get; } = recuperados;

    public static ContextoDeRecuperacion De(IEnumerable<EvaluationContext>? contexto) =>
        contexto?.OfType<ContextoDeRecuperacion>().FirstOrDefault()
        ?? throw new InvalidOperationException($"Falta el contexto «{Nombre}».");
}

/// <summary>¿Toda cita de la respuesta apunta a un fragmento recuperado? (la garantía de 07).</summary>
public sealed class EvaluadorDeCitaValida : IEvaluator
{
    public const string Metrica = "Cita válida";

    public IReadOnlyCollection<string> EvaluationMetricNames => [Metrica];

    public ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages, ChatResponse modelResponse, ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null, CancellationToken cancellationToken = default)
    {
        var ctx = ContextoDeRecuperacion.De(additionalContext);
        var v = ValidadorDeCitas.Validar(modelResponse.Text ?? string.Empty, ctx.Recuperados.Select(r => r.Fuente).ToHashSet(StringComparer.Ordinal));
        // En una no respondible, el «No sé» pedido (sin citas) es la respuesta válida.
        var ok = ctx.Pregunta.Respondible ? v.Veredicto == Veredicto.Aceptada : v.Veredicto == Veredicto.NoSe;
        var metrica = new BooleanMetric(Metrica, ok, v.Veredicto.ToString());
        metrica.Interpretation = new EvaluationMetricInterpretation(ok ? EvaluationRating.Good : EvaluationRating.Unacceptable, failed: !ok, v.Veredicto.ToString());
        return ValueTask.FromResult(new EvaluationResult(metrica));
    }
}

/// <summary>El oráculo de la afirmación central de 07 (H-029), como evaluador.</summary>
public sealed class EvaluadorDeAfirmacionCentral : IEvaluator
{
    public const string Metrica = "Afirmación central";

    public IReadOnlyCollection<string> EvaluationMetricNames => [Metrica];

    public ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages, ChatResponse modelResponse, ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null, CancellationToken cancellationToken = default)
    {
        var ctx = ContextoDeRecuperacion.De(additionalContext);
        var e = OraculoDeCitas.Evaluar(ctx.Pregunta, modelResponse.Text ?? string.Empty, ctx.Recuperados);
        var ok = e.Resultado == "pasa";
        var metrica = new BooleanMetric(Metrica, ok, e.Motivo);
        metrica.Interpretation = new EvaluationMetricInterpretation(ok ? EvaluationRating.Good : EvaluationRating.Unacceptable, failed: !ok, e.Motivo);
        return ValueTask.FromResult(new EvaluationResult(metrica));
    }
}

/// <summary>
/// Contrato de la salida estructurada de 04 (§10.3): un JSON {"medios": [...], "frecuencia": "...",
/// "motivo": "..."} cuyas claves pertenecen a <see cref="Catalogos" />. Malformado → error de
/// diagnóstico (nunca «pasa»); clave inventada («avion») → falla con la clave en el motivo.
/// </summary>
public sealed class EvaluadorDeCatalogo : IEvaluator
{
    public const string Metrica = "Dentro del catálogo";

    public IReadOnlyCollection<string> EvaluationMetricNames => [Metrica];

    public ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages, ChatResponse modelResponse, ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null, CancellationToken cancellationToken = default)
    {
        var metrica = new BooleanMetric(Metrica);
        try
        {
            using var doc = JsonDocument.Parse(modelResponse.Text ?? string.Empty);
            var fuera = new List<string>();
            void Revisar(string campo, IEnumerable<string> claves, IReadOnlyList<(string Clave, string Etiqueta)> catalogo) =>
                fuera.AddRange(claves.Where(c => catalogo.All(x => x.Clave != c)).Select(c => $"{campo}:{c}"));

            var raiz = doc.RootElement;
            var medios = raiz.TryGetProperty("medios", out var m) && m.ValueKind == JsonValueKind.Array
                ? m.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                : [];
            if (medios.Count == 0) fuera.Add("medios:(vacío)");
            Revisar("medios", medios, Catalogos.Medios);
            if (raiz.TryGetProperty("frecuencia", out var f)) Revisar("frecuencia", [f.GetString() ?? ""], Catalogos.Frecuencias);
            if (raiz.TryGetProperty("motivo", out var mo)) Revisar("motivo", [mo.GetString() ?? ""], Catalogos.Motivos);

            metrica.Value = fuera.Count == 0;
            metrica.Reason = fuera.Count == 0 ? "todas las claves están en Catalogos" : $"fuera de catálogo: {string.Join(", ", fuera)}";
        }
        catch (JsonException e)
        {
            metrica.AddDiagnostics(EvaluationDiagnostic.Error($"Salida malformada: no es JSON ({e.GetType().Name})."));
            metrica.Reason = "malformada";
        }
        var ok = metrica.Value == true;
        metrica.Interpretation = new EvaluationMetricInterpretation(ok ? EvaluationRating.Good : EvaluationRating.Unacceptable, failed: !ok, metrica.Reason);
        return ValueTask.FromResult(new EvaluationResult(metrica));
    }
}

/// <summary>Las dos de citas juntas, en el orden en que se aplican.</summary>
public static class Deterministas
{
    public static readonly IReadOnlyList<IEvaluator> DeCitas = [new EvaluadorDeCitaValida(), new EvaluadorDeAfirmacionCentral()];

    /// <summary>
    /// «pasa» con la misma regla que el oráculo rag-con-citas de 07: en las respondibles con valor, la
    /// afirmación central; en las sin valor, la cita; en las no respondibles, el «No sé».
    /// </summary>
    public static async Task<(string Resultado, bool CitaValida, bool Afirmacion)> EvaluarAsync(CasoDeEvaluacion caso, CancellationToken ct = default)
    {
        var contexto = new ContextoDeRecuperacion(caso.Pregunta, caso.Recuperados);
        IEnumerable<ChatMessage> pregunta = [new ChatMessage(ChatRole.User, caso.Pregunta.Texto)];
        var respuesta = new ChatResponse(new ChatMessage(ChatRole.Assistant, caso.Respuesta));
        var cita = (await DeCitas[0].EvaluateAsync(pregunta, respuesta, null, [contexto], ct)).Get<BooleanMetric>(EvaluadorDeCitaValida.Metrica).Value == true;
        var afirmacion = (await DeCitas[1].EvaluateAsync(pregunta, respuesta, null, [contexto], ct)).Get<BooleanMetric>(EvaluadorDeAfirmacionCentral.Metrica).Value == true;
        return (afirmacion ? "pasa" : "falla", cita, afirmacion);
    }
}
