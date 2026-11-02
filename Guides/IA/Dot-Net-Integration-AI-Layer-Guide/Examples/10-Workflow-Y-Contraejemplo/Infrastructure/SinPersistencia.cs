// Nuevo (ejemplo 10). El ejemplo sólo valida (ValidarPaso); no registra encuestas. EncuestaService pide
// un repositorio en el constructor, y éste deja explícito que nadie lo usa.
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Domain.Entities;

namespace MovilidadUrbana.Asistente.Infrastructure;

public sealed class SinPersistencia : IEncuestaRepository
{
    private const string Motivo = "El ejemplo 10 no persiste: sólo valida los pasos.";

    public Task<int> AddAsync(RespuestaDeEncuesta respuesta, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(Motivo);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException(Motivo);

    public Task<IReadOnlyList<RespuestaDeEncuesta>> GetAllAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(Motivo);
}
