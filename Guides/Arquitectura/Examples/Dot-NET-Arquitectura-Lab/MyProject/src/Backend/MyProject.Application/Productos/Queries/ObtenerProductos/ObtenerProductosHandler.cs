using MyProject.Domain.Productos;

namespace MyProject.Application.Productos.Queries.ObtenerProductos;

public class ObtenerProductosHandler
{
    private readonly IProductoRepository _repository;

    public ObtenerProductosHandler(IProductoRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ProductoDto>> Handle(ObtenerProductosQuery query, CancellationToken ct = default)
    {
        var productos = await _repository.GetAllAsync(ct);
        return productos.Select(ProductoDto.From).ToList();
    }
}
