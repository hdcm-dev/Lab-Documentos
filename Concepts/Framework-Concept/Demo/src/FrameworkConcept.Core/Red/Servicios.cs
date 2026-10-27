namespace FrameworkConcept.Core.Red;

/// <summary>
/// El estado de red del cascarón (borrador §6). Lo implementa Shell/Red/MonitorDeRed.cs (no confundir con Core.Update.IEstadoDeRed, que sólo dice si hay red para la política de soporte de P2). Lo leen el
/// sincronizador (sólo envía con servidor), la guardia de acciones y el puente (network.status, network.changed).
/// </summary>
public interface IMonitorDeRed
{
    EstadoDeRedCombinado Actual { get; }

    /// <summary>Cambió el estado combinado. Se dispara fuera del hilo principal.</summary>
    event Action<EstadoDeRedCombinado>? Cambio;

    /// <summary>Se suscribe a Connectivity y arranca el chequeo de salud. Una sola vez por proceso.</summary>
    void Iniciar();

    /// <summary>Volvió la app al frente, o alguien quiere saber ya: chequeo de salud (respetando la caché).</summary>
    void Reevaluar();

    /// <summary>Lo informa el DelegatingHandler: una llamada del framework falló por red o 5xx.</summary>
    void InformarLlamadaFallida(string detalle);

    /// <summary>Lo informa el DelegatingHandler: una llamada al servidor respondió (cualquier código no 5xx).</summary>
    void InformarLlamadaExitosa();
}

/// <summary>
/// La guardia de acciones (borrador §7): evalúa la política con la red y la sesión del momento. La implementa
/// Shell/Red/GuardiaDeAcciones.cs; la usan el despachador del puente y el sincronizador.
/// </summary>
public interface IGuardiaDeAcciones
{
    /// <summary>Todas las acciones de <see cref="PoliticaDeAcciones"/>, evaluadas ahora.</summary>
    IReadOnlyList<EvaluacionDeAccion> EvaluarTodas();

    /// <summary>Una acción por id. Lanza KeyNotFoundException si no está declarada.</summary>
    EvaluacionDeAccion Evaluar(string accion);

    /// <summary>Cambió alguna evaluación (por la red o por la sesión). El puente emite actions.changed.</summary>
    event Action<IReadOnlyList<EvaluacionDeAccion>>? Cambio;
}
