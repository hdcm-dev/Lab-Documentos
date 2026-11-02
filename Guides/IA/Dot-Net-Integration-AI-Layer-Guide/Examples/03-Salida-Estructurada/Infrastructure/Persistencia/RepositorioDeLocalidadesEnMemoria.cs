// Nuevo, escrito para el ejemplo: repositorio en memoria (el ejemplo no usa base). La única forma de
// escribir es LocalidadService.SaveAsync, que valida antes; Altas cuenta lo escrito para el invariante
// «ninguna escritura sin las reglas».
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Domain.Entities;

namespace MovilidadUrbana.Asistente.Infrastructure.Persistencia;

public sealed class RepositorioDeLocalidadesEnMemoria : ILocalidadRepository
{
    private readonly List<Localidad> _items = [];

    public IReadOnlyList<Localidad> Altas => _items;

    public Task<IReadOnlyList<Localidad>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Localidad>>([.. _items]);

    public Task<Localidad?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(l => l.Id == id));

    public Task<int> AddAsync(Localidad localidad, CancellationToken cancellationToken = default)
    {
        localidad.Id = _items.Count + 1;
        _items.Add(localidad);
        return Task.FromResult(localidad.Id);
    }

    public Task UpdateAsync(Localidad localidad, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _items.RemoveAll(l => l.Id == id);
        return Task.CompletedTask;
    }
}
