using System.Text.Json;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Shell.Sync;

namespace FrameworkConcept.Shell.Red;

/// <summary>
/// Los métodos del puente del incremento de red (DISENO §6): network.status, actions.policy, actions.check y
/// outbox.sendNow. El despachador (Bridge/DespachadorDelPuente.cs) le pasa todo método de esas filas: así el
/// constructor de red trabaja en este archivo y no en el despachador.
/// </summary>
public sealed class AtencionDeRed(IMonitorDeRed red, IGuardiaDeAcciones guardia, SincronizadorDelOutbox sincronizador)
{
    /// <summary>Los métodos que atiende esta clase. El despachador los consulta con <see cref="Atiende"/>.</summary>
    public static readonly IReadOnlySet<string> Metodos = new HashSet<string>(StringComparer.Ordinal)
    {
        "network.status", "actions.policy", "actions.check", "outbox.sendNow"
    };

    public static bool Atiende(string metodo) => Metodos.Contains(metodo);

    public SincronizadorDelOutbox Sincronizador { get; } = sincronizador;

    public Task<object> ResolverAsync(string metodo, JsonElement p, CancellationToken ct) => metodo switch
    {
        "network.status" => Task.FromResult<object>(new { conectado = red.Actual.Conectado, estado = red.Actual.Estado, motivo = red.Actual.Motivo }),
        "actions.policy" => Task.FromResult<object>(new { acciones = guardia.EvaluarTodas() }),
        "actions.check" => Task.FromResult<object>(new { evaluacion = guardia.Evaluar(p.GetProperty("accion").GetString() ?? "") }),
        "outbox.sendNow" => EnviarAhoraAsync(ct),
        _ => throw new InvalidOperationException($"AtencionDeRed no atiende «{metodo}».")
    };

    /// <summary>
    /// «Enviar ahora» (SoloEnLinea): el despachador ya la evaluó con la guardia antes de llegar acá. Fuerza una pasada
    /// del sincronizador, sin esperar el backoff de las filas, y devuelve lo que salió y lo que queda.
    /// </summary>
    private async Task<object> EnviarAhoraAsync(CancellationToken ct)
    {
        var resultado = await Sincronizador.PasadaAsync(ct, ignorarBackoff: true);
        return new { enviadas = resultado.Enviadas, pendientes = resultado.Pendientes, rechazadas = resultado.Rechazadas };
    }
}
