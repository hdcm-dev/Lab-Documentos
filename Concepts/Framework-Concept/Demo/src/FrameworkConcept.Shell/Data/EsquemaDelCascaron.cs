namespace FrameworkConcept.Shell.Data;

/// <summary>
/// P5, P7 — El número de esquema de la base del cascarón: un entero monótono (VPI §2.7).
///
/// C-1 (aprobado por el PO, folio 023): el esquema es un sobre FIJO. El binario exige este número y no arranca si la
/// base tiene otro; la migración al abrir no se implementa. Se guarda en <c>PRAGMA user_version</c>, el lugar que
/// SQLite reserva para eso. Subir este número es un cambio del cascarón: sale por tienda, con su decisión sobre el sobre.
/// </summary>
public static class EsquemaDelCascaron
{
    public const int Version = 1;

    public const string NombreDelArchivo = "cascaron.db";
}
