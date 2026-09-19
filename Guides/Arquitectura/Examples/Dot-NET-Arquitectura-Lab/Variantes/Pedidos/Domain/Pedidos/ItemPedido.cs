namespace MyProject.Domain.Pedidos;

public class ItemPedido
{
    public Guid ProductoId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal PrecioUnitario { get; private set; }   // copia del precio al momento de vender
    public int Cantidad { get; private set; }
    public decimal Subtotal => PrecioUnitario * Cantidad;

    private ItemPedido() { }

    // internal: fuera de Domain nadie crea un ítem sin pasar por Pedido.AgregarItem.
    internal ItemPedido(Guid productoId, string descripcion, decimal precioUnitario, int cantidad)
    {
        ProductoId = productoId;
        Descripcion = descripcion;
        PrecioUnitario = precioUnitario;
        Cantidad = cantidad;
    }
}
