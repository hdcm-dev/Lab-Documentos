namespace MyProject.Domain.Pedidos;

public interface IPedidoRepository
{
    void Add(Pedido pedido);   // marca para insertar; confirma IUnitOfWork
}
