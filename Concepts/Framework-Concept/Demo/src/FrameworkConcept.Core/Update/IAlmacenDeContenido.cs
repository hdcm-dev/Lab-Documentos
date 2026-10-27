using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Contenido;

namespace FrameworkConcept.Core.Update;

/// <summary>
/// Los archivos del contenido en el teléfono (P2). Disposición:
///   content/staging/vN/     lo bajado y verificado, todavía no activo
///   content/versions/vN/    versiones activables, inmutables (nunca se escribe adentro después de promover)
///   content/puntero.json    current, previous y la marca «pendiente»; se reemplaza con UN rename
///   content/fallidas.json   versiones que fallaron acá y no se reinstalan
/// Implementación sobre el sistema de archivos: <see cref="AlmacenDeContenidoEnDisco"/> (la usa el cascarón);
/// las pruebas usan además una en memoria que puede «cortarse» en cualquier paso.
/// </summary>
public interface IAlmacenDeContenido
{
    Task<PunteroDeContenido?> LeerPunteroAsync(CancellationToken cancellationToken = default);

    /// <summary>Escribe un temporal y lo renombra sobre puntero.json: o queda el viejo o el nuevo, nunca medio.</summary>
    Task ReemplazarPunteroAsync(PunteroDeContenido puntero, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<VersionSemantica>> LeerFallidasAsync(CancellationToken cancellationToken = default);

    Task AgregarFallidaAsync(VersionSemantica version, CancellationToken cancellationToken = default);

    /// <summary>Descomprime el zip (ya verificado por tamaño y sha256) en staging/vN, reemplazando lo que hubiera.</summary>
    Task DescomprimirEnStagingAsync(VersionSemantica version, Stream zip, CancellationToken cancellationToken = default);

    /// <summary>La versión que está completa en staging/, si hay una.</summary>
    Task<VersionSemantica?> VersionEnStagingAsync(CancellationToken cancellationToken = default);

    /// <summary>Lee el manifiesto de una versión en staging/ o en versions/.</summary>
    Task<ManifiestoContenido?> LeerManifiestoAsync(VersionSemantica version, CancellationToken cancellationToken = default);

    /// <summary>Mueve staging/vN a versions/vN (un rename en el mismo volumen).</summary>
    Task PromoverAsync(VersionSemantica version, CancellationToken cancellationToken = default);

    bool ExisteVersion(VersionSemantica version);

    /// <summary>Directorio raíz de una versión activable (lo sirve el servidor de contenido).</summary>
    string DirectorioDe(VersionSemantica version);

    /// <summary>Borra versions/vN. La retención del teléfono (P2) la usa para todo lo que no sea current ni previous.</summary>
    Task BorrarVersionAsync(VersionSemantica version, CancellationToken cancellationToken = default);

    /// <summary>Las versiones que hay en versions/ (para la retención: lo que sobra se borra al arrancar).</summary>
    Task<IReadOnlyList<VersionSemantica>> ListarVersionesAsync(CancellationToken cancellationToken = default);

    /// <summary>Vacía staging/: lo que quedó de una descarga cortada o rechazada no se activa nunca.</summary>
    Task LimpiarStagingAsync(CancellationToken cancellationToken = default);

    /// <summary>El último índice que pasó la verificación (para las retiradas y la política de soporte sin red).</summary>
    Task<IndiceDelCanal?> LeerUltimoIndiceValidoAsync(CancellationToken cancellationToken = default);

    Task GuardarUltimoIndiceValidoAsync(IndiceDelCanal indice, CancellationToken cancellationToken = default);
}

/// <summary>
/// puntero.json. <see cref="Pendiente"/> es la marca que se pone al activar y que el contenido borra con
/// «Listo(version)» (P2, rollback local).
/// <see cref="CargasSinListo"/> cuenta los arranques en que el WebView EMPEZÓ a cargar la versión pendiente (pidió
/// <c>_framework/dotnet.js</c>) sin que llegara «Listo». Un arranque cortado antes de cargar (la persona cerró la app,
/// Android mató el proceso) no suma: la marca sola no prueba que el contenido falló.
/// <see cref="SemillaRevisada"/> es la huella de la instalación del cascarón cuya semilla ya se revisó: mientras no
/// cambie (no se instaló otro APK), el arranque no vuelve a leer ni verificar la semilla (P1, umbral TPR).
/// </summary>
public sealed record PunteroDeContenido(
    VersionSemantica? Current,
    VersionSemantica? Previous,
    VersionSemantica? Pendiente,
    DateTimeOffset? UltimaConsultaExitosa = null,
    int CargasSinListo = 0,
    string? SemillaRevisada = null);
