// Nuevo (recuperación 2026-10-28, REC-18; guía §10.5–§10.6). Dos piezas que suma la recuperación:
//
//   · InterpretacionDeMeai: cuándo una métrica de M.E.AI.Evaluation NO es interpretable. Sin valor, sin
//     Interpretation, con Interpretation Inconclusive o Unknown, o con un diagnóstico de severidad Error.
//     La corrida lo usa como invariante: si el oráculo leyó un puntaje y M.E.AI no lo interpreta, la
//     corrida no queda en verde (una prueba que pasa con todas las métricas Inconclusive no mide nada).
//   · CorridaDeEquivalencia: EquivalenceEvaluator de Quality contra la referencia del oro, sólo sobre las
//     salidas de 07 cuya pregunta es respondible. La referencia es el texto de los fragmentos que el oro
//     marca como relevantes (CorpusDePolicies, el mismo texto que vio 07). Es DESCRIPTIVA: no entra en el
//     oráculo «acuerdo-del-juez», no tiene línea de entrada en el registro ni es compuerta. El juez es el
//     mismo cliente, por la misma capa única de opciones. OJO con el formato: Equivalence NO pide las
//     etiquetas <S0>/<S1>/<S2> de Relevance y Groundedness; su plantilla termina en «stars:» y espera el
//     entero solo. Por eso su lectura es LecturaEntera, no LecturaDelJuez (lo comprueba una prueba contra
//     la lectura de M.E.AI 10.11.0).
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;

namespace MovilidadUrbana.Asistente.Application;

public static class InterpretacionDeMeai
{
    /// <summary>¿M.E.AI dejó esta métrica en condiciones de leerse? Si no, no es interpretable.</summary>
    public static bool Interpretable(EvaluationMetric metrica, out string motivo)
    {
        if (metrica.Diagnostics?.Any(d => d.Severity == EvaluationDiagnosticSeverity.Error) == true) { motivo = "diagnóstico Error"; return false; }
        if (metrica is NumericMetric { Value: null } or BooleanMetric { Value: null }) { motivo = "sin valor"; return false; }
        if (metrica.Interpretation is null) { motivo = "sin Interpretation"; return false; }
        if (metrica.Interpretation.Rating is EvaluationRating.Inconclusive or EvaluationRating.Unknown) { motivo = $"Interpretation {metrica.Interpretation.Rating}"; return false; }
        motivo = "";
        return true;
    }

    public static bool Interpretable(EvaluationMetric metrica) => Interpretable(metrica, out _);
}

/// <summary>Lectura del formato de Equivalence: toda la salida es un entero (sin etiquetas). Si no, null.</summary>
public static partial class LecturaEntera
{
    public static int? Puntaje(string? salida)
    {
        if (salida is null) return null;
        var m = Entero().Match(salida);
        return m.Success && int.TryParse(m.Groups["v"].Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var p) ? p : null;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"^\s*(?<v>[0-9]+)\s*$")]
    private static partial System.Text.RegularExpressions.Regex Entero();
}

/// <summary>La referencia del oro para EquivalenceEvaluator: los fragmentos relevantes de una pregunta respondible.</summary>
public static class ReferenciaDelOro
{
    public static string? Para(PreguntaDeOro pregunta)
    {
        if (!pregunta.Respondible || pregunta.Relevantes.Count == 0) return null;
        var fragmentos = CorpusDePolicies.Fragmentos().ToDictionary(f => f.Id, StringComparer.Ordinal);
        return string.Join("\n", pregunta.Relevantes.Select(id => fragmentos[id].Texto));
    }
}

/// <summary>Una lectura de Equivalence: el puntaje (entero solo), si M.E.AI la interpreta, o por qué se excluyó.</summary>
public sealed record LecturaDeEquivalencia(string EntradaId, int? Puntaje, bool InterpretableMeai, string? Exclusion)
{
    public bool Interpretable => Exclusion is null && Puntaje is not null && InterpretableMeai;
}

public sealed record ResultadoDeEquivalencia(IReadOnlyList<LecturaDeEquivalencia> Lecturas, IReadOnlyList<string> InvariantesQueFallan, int LlamadasAlJuez);

