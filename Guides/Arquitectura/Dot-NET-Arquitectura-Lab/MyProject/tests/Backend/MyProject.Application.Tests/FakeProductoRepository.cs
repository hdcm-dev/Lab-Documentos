using MyProject.Domain.Productos;

namespace MyProject.Application.Tests;

/// <summary>Doble de prueba: guarda en una lista. Vive en el proyecto de tests, no en Infrastructure.</summary>
public class FakeProductoRepository : IProductoRepository
{
    public List<Producto> Guardados { get; } = new();

    public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Guardados.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Producto>>(Guardados);

    public Task AddAsync(Producto producto, CancellationToken ct = default)
    {
        Guardados.Add(producto);
        return Task.CompletedTask;
    }
}
