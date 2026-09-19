using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Productos;

namespace MyProject.Infrastructure.Persistence.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _db;

    public ProductoRepository(AppDbContext db) => _db = db;

    public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Productos.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Producto producto, CancellationToken ct = default)
    {
        _db.Productos.Add(producto);        // marca la entidad para insertar
        await _db.SaveChangesAsync(ct);     // confirma la escritura en la base
    }
}
