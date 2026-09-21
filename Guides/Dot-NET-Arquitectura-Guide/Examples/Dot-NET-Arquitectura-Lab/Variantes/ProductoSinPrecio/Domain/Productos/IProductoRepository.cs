namespace MyProject.Domain.Productos;

public interface IProductoRepository
{
    Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Producto producto, CancellationToken ct = default);
}
