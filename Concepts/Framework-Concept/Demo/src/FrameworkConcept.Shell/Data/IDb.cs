using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Shell.Data;

/// <summary>
/// P5 — Todo lo que el cascarón guarda en el teléfono pasa por esta interfaz (D-05: EF Core SQLite detrás de IDb).
/// Así el resto del código no sabe que hay EF Core abajo, y cambiar de motor (S1 o S3, condición de revisión de P5)
/// no toca a nadie más.
/// </summary>
public interface IDb
{
    /// <summary>Crea la base si no existe y comprueba el número de esquema. Lanza <see cref="EsquemaIncompatibleException"/>.</summary>
    Task AbrirAsync(CancellationToken ct = default);

    // --- Documentos del contenido ---
    Task<FilaDeDocumento?> ObtenerDocumentoAsync(string coleccion, string id, CancellationToken ct = default);
    Task<IReadOnlyList<FilaDeDocumento>> ListarDocumentosAsync(string coleccion, CancellationToken ct = default);

    /// <summary>Crea o reemplaza. Con <paramref name="versionEsperada"/> lanza <see cref="ConflictoDeVersionException"/> si no coincide.</summary>
    Task<int> GuardarDocumentoAsync(string coleccion, string id, int versionDeForma, string json, int? versionEsperada, CancellationToken ct = default);

    Task<bool> BorrarDocumentoAsync(string coleccion, string id, CancellationToken ct = default);

    /// <summary>
    /// P5 — Guarda el documento y su fila del outbox en UNA transacción: o quedan los dos o ninguno.
    /// Devuelve false si la clave de idempotencia ya estaba (no se duplica).
    /// </summary>
    Task<bool> GuardarYEncolarAsync(FilaDeDocumento documento, FilaDeOutbox fila, CancellationToken ct = default);

    // --- Outbox ---
    Task<IReadOnlyList<FilaDeOutbox>> ListarOutboxSinEnviarAsync(CancellationToken ct = default);
    Task ActualizarFilaAsync(FilaDeOutbox fila, CancellationToken ct = default);
    Task<(int Pendientes, int Rechazadas)> ContarOutboxAsync(CancellationToken ct = default);

    /// <summary>
    /// P4, folio 038 — Como <see cref="ContarOutboxAsync"/>, sólo las filas de <paramref name="dueno"/> (las de otra identidad
    /// son invisibles). Con null cuenta todas, como en A3.
    /// </summary>
    Task<(int Pendientes, int Rechazadas)> ContarOutboxDeAsync(string? dueno, CancellationToken ct = default);

    // --- Definiciones (P13.3) ---
    Task GuardarDefinicionAsync(DefinicionEncuesta definicion, string json, CancellationToken ct = default);
    Task<FilaDeDefinicion?> ObtenerDefinicionAsync(string encuestaId, int version, CancellationToken ct = default);
    Task<IReadOnlyList<FilaDeDefinicion>> ListarDefinicionesAsync(string? encuestaId = null, CancellationToken ct = default);

    // --- Versión de contenido y configuración ---
    Task AnotarVersionDeContenidoAsync(string version, string evento, CancellationToken ct = default);
    Task<IReadOnlyList<FilaDeVersionDeContenido>> UltimosEventosDeContenidoAsync(int cantidad, CancellationToken ct = default);
    Task<string?> LeerConfiguracionAsync(string clave, CancellationToken ct = default);
    Task EscribirConfiguracionAsync(string clave, string valor, CancellationToken ct = default);
}

/// <summary>La base del teléfono tiene otro número de esquema: este binario no arranca sobre ella (C-1, VPI §2.7 cláusula 2).</summary>
public sealed class EsquemaIncompatibleException(int encontrado, int exigido)
    : Exception($"La base local tiene el esquema {encontrado} y este cascarón exige el {exigido}.")
{
    public int Encontrado { get; } = encontrado;
    public int Exigido { get; } = exigido;
}

/// <summary>Otro cambió el documento: la versión esperada no coincide (D-04).</summary>
public sealed class ConflictoDeVersionException(int esperada, int actual)
    : Exception($"Se esperaba la versión {esperada} y el documento está en la {actual}.");
