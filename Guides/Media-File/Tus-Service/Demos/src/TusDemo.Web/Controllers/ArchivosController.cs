using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TusDemo.Web.Archivos;

namespace TusDemo.Web.Controllers;

/// <summary>
/// La "fila de archivo": un archivo se registra recién cuando su suba tus terminó.
/// </summary>
[ApiController]
[Route("api/archivos")]
[Authorize]
[Produces("application/json")]
public sealed class ArchivosController(RegistroDeArchivos registro, ServicioDeArchivos servicio) : ControllerBase
{
    /// <summary>Lista los archivos registrados, del más nuevo al más viejo.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ArchivoRegistrado>>(StatusCodes.Status200OK)]
    public IReadOnlyList<ArchivoRegistrado> Listar() => registro.Todos();

    /// <summary>Devuelve un archivo registrado por su id.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType<ArchivoRegistrado>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ArchivoRegistrado> Obtener(long id) =>
        registro.Buscar(id) is { } fila ? fila : NotFound();

    /// <summary>
    /// Registra un archivo cuya suba tus terminó. Es idempotente: repetir el pedido con el mismo
    /// idSuba devuelve la fila existente (200) en lugar de crear otra.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<ArchivoRegistrado>(StatusCodes.Status201Created)]
    [ProducesResponseType<ArchivoRegistrado>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArchivoRegistrado>> Registrar(PedidoDeRegistro pedido, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "desconocido";
        var (resultado, fila) = await servicio.RegistrarAsync(pedido.IdSuba, pedido.Descripcion, usuario, ct);
        return resultado switch
        {
            ResultadoDeRegistro.Creado => CreatedAtAction(nameof(Obtener), new { id = fila!.Id }, fila),
            ResultadoDeRegistro.YaExistia => Ok(fila),
            ResultadoDeRegistro.NoExiste => NotFound($"No existe la suba {pedido.IdSuba}"),
            _ => Conflict($"La suba {pedido.IdSuba} todavía no terminó")
        };
    }
}
