using MyProject.Domain.Common;

namespace MyProject.Domain.Productos;

// Variante de MyProject/src/Backend/MyProject.Domain/Productos/Producto.cs (§3.7 de la guía):
// el producto puede existir sin precio; lo que no se puede es venderlo.
public class Producto
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal? Precio { get; private set; }   // Structural Assertion: el precio puede faltar
    public bool Activo { get; private set; }

    private Producto() { }

    public static Producto Create(string nombre, decimal? precio = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre es obligatorio.");

        var producto = new Producto { Id = Guid.NewGuid(), Nombre = nombre, Activo = true };
        if (precio is not null)
            producto.AsignarPrecio(precio.Value);
        return producto;
    }

    // Invariant: si hay precio, es mayor a cero.
    public void AsignarPrecio(decimal precio)
    {
        if (precio <= 0)
            throw new DomainException("El precio debe ser mayor a cero.");
        Precio = precio;
    }

    // Precondition del acto de vender: quien vende pide este precio, no lee Precio.
    public decimal PrecioDeVenta() =>
        Precio ?? throw new DomainException($"'{Nombre}' no tiene precio: no se puede vender.");

    public void Desactivar() => Activo = false;
}
