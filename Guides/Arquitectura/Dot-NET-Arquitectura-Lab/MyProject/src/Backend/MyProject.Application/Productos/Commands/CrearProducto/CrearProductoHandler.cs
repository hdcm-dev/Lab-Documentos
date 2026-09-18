using MyProject.Domain.Productos;

namespace MyProject.Application.Productos.Commands.CrearProducto;

public class CrearProductoHandler
{
    private readonly IProductoRepository _repository;

    public CrearProductoHandler(IProductoRepository repository) => _repository = repository;

    public async Task<Guid> Handle(CrearProductoCommand command, CancellationToken ct = default)
    {
        var producto = Producto.Create(command.Nombre, command.Precio); // la regla vive en el dominio
        await _repository.AddAsync(producto, ct);
        return producto.Id;
    }
}
