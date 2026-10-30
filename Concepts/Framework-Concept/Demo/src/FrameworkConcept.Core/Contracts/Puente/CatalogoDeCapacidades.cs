namespace FrameworkConcept.Core.Contracts.Puente;

/// <summary>
/// Una capacidad del dispositivo que el cascarón implementa (P12): id estable, versión PROPIA de la
/// entrada (P7) y lo que declara en cada plataforma. El manifiesto de la app (AndroidManifest.xml,
/// Info.plist) declara sólo lo que este catálogo implementa; la compuerta lo compara (P8).
/// </summary>
/// <param name="Id">Estable, por ejemplo «device.info». No se reutiliza.</param>
/// <param name="Version">SemVer de la entrada. El contenido la pide en <c>requires</c> con un rango.</param>
/// <param name="PermisosAndroid">Permisos <c>uses-permission</c> que la entrada necesita.</param>
/// <param name="ClavesDeUsoIos">Claves <c>NS*UsageDescription</c> de Info.plist (rama iOS: diseño, NO VERIFICADO).</param>
/// <param name="ConsultasAndroid">Acciones de intent que la entrada declara en <c>&lt;queries&gt;</c> del AndroidManifest
/// (visibilidad de paquetes, API ≥ 30). La compuerta las compara con el manifiesto (P8).</param>
public sealed record EntradaDelCatalogo(
    string Id,
    VersionSemantica Version,
    IReadOnlyList<string> PermisosAndroid,
    IReadOnlyList<string> ClavesDeUsoIos,
    IReadOnlyList<string>? ConsultasAndroid = null)
{
    /// <summary><see cref="ConsultasAndroid"/> sin null: una entrada sin consultas devuelve una lista vacía.</summary>
    public IReadOnlyList<string> ConsultasAndroidDeclaradas => ConsultasAndroid ?? [];
}

/// <summary>Lo que el handshake informa de cada entrada: id y versión.</summary>
public sealed record EntradaVersionada(string Id, VersionSemantica Version);

/// <summary>
/// El catálogo es una CONSTANTE del cascarón (P7, P12). Vive en Core —y no en el proyecto MAUI— para que
/// la instantánea del contrato y la compuerta lo lean sin compilar Android; el cascarón lo usa tal cual.
/// Catálogo de la demo: sólo lo que su contenido usa, sin permisos de tiempo de ejecución (P12).
/// </summary>
public static class CatalogoDeCapacidades
{
    public static IReadOnlyList<EntradaDelCatalogo> Entradas { get; } =
    [
        new("device.info", VersionSemantica.Parse("1.0.0"), [], []),
        // ACCESS_NETWORK_STATE es un permiso «normal»: se concede al instalar, no se pide en el uso.
        // 1.1.0 (incremento de red, folio 038): network.status y network.changed suman el estado combinado
        // (borrador §6) como campos opcionales. Menor de la entrada: el contenido que pide >=1.0.0 <2.0.0 sigue andando.
        new("network.status", VersionSemantica.Parse("1.1.0"), ["android.permission.ACCESS_NETWORK_STATE"], []),
        // auth.session 1.0.0 (folio 038, P4): sesión OIDC y desbloqueo con la seguridad del teléfono.
        //   USE_BIOMETRIC: permiso «normal» de BiometricPrompt (huella, rostro o credencial del dispositivo).
        //   <queries> CustomTabsService: con API ≥ 30 el cascarón no ve el navegador de Custom Tabs sin declararlo
        //   (visibilidad de paquetes); WebAuthenticator abre el navegador del sistema (RFC 8252).
        //   NSFaceIDUsageDescription: rama iOS, diseño (P9, NO VERIFICADO).
        // 1.1.0 (login tradicional, 2026-10-30): signInWithPassword, lock y modoDeIngreso. Menor de la entrada.
        new("auth.session", VersionSemantica.Parse("1.1.0"), ["android.permission.USE_BIOMETRIC"], ["NSFaceIDUsageDescription"],
            ["android.support.customtabs.action.CustomTabsService"])
    ];

    public static EntradaDelCatalogo? Buscar(string id) =>
        Entradas.FirstOrDefault(e => string.Equals(e.Id, id, StringComparison.Ordinal));

    public static IReadOnlyList<EntradaVersionada> Versionadas() =>
        [.. Entradas.Select(e => new EntradaVersionada(e.Id, e.Version))];
}
