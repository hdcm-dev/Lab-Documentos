using MyProject.Domain.Productos;

namespace MyProject.Application.Productos.Queries.ObtenerProductoPorId;

public class ObtenerProductoPorIdHandler
{
    private readonly IProductoRepository _repository;

    public ObtenerProductoPorIdHandler(IProductoRepository repository) => _repository = repository;

    public async Task<ProductoDto?> Handle(ObtenerProductoPorIdQuery query, CancellationToken ct = default)
    {
        var producto = await _repository.GetByIdAsync(query.Id, ct);
        return producto is null ? null : ProductoDto.From(producto);
    }
}
