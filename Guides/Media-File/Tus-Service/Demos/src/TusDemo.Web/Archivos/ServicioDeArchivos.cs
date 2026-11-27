using tusdotnet.Stores;

namespace TusDemo.Web.Archivos;

public enum ResultadoDeRegistro { Creado, YaExistia, NoExiste, Incompleto }

/// <summary>
/// Registra la fila de un archivo cuya suba terminó. Lo usan el controller de la API y la página Blazor,
/// así las dos puertas siguen el mismo orden: primero la suba completa, después la fila.
/// </summary>
public sealed class ServicioDeArchivos(TusDiskStore almacen, RegistroDeArchivos registro, AvisoDeArchivos aviso)
{
    public async Task<(ResultadoDeRegistro Resultado, ArchivoRegistrado? Fila)> RegistrarAsync(
        string idSuba, string? descripcion, string usuario, CancellationToken ct)
    {
        var estado = await almacen.ConsultarAsync(idSuba, ct);
        if (!estado.Existe) return (ResultadoDeRegistro.NoExiste, null);
        if (!estado.Completa) return (ResultadoDeRegistro.Incompleto, null);

        var (fila, creada) = registro.RegistrarSiFalta(
            documento: $"descargas/{idSuba}",
            nombre: estado.Nombre,
            tipo: estado.Tipo,
            bytes: estado.Largo,
            descripcion: descripcion ?? "",
            usuario: usuario);

        if (!creada) return (ResultadoDeRegistro.YaExistia, fila);
        await aviso.AvisarAsync();
        return (ResultadoDeRegistro.Creado, fila);
    }
}
