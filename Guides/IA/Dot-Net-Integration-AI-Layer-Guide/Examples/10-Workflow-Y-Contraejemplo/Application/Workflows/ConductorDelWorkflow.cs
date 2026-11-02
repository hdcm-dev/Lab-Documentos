// Nuevo (ejemplo 10). Lo que en la Web haría la pantalla: mira los eventos del workflow, contesta cada
// RequestInfoEvent con lo que da la persona y cuenta supersteps y checkpoints. Con cortarEnPaso, deja
// el pedido de ese paso SIN contestar y corta: es la «pausa» que después se reanuda desde el checkpoint.
using MovilidadUrbana.Asistente.Application.Encuestas;
using Microsoft.Agents.AI.Workflows;

namespace MovilidadUrbana.Asistente.Application.Workflows;

public sealed record CorridaDelWorkflow(
    ResultadoDeEncuesta? Resultado, int Supersteps, int Checkpoints, CheckpointInfo? CheckpointDeCorte,
    int Pedidos, IReadOnlyList<string> Traza);

public static class ConductorDelWorkflow
{
    public static async Task<CorridaDelWorkflow> ConducirAsync(
        StreamingRun corrida, Persona persona, int? cortarEnPaso = null, CancellationToken cancellationToken = default)
    {
        int supersteps = 0, checkpoints = 0, pedidos = 0;
        CheckpointInfo? ultimo = null;
        var cortando = false;
        ResultadoDeEncuesta? resultado = null;
        var traza = new List<string>();

        await foreach (var evento in corrida.WatchStreamAsync(cancellationToken))
        {
            switch (evento)
            {
                case RequestInfoEvent pedido when pedido.Request.TryGetDataAs<PedidoDePaso>(out var p):
                    pedidos++;
                    if (cortarEnPaso == p.Paso && p.Intento == 1 && !cortando)
                    {
                        cortando = true;
                        traza.Add($"pedido paso {p.Paso}: se deja sin contestar (pausa)");
                        break;
                    }
                    var respuesta = persona.Contestar(p.Paso);
                    traza.Add(respuesta is null
                        ? $"pedido paso {p.Paso} (intento {p.Intento}): la persona deja la encuesta"
                        : $"pedido paso {p.Paso} (intento {p.Intento}{(p.Errores.Count > 0 ? ", errores: " + string.Join(",", p.Errores) : "")}): contesta");
                    await corrida.SendResponseAsync(pedido.Request.CreateResponse(new Contestacion(respuesta)));
                    break;

                case SuperStepCompletedEvent paso:
                    supersteps++;
                    if (paso.CompletionInfo?.Checkpoint is { } cp) { checkpoints++; ultimo = cp; }
                    if (cortando && paso.CompletionInfo?.HasPendingRequests == true)
                    {
                        traza.Add($"superstep {supersteps}: checkpoint con el pedido pendiente; se corta");
                        return new CorridaDelWorkflow(null, supersteps, checkpoints, ultimo, pedidos, traza);
                    }
                    break;

                case WorkflowOutputEvent salida when salida.Is<ResultadoDeEncuesta>(out var r):
                    resultado = r;
                    break;

                case WorkflowErrorEvent error:
                    throw new InvalidOperationException("El workflow falló.", error.Exception);

                case ExecutorFailedEvent falla:
                    throw new InvalidOperationException("Un executor falló.", falla.Data);
            }
        }
        return new CorridaDelWorkflow(resultado, supersteps, checkpoints, cortando ? ultimo : null, pedidos, traza);
    }
}
