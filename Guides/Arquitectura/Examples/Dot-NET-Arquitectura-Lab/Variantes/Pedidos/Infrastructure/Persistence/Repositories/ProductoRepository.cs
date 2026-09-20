using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Productos;

namespace MyProject.Infrastructure.Persistence.Repositories;

// Repository con seguimiento: lo que devuelve GetByIdAsync queda rastreado por el DbContext,
// y un cambio posterior en la Entity llega a la base cuando el Handler confirma con IUnitOfWork.
public class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _db;

    public ProductoRepository(AppDbContext db) => _db = db;

    public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Productos.FirstOrDefaultAsync(p => p.Id == id, ct);     // sin AsNoTracking: la Entity queda rastreada

    public Task AddAsync(Producto producto, CancellationToken ct = default)
    {
        _db.Productos.Add(producto);                                // marca para insertar; confirma IUnitOfWork
        return Task.CompletedTask;
    }
}
