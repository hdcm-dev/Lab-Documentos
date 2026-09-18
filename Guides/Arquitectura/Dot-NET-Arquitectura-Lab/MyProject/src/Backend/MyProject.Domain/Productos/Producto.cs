using MyProject.Domain.Common;

namespace MyProject.Domain.Productos;

public class Producto
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }
    public bool Activo { get; private set; }

    // Constructor privado: la única forma de crear un Producto es Create.
    private Producto() { }

    public static Producto Create(string nombre, decimal precio)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre es obligatorio.");
        if (precio <= 0)
            throw new DomainException("El precio debe ser mayor a cero.");

        return new Producto { Id = Guid.NewGuid(), Nombre = nombre, Precio = precio, Activo = true };
    }

    public void Desactivar() => Activo = false;
}
