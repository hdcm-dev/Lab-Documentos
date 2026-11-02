// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Application/Abstractions/IEncuestaRepository.cs @ 8fb4c3a
// Delta: Modificado a causa de la IA — GetAllAsync para ResultadosDeEncuestaService; Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using MovilidadUrbana.Asistente.Domain.Entities;

namespace MovilidadUrbana.Asistente.Application.Abstractions;

public interface IEncuestaRepository
{
    Task<int> AddAsync(RespuestaDeEncuesta respuesta, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Nuevo: las respuestas de la sesión, para los agregados de ResultadosDeEncuestaService.</summary>
    Task<IReadOnlyList<RespuestaDeEncuesta>> GetAllAsync(CancellationToken cancellationToken = default);
}
