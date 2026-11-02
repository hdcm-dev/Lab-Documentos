// Nuevo (plan §E.2, fila 05). Corre el plan de consultas contra un IChatClient (el real o el del
// modo falso, los dos ya envueltos por la capa única), escribe el registro R-LLM y comprueba los
// invariantes de código: lo que la aplicación controla, no lo que hace el servidor.
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Application;

public sealed record ResultadoDeLaCorrida(
    IReadOnlyList<RegistroDeLlamada> Llamadas,
    IReadOnlyDictionary<string, VeredictoDelPar> Veredictos,
    IReadOnlyList<string> InvariantesQueFallan);

public sealed class CorridaDelPrefijo(
    IChatClient cliente,
    EscritorDeRegistro registro,
    Func<ChatOptions, ChatOptions?> opcionesEfectivas,
    ConfiguracionDelModelo configuracion,
    string ejemplo,
    Func<CancellationToken, Task<string?>>? estadoCacheAntes = null)
{
    public async Task<ResultadoDeLaCorrida> CorrerAsync(
        IReadOnlyList<Pregunta> preguntas, string prefijo, IReadOnlyList<ConsultaPlanificada> plan,
        Plazo plazo, CancellationToken cancellationToken = default)
    {
        var hechas = new List<RegistroDeLlamada>();
        var errores = new Dictionary<string, string>();
        var enviados = new List<(ConsultaPlanificada Consulta, IReadOnlyList<ChatMessage> Mensajes)>();
        foreach (var consulta in plan)
        {
            if (plazo.Vencido) break;
            // El prefijo va primero (mensaje de sistema) y la pregunta AL FINAL: lo que cambia entre
            // consultas queda después de lo que se quiere reutilizar (N3b §3.4).
            List<ChatMessage> mensajes = [new(ChatRole.System, consulta.Sistema), new(ChatRole.User, consulta.Pregunta)];
            var opciones = new ChatOptions();
            var forzado = estadoCacheAntes is null ? null : await estadoCacheAntes(cancellationToken);
            var reloj = EscritorDeRegistro.Cronometro();
            ChatResponse respuesta;
            try
            {
                respuesta = await cliente.GetResponseAsync(mensajes, opciones, cancellationToken);
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
            {
                errores[consulta.EntradaId] = $"error del servidor ({e.GetType().Name})";
                continue;
            }
            enviados.Add((consulta, mensajes));
            var llamada = registro.Llamada(consulta.EntradaId, consulta.LlamadaN, mensajes, opcionesEfectivas(opciones), respuesta,
                reloj.Elapsed, consulta.Paso, contexto: consulta.Sistema, estadoCacheForzado: forzado);
            registro.Escribir(llamada);
            hechas.Add(llamada);
        }

        var veredictos = new Dictionary<string, VeredictoDelPar>();
        foreach (var p in preguntas)
        {
            var v = errores.TryGetValue(p.Id, out var error)
                ? new VeredictoDelPar(ResultadoDeEntrada.Excluida, error, null)
                : OraculoDelPar.Evaluar(hechas.Where(l => l.EntradaId == p.Id).ToList());
            veredictos[p.Id] = v;
            registro.Escribir(new RegistroDeEntrada
            {
                Ejemplo = ejemplo, EntradaId = p.Id, Oraculo = OraculoDelPar.Nombre, Resultado = v.Resultado, Motivo = v.Motivo
            });
        }

        return new(hechas, veredictos, Invariantes(prefijo, plan, enviados, hechas, plazo.Vencido || errores.Count > 0));
    }

    /// <summary>Invariantes de código de --verificar. Vacía = todos se cumplen.</summary>
    private List<string> Invariantes(
        string prefijo, IReadOnlyList<ConsultaPlanificada> plan,
        List<(ConsultaPlanificada Consulta, IReadOnlyList<ChatMessage> Mensajes)> enviados,
        List<RegistroDeLlamada> hechas, bool cortada)
    {
        var fallan = new List<string>();
        void Exigir(bool condicion, string invariante) { if (!condicion) fallan.Add(invariante); }

        Exigir(cortada || enviados.Count == plan.Count, $"se hicieron las {plan.Count} llamadas del plan");
        Exigir(enviados.Select(e => e.Consulta).SequenceEqual(plan.Take(enviados.Count)), "el orden de las llamadas es el del plan");
        Exigir(enviados.All(e => e.Mensajes.Count == 2 && e.Mensajes[0].Role == ChatRole.System && e.Mensajes[^1].Role == ChatRole.User
                                 && e.Mensajes[^1].Text == e.Consulta.Pregunta), "prefijo primero y pregunta al final");

        var conPrefijo = enviados.Where(e => e.Consulta.Paso is Pasos.ConPrefijo or Pasos.Calentamiento).Select(e => e.Mensajes[0].Text).ToList();
        Exigir(conPrefijo.All(t => string.Equals(t, prefijo, StringComparison.Ordinal)), "con-prefijo: prefijo idéntico byte a byte");

        var sinPrefijo = enviados.Where(e => e.Consulta.Paso == Pasos.SinPrefijo).Select(e => e.Mensajes[0].Text).ToList();
        Exigir(sinPrefijo.All(t => t.EndsWith(prefijo, StringComparison.Ordinal) && t.Length > prefijo.Length && t[0] != prefijo[0]),
            "sin-prefijo: el nonce va al principio y el resto es el prefijo");
        Exigir(sinPrefijo.Distinct(StringComparer.Ordinal).Count() == sinPrefijo.Count, "sin-prefijo: un nonce distinto por consulta");

        var vuelta = enviados.Where(e => e.Consulta.Paso == Pasos.VueltaAFrio).Select(e => e.Mensajes[0].Text).ToList();
        Exigir(vuelta.All(t => t.Length == prefijo.Length && t[0] != prefijo[0] && t.AsSpan(1).SequenceEqual(prefijo.AsSpan(1))),
            "vuelta a frío: un solo carácter cambiado, el primero");

        Exigir(hechas.All(l => l.Razonamiento == "None"), "razonamiento apagado en toda llamada");
        Exigir(hechas.All(l => l.NumCtx == configuracion.NumCtx && l.NumCtx > 0), $"num_ctx {configuracion.NumCtx} explícito en toda llamada");
        Exigir(hechas.All(l => l.Ejemplo == ejemplo), "registro con el nombre del ejemplo");
        return fallan;
    }
}
