namespace FrameworkConcept.Core.Auth;

// P4 (folio 035) — Entrada local y sesión con el servidor, separadas. Entrar es el desbloqueo local; la sesión
// con el servidor es el token. El refresh token nunca cruza el puente (P3): al contenido sólo le llega
// SesionInformada. Diseño: DISENO-RED-Y-AUTENTICACION.md §3.

/// <summary>De dónde sale la identidad del dueño de las filas del outbox.</summary>
public enum ModoDeSesion
{
    /// <summary>Sin emisor configurado (-p:FcOidcAutoridad vacío): la sesión por dispositivo del caso base (A1–A3).</summary>
    Dispositivo,

    /// <summary>Con emisor OIDC (D-07 = I1; en el ensayo, Keycloak en contenedor).</summary>
    Oidc
}

/// <summary>La máquina de estados de la sesión (DISENO §3.2). En el puente viaja en camelCase.</summary>
public enum EstadoDeSesion
{
    /// <summary>Nunca se ingresó en este teléfono, o se cerró la sesión. Sólo se ofrece «Ingresar».</summary>
    SinCuenta,

    /// <summary>El navegador del sistema está abierto con el emisor (RFC 8252).</summary>
    Ingresando,

    /// <summary>Hay identidad y entrada abierta. El token puede estar vencido y renovarse en segundo plano.</summary>
    Activa,

    /// <summary>Hay identidad y el refresh token está sellado en el Keystore: falta el desbloqueo local.</summary>
    Bloqueada,

    /// <summary>La sesión con el servidor venció en este proceso, después de una prueba local (ingreso o desbloqueo). Se trabaja local; para sincronizar, reingreso. En frío no se llega acá (corrector E03).</summary>
    Vencida,

    /// <summary>El servidor rechazó la renovación antes del vencimiento, o la clave del Keystore quedó invalidada. Reingreso.</summary>
    Revocada
}

/// <summary>
/// Cómo se ingresa con la cuenta (-p:FcModoDeIngreso, login tradicional 2026-10-30). Si falta en SesionInformada
/// (cascarón 1.1), el contenido lo toma como Navegador: ese cascarón no tiene auth.signInWithPassword.
/// </summary>
public enum ModoDeIngreso
{
    /// <summary>Formulario propio de la app (usuario y clave); el cascarón canjea en el emisor (first-party).</summary>
    Formulario,

    /// <summary>OIDC code + PKCE en el navegador del sistema (Custom Tab, RFC 8252).</summary>
    Navegador
}

/// <summary>D-03 = T1: el desbloqueo con la seguridad del teléfono se ofrece sólo con API ≥ 30 y bloqueo de pantalla.</summary>
public enum DesbloqueoLocal
{
    /// <summary>API &lt; 30, sin bloqueo de pantalla, o modo Dispositivo. No se ofrece.</summary>
    NoDisponible,

    /// <summary>Se puede activar (después de ingresar con la cuenta).</summary>
    Disponible,

    /// <summary>El refresh token está guardado bajo una clave del Keystore atada a la autenticación del dispositivo.</summary>
    Activado
}

/// <summary>
/// Lo que el contenido sabe de la sesión: forma «SesionInformada» del puente (auth.status, auth.changed…).
/// Nunca lleva tokens (P3).
/// </summary>
/// <param name="Modo">Dispositivo u Oidc.</param>
/// <param name="Estado">Estado de la máquina.</param>
/// <param name="Usuario">preferred_username del id_token, si hay identidad.</param>
/// <param name="DesbloqueoLocal">Si se ofrece, o si está activado.</param>
/// <param name="Motivo">Texto para la persona (por qué está bloqueada, vencida o revocada; por qué no se ofrece el desbloqueo).</param>
/// <param name="ModoDeIngreso">Puente 1.2: formulario propio o navegador. null en modo Dispositivo o con un cascarón 1.1.</param>
public sealed record SesionInformada(
    ModoDeSesion Modo,
    EstadoDeSesion Estado,
    string? Usuario,
    DesbloqueoLocal DesbloqueoLocal,
    string? Motivo = null,
    ModoDeIngreso? ModoDeIngreso = null)
{
    /// <summary>La sesión por dispositivo: siempre activa, sin usuario, sin desbloqueo local.</summary>
    public static SesionInformada PorDispositivo { get; } =
        new(ModoDeSesion.Dispositivo, EstadoDeSesion.Activa, null, DesbloqueoLocal.NoDisponible);
}

