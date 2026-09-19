namespace MyProject.Domain.Productos;

/// <summary>Lo que el dominio necesita para guardar y recuperar productos. La implementación vive en Infrastructure.</summary>
public interface IProductoRepository
{
    Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Producto producto, CancellationToken ct = default);
}
