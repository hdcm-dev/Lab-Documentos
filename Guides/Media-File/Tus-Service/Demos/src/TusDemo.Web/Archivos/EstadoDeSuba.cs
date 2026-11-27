using System.Text;
using tusdotnet.Interfaces;
using tusdotnet.Stores;

namespace TusDemo.Web.Archivos;

/// <summary>Lo que el almacén sabe de una suba: si existe, si está completa y sus metadatos.</summary>
public sealed record EstadoDeSuba(bool Existe, bool Completa, long Largo, string Nombre, string Tipo);

public static class AlmacenExtensiones
{
    /// <summary>
    /// Consulta el almacén de tus. Una suba está completa cuando lo recibido (offset)
    /// alcanzó el largo declarado al crearla (Upload-Length).
    /// </summary>
    public static async Task<EstadoDeSuba> ConsultarAsync(this TusDiskStore almacen, string id, CancellationToken ct)
    {
        var noExiste = new EstadoDeSuba(false, false, 0, "", "");
        // El id llega del cliente: sólo se aceptan letras y dígitos (el almacén genera GUID sin guiones).
        // Así un id como "../otra-carpeta" nunca llega a armar una ruta en disco.
        if (string.IsNullOrWhiteSpace(id) || !id.All(char.IsAsciiLetterOrDigit))
            return noExiste;
        if (!await almacen.FileExistAsync(id, ct))
            return noExiste;

        long? largo = await almacen.GetUploadLengthAsync(id, ct);
        long recibido = await almacen.GetUploadOffsetAsync(id, ct);

        ITusFile archivo = await almacen.GetFileAsync(id, ct);
        var meta = await archivo.GetMetadataAsync(ct);
        string nombre = meta.TryGetValue("filename", out var n) ? n.GetString(Encoding.UTF8) : id;
        string tipo = meta.TryGetValue("filetype", out var t) ? t.GetString(Encoding.UTF8) : "application/octet-stream";

        return new EstadoDeSuba(true, largo is not null && recibido == largo, largo ?? 0, nombre, tipo);
    }
}
