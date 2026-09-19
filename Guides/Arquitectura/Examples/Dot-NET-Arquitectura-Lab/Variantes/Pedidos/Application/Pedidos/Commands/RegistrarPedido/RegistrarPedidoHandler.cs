using MyProject.Application.Common;
using MyProject.Domain.Common;
using MyProject.Domain.Pedidos;
using MyProject.Domain.Productos;

namespace MyProject.Application.Pedidos.Commands.RegistrarPedido;

public class RegistrarPedidoHandler
{
    private readonly IProductoRepository _productos;
    private readonly IPedidoRepository _pedidos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _reloj;

    public RegistrarPedidoHandler(
        IProductoRepository productos, IPedidoRepository pedidos, IUnitOfWork unitOfWork, TimeProvider reloj)
        => (_productos, _pedidos, _unitOfWork, _reloj) = (productos, pedidos, unitOfWork, reloj);

    public async Task<PedidoRegistrado> Handle(RegistrarPedidoCommand command, CancellationToken ct = default)
    {
        var pedido = Pedido.Create(_reloj.GetUtcNow().UtcDateTime);

        foreach (var item in command.Items)                                    // pasos 2 y 3
        {
            var producto = await _productos.GetByIdAsync(item.ProductoId, ct)
                ?? throw new DomainException($"No existe el producto {item.ProductoId}."); // 2a
            pedido.AgregarItem(producto, item.Cantidad);                       // 3a y 3b, en Domain
        }

        pedido.Confirmar();                                                    // 4a, en Domain
        _pedidos.Add(pedido);
        await _unitOfWork.SaveChangesAsync(ct);                                // una sola confirmación
        return new PedidoRegistrado(pedido.Id, pedido.Total);                  // paso 5
    }
}
