namespace TusDemo.Web.Archivos;

/// <summary>
/// Una "fila de archivo": lo que el producto guarda de un archivo ya subido.
/// <see cref="Documento"/> es la ruta relativa para leerlo ("descargas/&lt;id&gt;"), nunca el host.
/// </summary>
public sealed record ArchivoRegistrado(
    long Id,
    string Documento,
    string Nombre,
    string Tipo,
    long Bytes,
    string Descripcion,
    DateTimeOffset FechaAlta,
    string UsuarioAlta);

/// <summary>Lo que el cliente manda para registrar un archivo cuya suba terminó.</summary>
public sealed record PedidoDeRegistro(string IdSuba, string? Descripcion);
