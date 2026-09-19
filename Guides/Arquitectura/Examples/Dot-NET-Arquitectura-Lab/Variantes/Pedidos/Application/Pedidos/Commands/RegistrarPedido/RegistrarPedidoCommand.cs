namespace MyProject.Application.Pedidos.Commands.RegistrarPedido;

public record RegistrarPedidoItem(Guid ProductoId, int Cantidad);

public record RegistrarPedidoCommand(IReadOnlyList<RegistrarPedidoItem> Items);

public record PedidoRegistrado(Guid PedidoId, decimal Total);
