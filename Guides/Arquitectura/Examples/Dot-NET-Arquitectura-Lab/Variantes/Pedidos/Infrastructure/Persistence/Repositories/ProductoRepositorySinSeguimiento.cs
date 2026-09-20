using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Productos;

namespace MyProject.Infrastructure.Persistence.Repositories;

// Contraste de §5.5: la misma interfaz con AsNoTracking, como en MyProject/.../ProductoRepository.cs.
// Vale para una Query (solo lectura); con un Use Case que carga, modifica y confirma, SaveChangesAsync
// no ve el cambio y devuelve 0 filas. El Demo lo mide.
public class ProductoRepositorySinSeguimiento : IProductoRepository
{
    private readonly AppDbContext _db;

    public ProductoRepositorySinSeguimiento(AppDbContext db) => _db = db;

    public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);   // la Entity vuelve sin rastreo

    public Task AddAsync(Producto producto, CancellationToken ct = default)
    {
        _db.Productos.Add(producto);
        return Task.CompletedTask;
    }
}