public sealed class CorridaDeEquivalencia(
    GrabadorDeLlamadas juez,
    EscritorDeRegistro registro,
    Func<ChatOptions?, ChatOptions?> opcionesEfectivas,
    string almacenDelReporte,
    string ejecucion,
    int umbral)
{
    /// <summary>Prefijo del entrada_id en el registro: distinto de «s07-», para que el oráculo jq no lo mezcle.</summary>
    public const string Prefijo = "eq-";

    public static IEnumerable<CasoDeEvaluacion> Elegibles(IEnumerable<CasoDeEvaluacion> casos) =>
        casos.Where(c => c.Grupo == GrupoDeEntrada.Salidas07 && ReferenciaDelOro.Para(c.Pregunta) is not null);

    public async Task<ResultadoDeEquivalencia> CorrerAsync(IReadOnlyList<CasoDeEvaluacion> casos, Plazo plazo, CancellationToken cancellationToken = default)
    {
        var reporte = DiskBasedReportingConfiguration.Create(
            storageRootPath: almacenDelReporte, evaluators: [new EquivalenceEvaluator()], chatConfiguration: new ChatConfiguration(juez),
            enableResponseCaching: true, executionName: ejecucion, cachingKeys: [GrupoDeEntrada.Salidas07, NombresDeMetricas.Equivalence]);

        var lecturas = new List<LecturaDeEquivalencia>();
        var fallan = new List<string>();
        var llamadas = 0;
        foreach (var caso in Elegibles(casos))
        {
            var id = Prefijo + caso.EntradaId;
            if (plazo.Vencido) { lecturas.Add(new(id, null, false, "plazo")); continue; }
            var referencia = ReferenciaDelOro.Para(caso.Pregunta)!;
            EvaluationResult resultado;
            try
            {
                await using var corrida = await reporte.CreateScenarioRunAsync(scenarioName: caso.EntradaId, iterationName: NombresDeMetricas.Equivalence, cancellationToken: cancellationToken);
                resultado = await corrida.EvaluateAsync(
                    [new ChatMessage(ChatRole.User, caso.Pregunta.Texto)],
                    new ChatResponse(new ChatMessage(ChatRole.Assistant, caso.Respuesta)),
                    [new EquivalenceEvaluatorContext(referencia)], cancellationToken);
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
            {
                juez.Tomar();
                lecturas.Add(new(id, null, false, $"error de llamada ({e.GetType().Name})"));
                continue;
            }

            var metrica = resultado.Get<NumericMetric>(NombresDeMetricas.Equivalence);
            var valorMeai = metrica.Value is double v ? (int?)(int)v : null;
            var interpretableMeai = InterpretacionDeMeai.Interpretable(metrica, out var porQue);
            var diagnostico = metrica.Diagnostics?.FirstOrDefault(d => d.Severity == EvaluationDiagnosticSeverity.Error)?.Message;
            var grabadas = juez.Tomar();
            if (grabadas.Count == 0) { lecturas.Add(new(id, null, false, "caché del reporte: el juez no se llamó en esta corrida")); continue; }
            if (grabadas.Count > 1) fallan.Add($"{id}: Equivalence hizo {grabadas.Count} llamadas (se espera 1)");

            string? exclusion = null;
            var n = 0;
            foreach (var g in grabadas)
            {
                var paso = $"evaluador={NombresDeMetricas.Equivalence};grupo={caso.Grupo};referencia=oro:{string.Join(",", caso.Pregunta.Relevantes)};umbral={umbral}" +
                           $";meai={(valorMeai?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "sin-valor")}" +
                           (interpretableMeai ? "" : $";no_interpretable={porQue}") +
                           (diagnostico is null ? "" : $";diagnostico={Limpiar(diagnostico)}");
                var linea = registro.Llamada(id, ++n, g.Mensajes, opcionesEfectivas(g.Opciones), g.Respuesta, g.Duracion, paso,
                    contexto: referencia) with { EntradasHash = caso.EntradasHash, EntradasVersion = "v1" };
                registro.Escribir(linea);
                llamadas++;
                fallan.AddRange(CorridaDeEvaluacion.InvariantesDeLaLlamada(id, NombresDeMetricas.Equivalence, linea));
                if (linea.TruncadoPosible == true) exclusion = "truncado_posible";
            }

            var lectura = LecturaEntera.Puntaje(grabadas[^1].Respuesta.Text);
            if (lectura != valorMeai)
                fallan.Add($"{id}: Equivalence: la lectura entera ({lectura?.ToString() ?? "nula"}) no coincide con la de M.E.AI ({valorMeai?.ToString() ?? "nula"})");
            if (lectura is not null && !interpretableMeai)
                fallan.Add($"{id}: Equivalence: se leyó {lectura} y M.E.AI no la interpreta ({porQue})");
            lecturas.Add(new(id, lectura, interpretableMeai, exclusion));
        }
        return new(lecturas, fallan, llamadas);
    }

    private static string Limpiar(string texto)
    {
        var s = texto.ReplaceLineEndings(" ").Replace(';', ',');
        return s.Length <= 240 ? s : s[..240] + "…";
    }
}
