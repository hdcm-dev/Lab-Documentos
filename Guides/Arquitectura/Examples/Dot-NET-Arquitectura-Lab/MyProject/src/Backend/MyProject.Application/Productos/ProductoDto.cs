using MyProject.Domain.Productos;

namespace MyProject.Application.Productos;

/// <summary>Modelo de lectura que devuelven los casos de uso. Mapeo manual desde la entidad.</summary>
public record ProductoDto(Guid Id, string Nombre, decimal Precio, bool Activo)
{
    public static ProductoDto From(Producto producto) =>
        new(producto.Id, producto.Nombre, producto.Precio, producto.Activo);
}
