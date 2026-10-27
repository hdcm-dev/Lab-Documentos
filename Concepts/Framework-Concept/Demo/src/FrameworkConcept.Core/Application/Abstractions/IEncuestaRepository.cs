using FrameworkConcept.Core.Domain.Entities;

namespace FrameworkConcept.Core.Application.Abstractions;

public interface IEncuestaRepository
{
    Task<int> AddAsync(RespuestaDeEncuesta respuesta, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
