// Nuevo (plan §E.2, fila 07). Corre las preguntas del oro por el asistente, escribe el registro
// R-LLM (una línea por llamada y una por entrada) y comprueba los invariantes de código de
// --verificar: lo que la aplicación garantiza, no lo que responde el modelo.
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Application;

public sealed record ResultadoDePregunta(PreguntaDeOro Pregunta, string EntradaId, ConsultaDeAyuda? Consulta, EvaluacionDeCitas Evaluacion);

public sealed record ResultadoDeLaCorrida(IReadOnlyList<ResultadoDePregunta> Resultados, IReadOnlyList<string> InvariantesQueFallan);

public sealed class CorridaDeAyuda(
    AsistenteDeAyuda asistente,
    EscritorDeRegistro registro,
    Func<ChatOptions, ChatOptions?> opcionesEfectivas,
    string ejemplo,
    string paso,
    Func<CancellationToken, Task<string?>>? estadoCacheAntes = null)
{
    /// <param name="preguntas">Pares (entrada_id, pregunta del oro); en la estabilidad, la misma pregunta con ids «r03#1»…</param>
    public async Task<ResultadoDeLaCorrida> CorrerAsync(
        IReadOnlyList<(string EntradaId, PreguntaDeOro Pregunta)> preguntas, Plazo plazo, CancellationToken cancellationToken = default)
    {
        var resultados = new List<ResultadoDePregunta>();
        var fallan = new List<string>();
        foreach (var (entradaId, pregunta) in preguntas)
        {
            if (plazo.Vencido)
            {
                Cerrar(entradaId, pregunta, null, new("excluida", false, false, false, "plazo"));
                continue;
            }
            var forzado = estadoCacheAntes is null ? null : await estadoCacheAntes(cancellationToken);
            var reloj = EscritorDeRegistro.Cronometro();
            ConsultaDeAyuda consulta;
            try
            {
                consulta = await asistente.PreguntarAsync(pregunta.Texto, cancellationToken);
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
            {
                Cerrar(entradaId, pregunta, null, new("excluida", false, false, false, $"error de llamada ({e.GetType().Name})"));
                continue;
            }
            var llamada = registro.Llamada(entradaId, 1, consulta.Mensajes, opcionesEfectivas(consulta.Opciones), consulta.Respuesta,
                reloj.Elapsed, $"{paso};veredicto={consulta.Validada.Veredicto}",
                recuperados: consulta.Recuperados.Select(r => new Recuperado(r.Fuente, r.Puntaje)).ToList(),
                contexto: consulta.Contexto, estadoCacheForzado: forzado);
            registro.Escribir(llamada);

            var evaluacion = llamada.TruncadoPosible == true
                ? new EvaluacionDeCitas("excluida", false, false, false, "truncado_posible")
                : OraculoDeCitas.Evaluar(pregunta, consulta.Respuesta.Text ?? string.Empty, consulta.Recuperados);
            Cerrar(entradaId, pregunta, consulta, evaluacion);
            fallan.AddRange(Invariantes(entradaId, consulta, llamada));
        }
        return new(resultados, fallan);

        void Cerrar(string entradaId, PreguntaDeOro pregunta, ConsultaDeAyuda? consulta, EvaluacionDeCitas evaluacion)
        {
            resultados.Add(new(pregunta, entradaId, consulta, evaluacion));
            registro.Escribir(new RegistroDeEntrada
            {
                Ejemplo = ejemplo, EntradaId = entradaId, Oraculo = OraculoDeCitas.Nombre,
                Resultado = evaluacion.Resultado, Motivo = evaluacion.Motivo
            });
        }
    }

    /// <summary>Invariantes de código por consulta. Vacía = se cumplen.</summary>
    public IEnumerable<string> Invariantes(string entradaId, ConsultaDeAyuda consulta, RegistroDeLlamada llamada)
    {
        var ids = consulta.Recuperados.Select(r => r.Fuente).ToList();
        var v = consulta.Validada;
        if (ids.Count != asistente.Top) yield return $"{entradaId}: se recuperaron {ids.Count}, no {asistente.Top}";
        if (ids.Distinct(StringComparer.Ordinal).Count() != ids.Count) yield return $"{entradaId}: fragmentos repetidos en el top-k";
        if (!ids.All(id => consulta.Contexto.Contains($"[Fuente: {id}]", StringComparison.Ordinal)))
            yield return $"{entradaId}: el contexto no lleva la Fuente de cada fragmento";
        // La garantía: lo que se muestra con cita, cita sólo lo recuperado; lo demás se rechaza.
        var muestraLaRespuesta = v.Mostrada == v.Texto && v.Veredicto is Veredicto.Aceptada or Veredicto.NoSe;
        var rechaza = v.Mostrada == ValidadorDeCitas.AvisoDeRechazo && v.Veredicto is Veredicto.RechazadaCitaNoRecuperada or Veredicto.RechazadaSinCita;
        if (!(muestraLaRespuesta || rechaza)) yield return $"{entradaId}: veredicto y texto mostrado no coinciden";
        if (v.Veredicto == Veredicto.Aceptada && !v.Citas.All(ids.Contains)) yield return $"{entradaId}: se aceptó una cita no recuperada";
        if (ValidadorDeCitas.Citas(v.Texto).Any(c => !ids.Contains(c)) && v.Veredicto != Veredicto.RechazadaCitaNoRecuperada)
            yield return $"{entradaId}: una cita no recuperada no se rechazó";
        if (llamada.Razonamiento != "None") yield return $"{entradaId}: razonamiento no apagado";
        if (llamada.NumCtx is null or <= 0) yield return $"{entradaId}: sin num_ctx explícito";
    }
}
