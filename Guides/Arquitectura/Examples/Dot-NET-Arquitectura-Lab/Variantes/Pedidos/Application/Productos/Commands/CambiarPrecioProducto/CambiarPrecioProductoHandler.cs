using MyProject.Application.Common;
using MyProject.Domain.Common;
using MyProject.Domain.Productos;

namespace MyProject.Application.Productos.Commands.CambiarPrecioProducto;

// Use Case de modificación: obtener → cambiar en la Entity → confirmar. El ciclo que necesita seguimiento (§5.5).
public class CambiarPrecioProductoHandler
{
    private readonly IProductoRepository _productos;
    private readonly IUnitOfWork _unitOfWork;

    public CambiarPrecioProductoHandler(IProductoRepository productos, IUnitOfWork unitOfWork)
        => (_productos, _unitOfWork) = (productos, unitOfWork);

    // Devuelve las filas afectadas: 1 si el cambio llegó a la base, 0 si la Entity no estaba rastreada.
    public async Task<int> Handle(CambiarPrecioProductoCommand command, CancellationToken ct = default)
    {
        var producto = await _productos.GetByIdAsync(command.ProductoId, ct)
            ?? throw new DomainException($"No existe el producto {command.ProductoId}.");
        producto.AsignarPrecio(command.NuevoPrecio);          // la regla (precio > 0) sigue en Domain
        return await _unitOfWork.SaveChangesAsync(ct);        // una sola confirmación
    }
}
