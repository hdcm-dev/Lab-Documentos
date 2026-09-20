namespace MyProject.Application.Productos.Commands.CambiarPrecioProducto;

public record CambiarPrecioProductoCommand(Guid ProductoId, decimal NuevoPrecio);
