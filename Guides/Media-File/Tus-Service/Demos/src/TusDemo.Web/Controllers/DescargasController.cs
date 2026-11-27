using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tusdotnet.Stores;
using TusDemo.Web.Archivos;

namespace TusDemo.Web.Controllers;

/// <summary>
/// Lectura de archivos. El protocolo tus no define la descarga, así que el servicio agrega la suya,
/// en una ruta distinta de la de tus (/archivos) para no chocar con sus métodos.
/// </summary>
[ApiController]
[Route("descargas")]
[Authorize]
public sealed class DescargasController(TusDiskStore almacen) : ControllerBase
{
    /// <summary>Devuelve el contenido de una suba completa.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Descargar(string id, CancellationToken ct)
    {
        var estado = await almacen.ConsultarAsync(id, ct);
        if (!estado.Existe) return NotFound();
        if (!estado.Completa) return Conflict("La suba todavía no terminó");

        var archivo = await almacen.GetFileAsync(id, ct);
        var contenido = await archivo.GetContentAsync(ct);
        return File(contenido, estado.Tipo, estado.Nombre);
    }
}