/// <summary>Lo que hace cambiar el estado de la sesión (las flechas de DISENO §3.2).</summary>
public enum EventoDeSesion
{
    IngresoIniciado,
    IngresoExitoso,

    /// <summary>La persona cerró el navegador, o el emisor devolvió error: vuelve al estado previo al ingreso.</summary>
    IngresoFallido,

    /// <summary>Arranque en frío con desbloqueo activado, o más de X minutos en segundo plano.</summary>
    Bloquear,

    DesbloqueoExitoso,

    /// <summary>La persona canceló el BiometricPrompt: sigue bloqueada.</summary>
    DesbloqueoCancelado,

    /// <summary>KeyPermanentlyInvalidatedException (huella nueva, bloqueo quitado): borra la clave y va a Revocada.</summary>
    ClaveInvalidada,

    /// <summary>El emisor contestó invalid_grant a la renovación antes del vencimiento conocido.</summary>
    RenovacionRechazada,

    /// <summary>Pasó refresh_expires_in, o el proceso arrancó sin refresh token recuperable.</summary>
    SesionVencida,

    /// <summary>La persona cerró la sesión: tokens y clave borrados; las filas pendientes quedan con su dueño.</summary>
    Cierre
}

/// <summary>La máquina de la sesión como función pura (DISENO §3.2). Se prueba en Core.Tests/Auth.</summary>
public static class TransicionesDeSesion
{
    /// <param name="actual">Estado antes del evento.</param>
    /// <param name="evento">Lo que pasó.</param>
    /// <param name="antesDeIngresar">Sólo para IngresoFallido: adónde vuelve (SinCuenta, Vencida o Revocada).</param>
    public static EstadoDeSesion Siguiente(EstadoDeSesion actual, EventoDeSesion evento, EstadoDeSesion antesDeIngresar = EstadoDeSesion.SinCuenta) =>
        (actual, evento) switch
        {
            // P4 primer ingreso; §4 «si falla o se revoca: volver al login».
            (EstadoDeSesion.SinCuenta or EstadoDeSesion.Vencida or EstadoDeSesion.Revocada, EventoDeSesion.IngresoIniciado) => EstadoDeSesion.Ingresando,
            (EstadoDeSesion.Ingresando, EventoDeSesion.IngresoExitoso) => EstadoDeSesion.Activa,
            // Cerrar el navegador no pierde la identidad previa (sólo se vuelve a un estado desde el que se pudo ingresar).
            (EstadoDeSesion.Ingresando, EventoDeSesion.IngresoFallido) =>
                antesDeIngresar is EstadoDeSesion.Vencida or EstadoDeSesion.Revocada ? antesDeIngresar : EstadoDeSesion.SinCuenta,

            // §4 «re-pedir la seguridad del teléfono…»; P4 desbloqueo local.
            (EstadoDeSesion.Activa, EventoDeSesion.Bloquear) => EstadoDeSesion.Bloqueada,
            (EstadoDeSesion.Bloqueada, EventoDeSesion.DesbloqueoExitoso) => EstadoDeSesion.Activa,
            (EstadoDeSesion.Bloqueada, EventoDeSesion.DesbloqueoCancelado) => EstadoDeSesion.Bloqueada,

            // §4 «si se agrega una huella nueva…» y «si falla o se revoca».
            // Corrector E03 — Desde Bloqueada no hubo prueba local en este proceso: va a SinCuenta (la entrada se cierra
            // y sólo se ofrece la cuenta). Desde Activa la persona ya probó quién es: Revocada, con la entrada abierta.
            (EstadoDeSesion.Bloqueada, EventoDeSesion.ClaveInvalidada) => EstadoDeSesion.SinCuenta,
            (EstadoDeSesion.Activa, EventoDeSesion.ClaveInvalidada) => EstadoDeSesion.Revocada,
            (EstadoDeSesion.Activa, EventoDeSesion.RenovacionRechazada) => EstadoDeSesion.Revocada,
            (EstadoDeSesion.Activa, EventoDeSesion.SesionVencida) => EstadoDeSesion.Vencida,

            // §4 «cerrar sesión = borrar tokens y la clave», desde cualquier estado con identidad.
            (not EstadoDeSesion.SinCuenta, EventoDeSesion.Cierre) => EstadoDeSesion.SinCuenta,

            _ => actual
        };

