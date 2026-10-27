using System.Text.Json;
using FrameworkConcept.Content.Bridge;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Red;

namespace FrameworkConcept.Content.Red;

/// <summary>
/// Lo que el contenido sabe de la red y de las acciones (borrador §6–§7): el estado combinado que informa el cascarón
/// (network.status, network.changed) y la evaluación de cada acción (actions.policy, actions.changed). Las pantallas lo
/// leen para el aviso permanente (AvisoDeRed.razor) y para deshabilitar botones con su motivo (BotonDeAccion.razor).
/// El contenido NO decide: muestra lo que el cascarón evaluó (DISENO §4.1).
/// </summary>
public sealed class EstadoDeRedDelContenido : IDisposable
{
    private readonly BridgeSession _sesion;
    private Task? _inicio;

    public EstadoDeRedDelContenido(BridgeSession sesion, BridgeClient puente)
    {
        _sesion = sesion;
        Puente = puente;
        _sesion.EventoRecibido += AlRecibirEvento;
    }

    public BridgeClient Puente { get; }

    /// <summary>null mientras no se sepa, o si el cascarón no trae network.status.</summary>
    public EstadoDeRedInformado? Red { get; private set; }

    /// <summary>
    /// El estado combinado. Con un cascarón que sólo informa «conectado» (puente 1.0), se deduce de ahí como en A3.
    /// null si no se sabe.
    /// </summary>
    public EstadoDeRed? Estado => Red is null ? null : Red.Estado ?? (Red.Conectado ? EstadoDeRed.EnLinea : EstadoDeRed.SinRed);

    /// <summary>Vacía mientras no se pidió actions.policy.</summary>
    public IReadOnlyList<EvaluacionDeAccion> Acciones { get; private set; } = [];

    public event Action? Cambio;

    /// <summary>La evaluación de una acción, o null si todavía no llegó (el botón queda habilitado: el cascarón igual la evalúa).</summary>
    public EvaluacionDeAccion? Evaluacion(string accion) =>
        Acciones.FirstOrDefault(a => string.Equals(a.Accion, accion, StringComparison.Ordinal));

    /// <summary>
    /// Pide network.status (si la capacidad está) y actions.policy, una sola vez por arranque del contenido. Después
    /// todo llega por eventos. Lo llaman los componentes que lo muestran; las llamadas siguientes devuelven la misma tarea.
    /// </summary>
    public Task IniciarAsync() => _inicio ??= LeerAsync();

    private async Task LeerAsync()
    {
        await _sesion.IniciarAsync();
        if (_sesion.Estado != EstadoDelPuente.Listo) return;
        try
        {
            // P12 — network.status es opcional: sin la capacidad, no se llama y el aviso no se muestra.
            if (_sesion.Capacidad("network.status").Disponible) Red = await Puente.NetworkStatusCombinadoAsync();
            Acciones = await Puente.ActionsPolicyAsync();
        }
        catch (BridgeException excepcion)
        {
            // Sin política, los botones quedan habilitados; el cascarón igual rechaza lo no permitido (doble cerrojo).
            Console.WriteLine("red: no se pudo leer el estado inicial: " + excepcion.Message);
        }
        Avisar();
    }

    private void AlRecibirEvento(string metodo, JsonElement parametros)
    {
        switch (metodo)
        {
            case "network.changed":
                Red = parametros.Deserialize<EstadoDeRedInformado>(JsonDelContrato.Opciones);
                break;
            case "actions.changed":
                Acciones = parametros.GetProperty("acciones").Deserialize<List<EvaluacionDeAccion>>(JsonDelContrato.Opciones) ?? [];
                break;
            default:
                return;
        }
        Avisar();
    }

    private void Avisar() => Cambio?.Invoke();

    public void Dispose() => _sesion.EventoRecibido -= AlRecibirEvento;
}
