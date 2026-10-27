using FrameworkConcept.Shell.Data;

namespace FrameworkConcept.Shell.Sync;

/// <summary>
/// P4 — El «dueño» de las filas del outbox en modo Dispositivo (APK compilado sin -p:FcOidcAutoridad): la sesión por
/// dispositivo del caso base, un identificador que se crea en el primer arranque y queda en la tabla de configuración.
/// En modo Oidc (folio 038) el dueño es la identidad autenticada, «oidc:{sub}» (Auth/SesionDelUsuario.cs), y las filas
/// de otro dueño no se envían nunca.
/// </summary>
public sealed class IdentidadDelDispositivo(IDb db)
{
    private const string Clave = "instalacion.id";
    private string? _id;

    public async Task<string> ObtenerAsync()
    {
        if (_id is not null) return _id;
        _id = await db.LeerConfiguracionAsync(Clave);
        if (_id is null)
        {
            _id = "dispositivo-" + Guid.NewGuid().ToString("N");
            await db.EscribirConfiguracionAsync(Clave, _id);
        }
        return _id;
    }
}
