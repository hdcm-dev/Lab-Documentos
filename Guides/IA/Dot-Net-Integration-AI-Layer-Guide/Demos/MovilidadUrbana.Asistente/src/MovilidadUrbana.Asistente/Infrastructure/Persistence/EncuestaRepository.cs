// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Infrastructure/Persistence/EncuestaRepository.cs @ 8fb4c3a
// Delta: Modificado a causa de la IA — GetAllAsync; Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using Microsoft.EntityFrameworkCore;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Domain.Entities;

namespace MovilidadUrbana.Asistente.Infrastructure.Persistence;

public sealed class EncuestaRepository(
    IDbContextFactory<AppDbContext> factory,
    ISessionContext session,
    SessionSeeder seeder) : IEncuestaRepository
{
    public async Task<int> AddAsync(RespuestaDeEncuesta respuesta, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        respuesta.SessionId = session.Id;
        context.Encuestas.Add(respuesta);
        await context.SaveChangesAsync(cancellationToken);
        return respuesta.Id;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        return await context.Encuestas.CountAsync(e => e.SessionId == session.Id, cancellationToken);
    }

    // Nuevo: lectura acotada a la sesión, como las demás. Siembra antes de leer, igual que LocalidadRepository.
    public async Task<IReadOnlyList<RespuestaDeEncuesta>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await seeder.EnsureSeededAsync(cancellationToken);
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        return await context.Encuestas
            .AsNoTracking()
            .Where(e => e.SessionId == session.Id)
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }
}
