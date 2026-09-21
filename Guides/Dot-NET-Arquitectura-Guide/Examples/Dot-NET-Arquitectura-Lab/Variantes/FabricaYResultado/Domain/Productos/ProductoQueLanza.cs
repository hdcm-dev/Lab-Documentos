using MyProject.Domain.Common;

namespace MyProject.Domain.Productos;

// ESTRATEGIA A — la de §3.3: la regla incumplida se LANZA.
// Constructor privado sin parámetros + Factory Function `Create`: el mismo patrón de MyProject.
public class ProductoQueLanza
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }
    public EstadoProducto Estado { get; private set; }

    // Dos propósitos en el mismo constructor: aquí entra EF Core al materializar una fila, y por aquí
    // pasa también `Create` al constituir un producto nuevo. Este camino NO valida (§5.4).
    private ProductoQueLanza() { }

    public static ProductoQueLanza Create(string nombre, decimal precio)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre es obligatorio.");
        if (precio <= 0)
            throw new DomainException("El precio debe ser mayor a cero.");

        return new ProductoQueLanza
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Precio = precio,
            Estado = EstadoProducto.Borrador
        };
    }
}
