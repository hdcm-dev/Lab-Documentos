using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Shell.Bridge;

namespace FrameworkConcept.Shell.Red;

/// <summary>
/// Borrador §7 — El cascarón evalúa la política de acciones (PoliticaDeAcciones) con la red y la sesión del momento.
/// El despachador del puente la consulta antes de ejecutar un método que sea una acción; el contenido la pide con
/// actions.policy y recibe actions.changed cuando cambia.
///
/// La regla vive en la función pura <see cref="EvaluadorDeAcciones.Evaluar"/> (Core, probada en Core.Tests/Red); esta
/// clase sólo le pasa el estado del momento y avisa cuando la evaluación de alguna acción cambió.
/// </summary>
public sealed class GuardiaDeAcciones : IGuardiaDeAcciones
{
    private readonly Lock _cerrojo = new();
    private IReadOnlyList<EvaluacionDeAccion> _ultima = [];

    public GuardiaDeAcciones(IMonitorDeRed red, ISesionDelUsuario sesion, EmisorDeEventos emisor)
    {
        Red = red;
        Sesion = sesion;
        Emisor = emisor;
        _ultima = EvaluarTodas();

        // Cambia la red o la sesión: se reevalúa todo y, si algo cambió, se avisa (al contenido, por actions.changed).
        red.Cambio += _ => Avisar();
        sesion.Cambio += _ => Avisar();
    }

    public IMonitorDeRed Red { get; }
    public ISesionDelUsuario Sesion { get; }
    public EmisorDeEventos Emisor { get; }

    public event Action<IReadOnlyList<EvaluacionDeAccion>>? Cambio;

    public IReadOnlyList<EvaluacionDeAccion> EvaluarTodas()
    {
        // Una sola lectura del estado para todas: así la lista es coherente consigo misma.
        var red = Red.Actual.Estado;
        var sesion = Sesion.Actual;
        return [.. PoliticaDeAcciones.Acciones.Select(a => EvaluadorDeAcciones.Evaluar(a, red, sesion))];
    }

    public EvaluacionDeAccion Evaluar(string accion) =>
        EvaluadorDeAcciones.Evaluar(
            PoliticaDeAcciones.Buscar(accion) ?? throw new KeyNotFoundException($"La acción «{accion}» no está en PoliticaDeAcciones."),
            Red.Actual.Estado, Sesion.Actual);

    private void Avisar()
    {
        var ahora = EvaluarTodas();
        lock (_cerrojo)
        {
            if (ahora.SequenceEqual(_ultima)) return;   // records: compara por valor
            _ultima = ahora;
        }
        Bitacora.Log("acciones: " + string.Join(", ", ahora.Select(e => $"{e.Accion}={(e.Permitida ? e.QuedaPendiente ? "pendiente" : "sí" : "no")}")));
        Cambio?.Invoke(ahora);
        Emisor.Emitir("actions.changed", new { acciones = ahora });
    }
}