    /// <summary>
    /// Estado al arrancar en frío (DISENO §3.2). Corrector E03 — La entrada local exige una prueba local en ESTE
    /// proceso (P4 «entrada local separada»): con desbloqueo activado, Bloqueada (la huella, el rostro, el PIN o el
    /// patrón la abren aun sin red; si el refresh sellado ya venció, después del desbloqueo queda Vencida). Sin
    /// desbloqueo activado no hay con qué probar quién es: SinCuenta y se pide la cuenta (D-03 = T1). Las filas
    /// pendientes no se tocan: conservan su dueño y salen cuando ese dueño vuelva a ingresar.
    /// </summary>
    public static EstadoDeSesion AlArrancar(bool hayIdentidad, bool desbloqueoActivado) =>
        hayIdentidad && desbloqueoActivado ? EstadoDeSesion.Bloqueada : EstadoDeSesion.SinCuenta;

    /// <summary>
    /// Borrador §4 — Al volver de segundo plano: ¿se re-pide la seguridad del teléfono? Sólo con el desbloqueo activado,
    /// la sesión Activa y más de <paramref name="umbral"/> en segundo plano.
    /// </summary>
    public static bool DebeBloquearAlVolver(EstadoDeSesion actual, bool desbloqueoActivado, DateTimeOffset? pasoASegundoPlano, DateTimeOffset ahora, TimeSpan umbral) =>
        actual == EstadoDeSesion.Activa && desbloqueoActivado && pasoASegundoPlano is { } desde && ahora - desde >= umbral;
}

/// <summary>
/// D-03 = T1 — ¿Se ofrece el desbloqueo con la seguridad del teléfono? Sólo con API ≥ 30 (setUserAuthenticationParameters)
/// y con bloqueo de pantalla. Si no, se pide la cuenta cada vez que la sesión vence y el refresh no se persiste.
/// </summary>
public static class ReglaDelDesbloqueo
{
    public const string MotivoApiVieja = "El desbloqueo con la seguridad del teléfono necesita Android 11 o más nuevo.";
    public const string MotivoSinBloqueo = "Este teléfono no tiene bloqueo de pantalla: configurá un PIN, patrón o huella para usar la seguridad del teléfono.";

    public static (DesbloqueoLocal Desbloqueo, string? Motivo) Evaluar(int api, bool dispositivoSeguro, bool activado) =>
        api < ParametrosDeSesion.ApiMinimaDelDesbloqueoLocal ? (DesbloqueoLocal.NoDisponible, (string?)MotivoApiVieja)
        : !dispositivoSeguro ? (DesbloqueoLocal.NoDisponible, (string?)MotivoSinBloqueo)
        : activado ? (DesbloqueoLocal.Activado, (string?)null)
        : (DesbloqueoLocal.Disponible, (string?)null);
}

/// <summary>
/// P4 — El outbox tiene dueño: una fila sale sólo con la identidad de su dueño y con sesión válida. Nunca con otra
/// identidad. Cerrar o vencer la sesión no borra filas: quedan esperando a su dueño.
/// </summary>
public static class PropiedadDelOutbox
{
    /// <summary>Prefijo del dueño cuando la identidad la da el emisor: «oidc:{sub}».</summary>
    public const string PrefijoOidc = "oidc:";

    public static string DuenoOidc(string sub) => PrefijoOidc + sub;

    /// <param name="duenoDeLaFila">FilaDeOutbox.Dueno.</param>
    /// <param name="duenoActivo">El dueño de la identidad presente (null si no hay).</param>
    /// <param name="sesion">La sesión del momento.</param>
    public static bool PuedeEnviarse(string duenoDeLaFila, string? duenoActivo, SesionInformada sesion) =>
        // Sólo la identidad presente envía sus propias filas, nunca las de otro dueño…
        duenoActivo is not null
        && string.Equals(duenoDeLaFila, duenoActivo, StringComparison.Ordinal)
        // …y sólo con sesión válida: en modo Dispositivo siempre; con cuenta, Activa (Vencida, Revocada, Bloqueada no).
        && (sesion.Modo == ModoDeSesion.Dispositivo || sesion.Estado == EstadoDeSesion.Activa);
}
