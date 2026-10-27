using System.Text.Json;
using FrameworkConcept.Content.Bridge;
using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Contracts;

namespace FrameworkConcept.Content.Auth;

/// <summary>
/// Lo que el contenido sabe de la sesión (P4): modo, estado, usuario y si se ofrece el desbloqueo local. Viene de
/// auth.status y de auth.changed; nunca incluye tokens (P3). La usa PuertaDeSesion.razor para decidir si muestra la
/// página, el ingreso con cuenta o el desbloqueo.
///
/// Con un cascarón que no trae «auth.session» (capacidad opcional), la sesión queda en null y la puerta deja pasar:
/// es la sesión por dispositivo de A1–A3. El contenido no decide nada de la sesión: pide (auth.*) y muestra.
/// </summary>
public sealed class SesionDelContenido : IDisposable
{
    private readonly BridgeSession _sesion;
    private Task? _inicio;

    public SesionDelContenido(BridgeSession sesion, BridgeClient puente)
    {
        _sesion = sesion;
        Puente = puente;
        _sesion.EventoRecibido += AlRecibirEvento;
    }

    public BridgeClient Puente { get; }

    /// <summary>null mientras no se sepa, o si el cascarón no trae «auth.session».</summary>
    public SesionInformada? Sesion { get; private set; }

    /// <summary>Lo que salió mal en la última acción de sesión (cancelada, fallida), para mostrarlo.</summary>
    public string? Error { get; private set; }

    /// <summary>Después de cerrar sesión: cuántas filas pendientes quedaron con su dueño (P4).</summary>
    public int? PendientesConservados { get; private set; }

    /// <summary>La persona dijo «Ahora no» a la seguridad del teléfono: no se vuelve a ofrecer en este arranque.</summary>
    public bool DesbloqueoRechazado { get; private set; }

    /// <summary>Hay una llamada auth.* en curso (los botones se deshabilitan).</summary>
    public bool Ocupada { get; private set; }

    public event Action? Cambio;

    /// <summary>Pide auth.status una vez por arranque del contenido; después todo llega por auth.changed.</summary>
    public Task IniciarAsync() => _inicio ??= LeerAsync();

    private async Task LeerAsync()
    {
        await _sesion.IniciarAsync();
        if (_sesion.Estado != EstadoDelPuente.Listo || !_sesion.Capacidad("auth.session").Disponible) return;
        try
        {
            Sesion = await Puente.AuthStatusAsync();
        }
        catch (BridgeException excepcion)
        {
            Console.WriteLine("sesión: no se pudo leer el estado: " + excepcion.Message);
        }
        Avisar();
    }

    public Task IngresarAsync() => EjecutarAsync(Puente.AuthSignInAsync);

    public Task DesbloquearAsync() => EjecutarAsync(Puente.AuthUnlockAsync);

    public Task ActivarDesbloqueoAsync() => EjecutarAsync(Puente.AuthEnableUnlockAsync);

    public void RechazarDesbloqueo()
    {
        DesbloqueoRechazado = true;
        Avisar();
    }

    public Task CerrarAsync() => EjecutarAsync(async () =>
    {
        var (sesion, pendientes) = await Puente.AuthSignOutAsync();
        PendientesConservados = pendientes;
        return sesion;
    });

    private async Task EjecutarAsync(Func<Task<SesionInformada>> accion)
    {
        Error = null;
        Ocupada = true;
        Avisar();
        try
        {
            Sesion = await accion();
        }
        catch (BridgeException excepcion)
        {
            // AutenticacionCancelada (-32006), AutenticacionFallida (-32007) o AccionBloqueada (-32005): el mensaje
            // del cascarón es para la persona.
            Error = excepcion.Message;
        }
        finally
        {
            Ocupada = false;
        }
        Avisar();
    }

    private void AlRecibirEvento(string metodo, JsonElement parametros)
    {
        if (metodo != "auth.changed") return;
        Sesion = parametros.GetProperty("sesion").Deserialize<SesionInformada>(JsonDelContrato.Opciones);
        if (Sesion?.Estado == EstadoDeSesion.Activa) PendientesConservados = null;
        // P4 — El dueño visible cambió (otra cuenta, o ninguna): el conteo «Por enviar» se vuelve a pedir.
        _ = _sesion.RefrescarAsync();
        Avisar();
    }

    private void Avisar() => Cambio?.Invoke();

    public void Dispose() => _sesion.EventoRecibido -= AlRecibirEvento;
}
