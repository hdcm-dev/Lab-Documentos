using System.Text.Json;
using FrameworkConcept.Core.Auth;

namespace FrameworkConcept.Shell.Auth;

/// <summary>
/// Los métodos auth.* del puente (DISENO §6). El despachador le pasa todo método que empiece con «auth.»: así el
/// constructor de autenticación trabaja en este archivo y no en el despachador. Sólo devuelve SesionInformada: nunca
/// tokens (P3).
/// </summary>
public sealed class AtencionDeSesion(ISesionDelUsuario sesion)
{
    public static bool Atiende(string metodo) => metodo.StartsWith("auth.", StringComparison.Ordinal);

    public async Task<object> ResolverAsync(string metodo, JsonElement p, CancellationToken ct)
    {
        switch (metodo)
        {
            case "auth.status":
                // El contenido lo pide al arrancar: desde acá está a la vista y las pantallas propias de ingreso y
                // desbloqueo pueden ir encima (PresentadorDeSesion).
                PresentadorDeSesion.ContenidoVisible(sesion.Actual);
                return new { sesion = sesion.Actual };
            case "auth.signIn":
                return new { sesion = await sesion.IngresarAsync(ct) };
            case "auth.signInWithPassword":
            {
                // Regla 2 del login tradicional: la credencial cruza el puente UNA vez, por este método; se pasa al canje
                // y se descarta. Nunca se registra (el despachador sólo anota el nombre del método) ni se guarda.
                if (!p.TryGetProperty("usuario", out var usuario) || usuario.ValueKind != JsonValueKind.String
                    || !p.TryGetProperty("clave", out var clave) || clave.ValueKind != JsonValueKind.String)
                    throw ExcepcionDeAutenticacion.Fallida("Falta usuario o clave.");
                return new { sesion = await sesion.IngresarConClaveAsync(usuario.GetString()!, clave.GetString()!, ct) };
            }
            case "auth.lock":
                return new { sesion = await sesion.BloquearAsync(ct) };
            case "auth.enableUnlock":
                return new { sesion = await sesion.ActivarDesbloqueoAsync(ct) };
            case "auth.unlock":
                return new { sesion = await sesion.DesbloquearAsync(ct) };
            case "auth.signOut":
            {
                var (nueva, pendientesConservados) = await sesion.CerrarAsync(ct);
                return new { sesion = nueva, pendientesConservados };
            }
            default:
                throw new InvalidOperationException($"AtencionDeSesion no atiende «{metodo}».");
        }
    }
}
