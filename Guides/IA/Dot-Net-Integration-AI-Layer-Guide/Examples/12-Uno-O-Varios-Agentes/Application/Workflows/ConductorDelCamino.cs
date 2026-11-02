// Nuevo (ejemplo 12): corre el workflow con una solicitud y junta lo que hace falta para comprobar el
// camino: el orden en que se invocaron los executors (ExecutorInvokedEvent) y la salida.
using Microsoft.Agents.AI.Workflows;

namespace MovilidadUrbana.Asistente.Application.Workflows;

public sealed record CorridaDelCamino(ResumenVerificado? Salida, IReadOnlyList<string> Camino);

public static class ConductorDelCamino
{
    private static readonly HashSet<string> Nodos =
        [CaminoDelResumen.DatosId, CaminoDelResumen.RedactorId, CaminoDelResumen.VerificadorId, CaminoDelResumen.CorrectorId];

    public static async Task<CorridaDelCamino> CorrerAsync(Workflow workflow, SolicitudDeResumen solicitud, CancellationToken cancellationToken = default)
    {
        var camino = new List<string>();
        ResumenVerificado? salida = null;
        await using var corrida = await InProcessExecution.RunStreamingAsync(workflow, solicitud, solicitud.EntradaId, cancellationToken);
        await foreach (var evento in corrida.WatchStreamAsync(cancellationToken))
        {
            switch (evento)
            {
                case ExecutorInvokedEvent invocado when Nodos.Contains(invocado.ExecutorId):
                    camino.Add(invocado.ExecutorId);
                    break;
                case WorkflowOutputEvent o when o.Is<ResumenVerificado>(out var r):
                    salida = r;
                    break;
                case WorkflowErrorEvent error:
                    throw new InvalidOperationException("El workflow falló.", error.Exception);
                case ExecutorFailedEvent falla:
                    throw new InvalidOperationException($"El executor {falla.ExecutorId} falló.", falla.Data);
            }
        }
        return new CorridaDelCamino(salida, camino);
    }

    /// <summary>El camino que el código fija para una variante, según cuántas vueltas hubo.</summary>
    public static IReadOnlyList<string> CaminoEsperado(Variante variante, int vueltas)
    {
        List<string> camino = [CaminoDelResumen.DatosId, CaminoDelResumen.RedactorId, CaminoDelResumen.VerificadorId];
        for (var i = 0; i < vueltas; i++)
        {
            camino.Add(variante == Variante.UnAgente ? CaminoDelResumen.RedactorId : CaminoDelResumen.CorrectorId);
            camino.Add(CaminoDelResumen.VerificadorId);
        }
        return camino;
    }
}
