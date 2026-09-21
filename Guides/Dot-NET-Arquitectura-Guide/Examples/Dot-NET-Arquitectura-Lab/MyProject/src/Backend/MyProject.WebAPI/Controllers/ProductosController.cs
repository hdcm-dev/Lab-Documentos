using Microsoft.AspNetCore.Mvc;
using MyProject.Application.Productos;
using MyProject.Application.Productos.Commands.CrearProducto;
using MyProject.Application.Productos.Queries.ObtenerProductoPorId;
using MyProject.Application.Productos.Queries.ObtenerProductos;
using MyProject.Contracts;

namespace MyProject.WebAPI.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoResponse>>> GetAll(
        [FromServices] ObtenerProductosHandler handler, CancellationToken ct)
    {
        var productos = await handler.Handle(new ObtenerProductosQuery(), ct);
        return Ok(productos.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductoResponse>> GetById(
        Guid id, [FromServices] ObtenerProductoPorIdHandler handler, CancellationToken ct)
    {
        var producto = await handler.Handle(new ObtenerProductoPorIdQuery(id), ct);
        return producto is null ? NotFound() : Ok(ToResponse(producto));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CrearProductoRequest request, [FromServices] CrearProductoHandler handler, CancellationToken ct)
    {
        var id = await handler.Handle(new CrearProductoCommand(request.Nombre, request.Precio), ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    // El contrato expone solo lo que promete: Activo no viaja.
    private static ProductoResponse ToResponse(ProductoDto producto) =>
        new(producto.Id, producto.Nombre, producto.Precio);
}
