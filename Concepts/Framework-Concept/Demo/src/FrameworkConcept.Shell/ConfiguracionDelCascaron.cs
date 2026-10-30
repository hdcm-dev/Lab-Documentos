using System.Reflection;
using FrameworkConcept.Core.Auth;

namespace FrameworkConcept.Shell;

/// <summary>
/// La configuración del cascarón, fijada al compilar (propiedades -p:FcCanal, -p:FcCanalUrl, -p:FcReceptorUrl,
/// -p:FcKeyId, -p:FcOidcAutoridad, -p:FcOidcClienteId, -p:FcPantallasDeSesion y -p:FcModoDeIngreso del .csproj). Así la misma fuente apunta a un túnel de prueba o a un servidor propio sin editar código.
/// </summary>
public sealed class ConfiguracionDelCascaron
{
    public ConfiguracionDelCascaron()
    {
        Canal = Leer("FcCanal", "dev");
        CanalUrl = new Uri(Leer("FcCanalUrl", "https://canal.invalid").TrimEnd('/') + "/");
        ReceptorUrl = new Uri(Leer("FcReceptorUrl", CanalUrl.ToString()).TrimEnd('/') + "/");
        KeyId = Leer("FcKeyId", "demo-2026");
        PantallasDeSesionEnElCascaron = string.Equals(Leer("FcPantallasDeSesion", "Contenido"), "Cascaron", StringComparison.OrdinalIgnoreCase);
        ModoDeIngreso = Enum.TryParse<ModoDeIngreso>(Leer("FcModoDeIngreso", "Formulario"), true, out var modo) ? modo : ModoDeIngreso.Formulario;

        // Folio 038, P4 — Emisor OIDC. Vacío = modo Dispositivo (la sesión por dispositivo del caso base).
        var autoridad = Leer("FcOidcAutoridad", "");
        Oidc = autoridad.Length == 0
            ? null
            : new ConfiguracionOidc(new Uri(autoridad.TrimEnd('/')), Leer("FcOidcClienteId", ParametrosDeSesion.ClienteIdPorDefecto),
                new Uri(ParametrosDeSesion.Redireccion), ParametrosDeSesion.Alcances);
    }

    /// <summary>
    /// P4, D-07 = I1 — El emisor OIDC (-p:FcOidcAutoridad, -p:FcOidcClienteId). null = modo Dispositivo: sin login,
    /// como en los tramos A1–A3. En el ensayo, https://203.0.113.10:8443/realms/fc-demo (infra/keycloak/).
    /// </summary>
    public ConfiguracionOidc? Oidc { get; }

    /// <summary>dev, beta o prod. El canal es un puntero, no una versión (P7): cada canal es un índice distinto.</summary>
    public string Canal { get; }

    /// <summary>Raíz del servidor estático. El índice está en {CanalUrl}/{canal}/puente-{mayor}/indice.json (P2).</summary>
    public Uri CanalUrl { get; }

    /// <summary>Adonde el outbox envía las respuestas (en la demo, el receptor didáctico).</summary>
    public Uri ReceptorUrl { get; }

    /// <summary>
    /// Quién muestra el ingreso y el desbloqueo (-p:FcPantallasDeSesion). Por defecto, el contenido: su puerta de sesión
    /// (Content/Auth/PuertaDeSesion.razor) es la cara del login y se actualiza sin tienda; el canje, los tokens y el
    /// diálogo del sistema siguen en el cascarón (auth.session). true = las pantallas XAML del cascarón la tapan.
    /// </summary>
    public bool PantallasDeSesionEnElCascaron { get; }

    /// <summary>
    /// Login tradicional (-p:FcModoDeIngreso=Formulario|Navegador, por defecto Formulario): la puerta del contenido
    /// muestra el formulario propio (auth.signInWithPassword) o el botón del navegador (auth.signIn). Viaja al
    /// contenido en SesionInformada.ModoDeIngreso. Las pantallas XAML del cascarón siguen con el navegador.
    /// </summary>
    public ModoDeIngreso ModoDeIngreso { get; }

    /// <summary>keyId de la clave pública embebida (claves/publica-demo.pem).</summary>
    public string KeyId { get; }

    private static string Leer(string clave, string porDefecto) =>
        typeof(ConfiguracionDelCascaron).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == clave)?.Value is { Length: > 0 } valor ? valor : porDefecto;
}
