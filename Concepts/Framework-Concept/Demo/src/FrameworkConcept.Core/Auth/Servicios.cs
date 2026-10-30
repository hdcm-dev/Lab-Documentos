namespace FrameworkConcept.Core.Auth;

/// <summary>
/// La sesión del usuario en el cascarón (P4). La implementa Shell/Auth/SesionDelUsuario.cs. La usan el puente
/// (auth.*), el sincronizador (qué filas salen y con qué token), el DelegatingHandler (Authorization: Bearer) y la
/// guardia de acciones.
/// </summary>
public interface ISesionDelUsuario
{
    SesionInformada Actual { get; }

    /// <summary>Cambió la sesión. El puente emite auth.changed; la guardia reevalúa las acciones.</summary>
    event Action<SesionInformada>? Cambio;

    /// <summary>Arranque en frío: SinCuenta, Bloqueada (desbloqueo activado) o Vencida (refresh sólo en memoria).</summary>
    Task IniciarAsync();

    /// <summary>El dueño para una fila NUEVA del outbox: el de la identidad presente. En modo Dispositivo, el id de instalación.</summary>
    Task<string> ObtenerDuenoAsync();

    /// <summary>
    /// P4 — El dueño cuyas filas ve la persona (conteos de outbox.status y outbox.changed). Las de otro dueño son
    /// invisibles. null = todas (modo Dispositivo, como en A3).
    /// </summary>
    string? DuenoVisible { get; }

    /// <summary>P4 — ¿Esta fila puede salir ahora? Sólo con la identidad de su dueño y sesión válida.</summary>
    bool PuedeEnviarFilaDe(string dueno);

    /// <summary>Access token vigente (renovado si hacía falta), o null si no hay sesión válida. Nunca el refresh.</summary>
    Task<string?> ObtenerAccessTokenAsync(CancellationToken ct);

    /// <summary>
    /// Corrector E03 — El access token para enviar UNA fila de <paramref name="dueno"/>: se comprueba el dueño y se
    /// obtiene el token bajo el mismo cerrojo con que cambia la identidad (ingreso, cierre). Así una fila de A nunca
    /// sale con el token de B. null = esa fila no puede salir ahora (otro dueño, sin sesión válida o sin token).
    /// </summary>
    Task<string?> ObtenerAccessTokenParaAsync(string dueno, CancellationToken ct);

    /// <summary>El servidor contestó 401 a <paramref name="tokenRechazado"/>: si sigue siendo el token de la sesión, se renueva una vez; si el emisor rechaza, Revocada.</summary>
    Task InformarNoAutorizadoAsync(string tokenRechazado, CancellationToken ct);

    /// <summary>auth.signIn — OIDC code + PKCE en el navegador del sistema.</summary>
    Task<SesionInformada> IngresarAsync(CancellationToken ct);

    /// <summary>
    /// auth.signInWithPassword — login tradicional (first-party): canjea usuario y clave en el emisor y los descarta.
    /// La clave nunca se registra ni se guarda.
    /// </summary>
    Task<SesionInformada> IngresarConClaveAsync(string usuario, string clave, CancellationToken ct);

    /// <summary>auth.lock — bloquea ya (Activa → Bloqueada), igual que al volver de segundo plano. Exige el desbloqueo activado.</summary>
    Task<SesionInformada> BloquearAsync(CancellationToken ct);

    /// <summary>auth.enableUnlock — sella el refresh token con una clave del Keystore atada a la autenticación del dispositivo.</summary>
    Task<SesionInformada> ActivarDesbloqueoAsync(CancellationToken ct);

    /// <summary>auth.unlock — BiometricPrompt (huella, rostro, PIN o patrón), abre el refresh y renueva.</summary>
    Task<SesionInformada> DesbloquearAsync(CancellationToken ct);

    /// <summary>auth.signOut — borra tokens y clave; devuelve cuántas filas pendientes quedaron con su dueño.</summary>
    Task<(SesionInformada Sesion, int PendientesConservados)> CerrarAsync(CancellationToken ct);

    /// <summary>La app pasó a segundo plano (Window.Stopped): se anota la hora.</summary>
    void AlPasarASegundoPlano();

    /// <summary>La app volvió (Window.Resumed): si pasaron más de ParametrosDeSesion.BloqueoTrasSegundoPlano, Bloqueada.</summary>
    void AlVolverAlFrente();
}
