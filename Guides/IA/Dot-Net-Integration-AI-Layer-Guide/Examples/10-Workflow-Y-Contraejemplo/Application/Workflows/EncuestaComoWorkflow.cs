// Nuevo (ejemplo 10, §8.6). EL CONTRAEJEMPLO: la misma encuesta de tres pasos como workflow de MAF
// (Microsoft.Agents.AI.Workflows 1.26.0). Compila, corre, deja un checkpoint por superstep y se
// rehidrata en otra instancia: todo funciona. Y para este caso no compra nada que la función no tenga,
// porque no hay modelo que elija el camino ni una espera de días que sobrevivir (N6 C8; N4 P3, P11).
//
// Lo que hace falta saber para que compile y ande (N4 E02, V01):
//   - un executor con [MessageHandler] es una clase partial derivada de Executor y EXIGE el paquete
//     Microsoft.Agents.AI.Workflows.Generators: sin él, CS0534 (no implementa ConfigureProtocol);
//   - un handler que envía mensajes lo declara (Send = [...]); si no, falla en ejecución;
//   - una función se vuelve executor con BindAsExecutor (la doc dice BindExecutor: no compila en 1.26.0).
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Domain.Rules;
using Microsoft.Agents.AI.Workflows;

namespace MovilidadUrbana.Asistente.Application.Workflows;

/// <summary>Mensaje de arranque del workflow.</summary>
public sealed record IniciarEncuesta;

/// <summary>Lo que el workflow le pide a la persona por el RequestPort: un paso, el intento y los errores del anterior.</summary>
public sealed record PedidoDePaso(int Paso, int Intento, IReadOnlyList<string> Errores);

/// <summary>Lo que vuelve por el RequestPort. Respuesta null: la persona dejó la encuesta.</summary>
public sealed record Contestacion(RespuestaDePaso? Respuesta);

/// <summary>
/// Estado del workflow entre supersteps. Va en el estado del contexto (no en campos del executor), para
/// que quede en el checkpoint y vuelva al rehidratar: por eso es sólo datos serializables.
/// </summary>
public sealed record EstadoDeEncuesta(int Paso, List<int> Intentos, List<string> Rechazos, List<RespuestaDePaso> Aceptadas)
{
    public static EstadoDeEncuesta Inicial() => new(1, [.. new int[EncuestaRules.TotalDePasos]], [], []);

    /// <summary>El EncuestaModel del caso base, rearmado con las respuestas aceptadas y, si hay, la del intento en curso.</summary>
    public EncuestaModel Modelo(RespuestaDePaso? enCurso = null)
    {
        var modelo = new EncuestaModel();
        foreach (var r in Aceptadas) r.AplicarA(modelo);
        enCurso?.AplicarA(modelo);
        return modelo;
    }
}

/// <summary>La encuesta terminó (completa o abandonada): va al executor de cierre.</summary>
public sealed record EncuestaTerminada(bool Completa, EstadoDeEncuesta Estado);

/// <summary>El executor que lleva la encuesta: pide un paso, valida la contestación con ValidarPaso y decide.</summary>
public sealed partial class EncuestaExecutor(EncuestaService servicio) : Executor(EncuestaComoWorkflow.EncuestaId)
{
    private const string ClaveDeEstado = "encuesta";

    [MessageHandler(Send = [typeof(PedidoDePaso)])]
    private async ValueTask IniciarAsync(IniciarEncuesta inicio, IWorkflowContext contexto, CancellationToken cancellationToken)
    {
        await contexto.QueueStateUpdateAsync(ClaveDeEstado, EstadoDeEncuesta.Inicial(), cancellationToken);
        await contexto.SendMessageAsync(new PedidoDePaso(1, 1, []), EncuestaComoWorkflow.PersonaId, cancellationToken);
    }

    [MessageHandler(Send = [typeof(PedidoDePaso), typeof(EncuestaTerminada)])]
    private async ValueTask ContestarAsync(Contestacion contestacion, IWorkflowContext contexto, CancellationToken cancellationToken)
    {
        var estado = await contexto.ReadStateAsync<EstadoDeEncuesta>(ClaveDeEstado, cancellationToken)
                     ?? throw new InvalidOperationException("No hay estado de encuesta: el workflow no se inició.");
        var paso = estado.Paso;

        if (contestacion.Respuesta is not { } respuesta)
        {
            await contexto.SendMessageAsync(new EncuestaTerminada(false, estado), EncuestaComoWorkflow.CierreId, cancellationToken);
            return;
        }

        var intentos = new List<int>(estado.Intentos);
        intentos[paso - 1]++;
        var errores = servicio.ValidarPaso(paso, estado.Modelo(respuesta));

        if (errores.Count > 0)
        {
            var campos = errores.Keys.Order(StringComparer.Ordinal).ToList();
            var siguiente = estado with { Intentos = intentos, Rechazos = [.. estado.Rechazos, .. campos.Select(c => $"{paso}.{c}")] };
            await contexto.QueueStateUpdateAsync(ClaveDeEstado, siguiente, cancellationToken);
            await contexto.SendMessageAsync(new PedidoDePaso(paso, intentos[paso - 1] + 1, campos), EncuestaComoWorkflow.PersonaId, cancellationToken);
            return;
        }

        var aceptado = estado with { Intentos = intentos, Aceptadas = [.. estado.Aceptadas, respuesta] };
        if (paso == EncuestaRules.TotalDePasos)
        {
            await contexto.QueueStateUpdateAsync(ClaveDeEstado, aceptado, cancellationToken);
            await contexto.SendMessageAsync(new EncuestaTerminada(true, aceptado), EncuestaComoWorkflow.CierreId, cancellationToken);
            return;
        }

        await contexto.QueueStateUpdateAsync(ClaveDeEstado, aceptado with { Paso = paso + 1 }, cancellationToken);
        await contexto.SendMessageAsync(new PedidoDePaso(paso + 1, 1, []), EncuestaComoWorkflow.PersonaId, cancellationToken);
    }
}

public static class EncuestaComoWorkflow
{
    public const string EncuestaId = "Encuesta";
    public const string PersonaId = "Persona";
    public const string CierreId = "Cierre";

    /// <summary>
    /// El grafo: Encuesta ⇄ Persona (RequestPort: la pausa humana) y Encuesta → Cierre (una función hecha
    /// executor con BindAsExecutor). Cada llamada arma instancias nuevas: así se prueba la rehidratación.
    /// </summary>
    public static Workflow Construir(EncuestaService servicio)
    {
        var encuesta = new EncuestaExecutor(servicio);
        var persona = RequestPort.Create<PedidoDePaso, Contestacion>(PersonaId);
        Func<EncuestaTerminada, ResultadoDeEncuesta> cerrar = t => new ResultadoDeEncuesta(
            t.Completa, t.Estado.Paso, t.Estado.Intentos, t.Estado.Rechazos, ResultadoDeEncuesta.DatosDe(t.Estado.Modelo()));
        var cierre = cerrar.BindAsExecutor(CierreId);

        return new WorkflowBuilder(encuesta)
            .AddEdge(encuesta, persona)
            .AddEdge(persona, encuesta)
            .AddEdge(encuesta, cierre)
            .WithOutputFrom(cierre)
            .WithName("encuesta-de-tres-pasos")
            .Build();
    }
}
