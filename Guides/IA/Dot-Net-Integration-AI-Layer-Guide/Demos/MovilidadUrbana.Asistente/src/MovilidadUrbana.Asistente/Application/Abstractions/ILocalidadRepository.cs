// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Application/Abstractions/ILocalidadRepository.cs @ 8fb4c3a
// Delta: Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using MovilidadUrbana.Asistente.Domain.Entities;

namespace MovilidadUrbana.Asistente.Application.Abstractions;

public interface ILocalidadRepository
{
    Task<IReadOnlyList<Localidad>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Localidad?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> AddAsync(Localidad localidad, CancellationToken cancellationToken = default);

    Task UpdateAsync(Localidad localidad, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
