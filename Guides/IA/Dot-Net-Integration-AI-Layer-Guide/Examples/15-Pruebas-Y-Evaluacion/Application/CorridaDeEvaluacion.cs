// Nuevo (plan §E.2, fila 15; guía §10.5–§10.6). Corre los 26 casos: primero los evaluadores
// deterministas (sin modelo) y después RelevanceEvaluator y GroundednessEvaluator con el juez, a
// través del reporte de M.E.AI.Evaluation.Reporting con caché de respuestas en disco. Escribe el
// registro R-LLM (una línea por llamada al juez y una por entrada) y comprueba los invariantes de
// código de --verificar: lo que la aplicación garantiza, no lo que puntúa el juez.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>Lo que se supo de un caso: deterministas, lecturas del juez y el veredicto del oráculo.</summary>
public sealed record ResultadoDeCaso(
    CasoDeEvaluacion Caso,
    string Determinista,
    bool CitaValida,
    IReadOnlyList<LecturaDeEvaluador> Lecturas,
    VeredictoDelJuez Veredicto);

public sealed record ResultadoDeLaEvaluacion(IReadOnlyList<ResultadoDeCaso> Casos, IReadOnlyList<string> InvariantesQueFallan, int LlamadasAlJuez);

public sealed class CorridaDeEvaluacion(
    GrabadorDeLlamadas juez,
    EscritorDeRegistro registro,
    Func<ChatOptions?, ChatOptions?> opcionesEfectivas,
    string ejemplo,
    string almacenDelReporte,
    string ejecucion,
    CalibracionDelJuez calibracion)
{
    public static readonly IReadOnlyList<string> Evaluadores = [NombresDeMetricas.Relevance, NombresDeMetricas.Groundedness];

    public async Task<ResultadoDeLaEvaluacion> CorrerAsync(IReadOnlyList<CasoDeEvaluacion> casos, Plazo plazo, CancellationToken cancellationToken = default)
    {
        // El juez es el cliente que entra por la capa única (razonamiento apagado, num_ctx, temperatura).
        var configuracionDelJuez = new ChatConfiguration(juez);
        // Un reporte por evaluador y por grupo: así una respuesta «correcta tal cual» de la calibración,
        // idéntica a su salida de 07, no sale de la caché de la otra (cachingKeys distintas).
        var reportes = new Dictionary<(string, string), ReportingConfiguration>();
        foreach (var grupo in new[] { GrupoDeEntrada.Salidas07, GrupoDeEntrada.Calibracion })
        {
            reportes[(grupo, NombresDeMetricas.Relevance)] = DiskBasedReportingConfiguration.Create(
                storageRootPath: almacenDelReporte, evaluators: [new RelevanceEvaluator()], chatConfiguration: configuracionDelJuez,
                enableResponseCaching: true, executionName: ejecucion, cachingKeys: [grupo, NombresDeMetricas.Relevance]);
            reportes[(grupo, NombresDeMetricas.Groundedness)] = DiskBasedReportingConfiguration.Create(
                storageRootPath: almacenDelReporte, evaluators: [new GroundednessEvaluator()], chatConfiguration: configuracionDelJuez,
                enableResponseCaching: true, executionName: ejecucion, cachingKeys: [grupo, NombresDeMetricas.Groundedness]);
        }

        var resultados = new List<ResultadoDeCaso>();
        var fallan = new List<string>();
        var llamadas = 0;
        foreach (var caso in casos)
        {
            // 1. Deterministas: siempre, sin modelo y antes que el juez.
            var (determinista, citaValida, _) = await Deterministas.EvaluarAsync(caso, cancellationToken);
            var esperadoDet = caso.Grupo == GrupoDeEntrada.Calibracion ? caso.Anotacion!.Determinista : caso.Resultado07!;
            if (determinista != esperadoDet)
                fallan.Add($"{caso.EntradaId}: el determinista dio «{determinista}» y se esperaba «{esperadoDet}»");

            if (plazo.Vencido)
            {
                Cerrar(caso, determinista, citaValida, [], new("excluida", "plazo", null));
                continue;
            }

            // 2. El juez: un evaluador por vez (sin concurrencia: el guion del modo falso responde en orden).
            var lecturas = new List<LecturaDeEvaluador>();
            string? exclusion = null;
            var n = 0;
            foreach (var evaluador in Evaluadores)
            {
                var esperado = caso.Esperado(evaluador);
                EvaluationResult resultado;
                try
                {
                    await using var corrida = await reportes[(caso.Grupo, evaluador)].CreateScenarioRunAsync(
                        scenarioName: caso.EntradaId, iterationName: evaluador, cancellationToken: cancellationToken);
                    IEnumerable<EvaluationContext>? contexto = evaluador == NombresDeMetricas.Groundedness
                        ? [new GroundednessEvaluatorContext(caso.Contexto)]
                        : null;
                    resultado = await corrida.EvaluateAsync(
                        [new ChatMessage(ChatRole.User, caso.Pregunta.Texto)],
                        new ChatResponse(new ChatMessage(ChatRole.Assistant, caso.Respuesta)),
                        contexto, cancellationToken);
                }
                catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
                {
                    exclusion = $"error de llamada ({e.GetType().Name})";
                    juez.Tomar();
                    break;
                }

                var metrica = resultado.Get<NumericMetric>(evaluador);
                var valorMeai = metrica.Value is double v ? (int?)(int)v : null;
                var diagnostico = metrica.Diagnostics?.FirstOrDefault(d => d.Severity == EvaluationDiagnosticSeverity.Error)?.Message;
                var grabadas = juez.Tomar();
                if (grabadas.Count == 0)
                {
                    exclusion = "caché del reporte: el juez no se llamó en esta corrida";
                    continue;
                }
                if (grabadas.Count > 1) fallan.Add($"{caso.EntradaId}: {evaluador} hizo {grabadas.Count} llamadas (se espera 1)");

                foreach (var g in grabadas)
                {
                    n++;
                    var efectivas = opcionesEfectivas(g.Opciones);
                    var paso = $"evaluador={evaluador};grupo={caso.Grupo};esperado={esperado};umbral={calibracion.Umbral}" +
                               (caso.Tipo is null ? "" : $";tipo={caso.Tipo};anotador={calibracion.Anotador}") +
                               $";meai={(valorMeai?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "sin-valor")}" +
                               (diagnostico is null ? "" : $";diagnostico={Limpiar(diagnostico)}");
                    var linea = registro.Llamada(caso.EntradaId, n, g.Mensajes, efectivas, g.Respuesta, g.Duracion, paso,
                        recuperados: caso.Recuperados.Select(r => new Recuperado(r.Fuente, r.Puntaje)).ToList(),
                        contexto: caso.Contexto) with { EntradasHash = caso.EntradasHash, EntradasVersion = "v1" };
                    registro.Escribir(linea);
                    llamadas++;
                    fallan.AddRange(InvariantesDeLaLlamada(caso.EntradaId, evaluador, linea));
                    if (linea.TruncadoPosible == true) exclusion = "truncado_posible";
                }

                var lectura = LecturaDelJuez.Puntaje(grabadas[^1].Respuesta.Text);
                if (lectura != valorMeai)
                    fallan.Add($"{caso.EntradaId}: {evaluador}: la lectura del oráculo ({lectura?.ToString() ?? "nula"}) no coincide con la de M.E.AI ({valorMeai?.ToString() ?? "nula"})");
                // REC-18: sin Interpretation, Inconclusive o con diagnóstico Error, M.E.AI no la interpreta;
                // si el oráculo leyó un puntaje igual, la corrida no queda en verde.
                if (lectura is not null && !InterpretacionDeMeai.Interpretable(metrica, out var porQue))
                    fallan.Add($"{caso.EntradaId}: {evaluador}: el oráculo leyó {lectura} y M.E.AI no la interpreta ({porQue})");
                lecturas.Add(new(evaluador, lectura, esperado));
            }

            var veredicto = exclusion is not null
                ? new VeredictoDelJuez("excluida", exclusion, null)
                : OraculoDelJuez.Evaluar(caso.Grupo, lecturas, calibracion.Umbral);
            Cerrar(caso, determinista, citaValida, lecturas, veredicto);
        }
        return new(resultados, fallan, llamadas);

        void Cerrar(CasoDeEvaluacion caso, string determinista, bool citaValida, IReadOnlyList<LecturaDeEvaluador> lecturas, VeredictoDelJuez veredicto)
        {
            resultados.Add(new(caso, determinista, citaValida, lecturas, veredicto));
            registro.Escribir(new RegistroDeEntrada
            {
                Ejemplo = ejemplo, EntradaId = caso.EntradaId, Oraculo = OraculoDelJuez.Nombre,
                Resultado = veredicto.Resultado, Motivo = veredicto.Motivo,
                EntradasVersion = "v1", EntradasHash = caso.EntradasHash
            });
        }
    }

    /// <summary>
    /// Regla de calibración (definida antes de juzgar): el juez queda «no descartado» sólo si los 10 casos
    /// de calibración tienen veredicto (ninguno excluido) y acierta al menos <c>aciertos_minimos</c>. Una
    /// salida no interpretable es una falla: con todas no interpretables, el juez queda descartado.
    /// </summary>
    public static bool JuezNoDescartado(ResultadoDeLaEvaluacion resultado, CalibracionDelJuez calibracion)
    {
        var calib = resultado.Casos.Where(r => r.Caso.Grupo == GrupoDeEntrada.Calibracion && r.Veredicto.Resultado != "excluida").ToList();
        return calib.Count == calibracion.Casos.Count && calib.Count(r => r.Veredicto.Resultado == "pasa") >= calibracion.AciertosMinimos;
    }

    /// <summary>La capa única también rige para el juez: razonamiento apagado, num_ctx y temperatura 0.</summary>
    public static IEnumerable<string> InvariantesDeLaLlamada(string entradaId, string evaluador, RegistroDeLlamada linea)
    {
        if (linea.Razonamiento != "None") yield return $"{entradaId}: {evaluador}: razonamiento no apagado";
        if (linea.NumCtx is null or <= 0) yield return $"{entradaId}: {evaluador}: sin num_ctx explícito";
        if (linea.Temperature is not 0f) yield return $"{entradaId}: {evaluador}: temperatura {linea.Temperature?.ToString() ?? "nula"}, no 0";
    }

    private static string Limpiar(string texto)
    {
        var s = texto.ReplaceLineEndings(" ").Replace(';', ',');
        return s.Length <= 240 ? s : s[..240] + "…";
    }
}
