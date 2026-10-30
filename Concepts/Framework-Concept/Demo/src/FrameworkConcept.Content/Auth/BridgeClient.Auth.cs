using FrameworkConcept.Core.Auth;

namespace FrameworkConcept.Content.Bridge;

// Folio 038 — Filas auth.* del puente 1.1 (DISENO-RED-Y-AUTENTICACION.md §6). Al contenido sólo le llega
// SesionInformada: nunca un token (P3). Archivo del constructor de autenticación.
public sealed partial class BridgeClient
{
    public async Task<SesionInformada> AuthStatusAsync() => (await CallAsync<ConSesion>("auth.status")).Sesion;

    /// <summary>Abre el navegador del sistema; la llamada vuelve cuando el navegador vuelve (o la persona lo cierra).</summary>
    public async Task<SesionInformada> AuthSignInAsync() => (await CallAsync<ConSesion>("auth.signIn")).Sesion;

    /// <summary>
    /// Login tradicional (puente 1.2): la credencial cruza el puente UNA vez, por este método; el cascarón la canjea y
    /// la descarta. No se registra: CallAsync no escribe parámetros en la consola.
    /// </summary>
    public async Task<SesionInformada> AuthSignInWithPasswordAsync(string usuario, string clave) =>
        (await CallAsync<ConSesion>("auth.signInWithPassword", new { usuario, clave })).Sesion;

    /// <summary>Puente 1.2: bloquea ya; el desbloqueo es el diálogo del sistema (auth.unlock).</summary>
    public async Task<SesionInformada> AuthLockAsync() => (await CallAsync<ConSesion>("auth.lock")).Sesion;

    public async Task<SesionInformada> AuthEnableUnlockAsync() => (await CallAsync<ConSesion>("auth.enableUnlock")).Sesion;

    public async Task<SesionInformada> AuthUnlockAsync() => (await CallAsync<ConSesion>("auth.unlock")).Sesion;

    public async Task<(SesionInformada Sesion, int PendientesConservados)> AuthSignOutAsync()
    {
        var r = await CallAsync<CierreDeSesion>("auth.signOut");
        return (r.Sesion, r.PendientesConservados);
    }

    private sealed record ConSesion(SesionInformada Sesion);

    private sealed record CierreDeSesion(SesionInformada Sesion, int PendientesConservados);
}
