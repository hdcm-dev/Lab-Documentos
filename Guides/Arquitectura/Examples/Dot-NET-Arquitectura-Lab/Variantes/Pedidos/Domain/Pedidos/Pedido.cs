using MyProject.Domain.Common;
using MyProject.Domain.Productos;

namespace MyProject.Domain.Pedidos;

// Aggregate Root: los ítems se crean y se leen solo a través del Pedido.
public class Pedido
{
    private readonly List<ItemPedido> _items = new();

    public Guid Id { get; private set; }
    public DateTime Fecha { get; private set; }
    public bool Confirmado { get; private set; }
    public IReadOnlyList<ItemPedido> Items => _items;
    public decimal Total => _items.Sum(i => i.Subtotal);    // Derivation: se calcula, no se guarda

    private Pedido() { }

    public static Pedido Create(DateTime fecha) => new() { Id = Guid.NewGuid(), Fecha = fecha };

    public void AgregarItem(Producto producto, int cantidad)
    {
        if (Confirmado)
            throw new DomainException("Un pedido confirmado no se modifica.");
        if (cantidad <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        _items.Add(new ItemPedido(producto.Id, producto.Nombre, producto.PrecioDeVenta(), cantidad));
    }

    public void Confirmar()
    {
        if (_items.Count == 0)
            throw new DomainException("No se confirma un pedido sin ítems.");
        Confirmado = true;
    }
}
