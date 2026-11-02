// Nuevo (ejemplo 14). Sin base: el ejemplo compara anotaciones, no persistencia. Un ILocalidadRepository
// en memoria con las localidades de una sesión: las dos que siembra SessionSeeder (Corrientes y
// Resistencia: SessionSeeder.cs @ 8fb4c3a) y Goya y Bella Vista con los valores del caso base
// (LocalidadesTests.cs l. 50 y l. 102 @ 8fb4c3a). Sólo lectura: el ejemplo no escribe.
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Domain.Entities;

namespace MovilidadUrbana.Asistente.Infrastructure.Persistence;

public sealed class LocalidadesEnMemoria : ILocalidadRepository
{
    private const string Motivo = "El ejemplo 14 sólo lee: compara cómo se anota una herramienta.";

    public const string Sesion = "ejemplo-14";

    private readonly IReadOnlyList<Localidad> _localidades =
    [
        new() { Id = 1, SessionId = Sesion, Nombre = "Corrientes", Provincia = "Corrientes", CodigoPostal = "3400", Habitantes = 346334 },
        new() { Id = 2, SessionId = Sesion, Nombre = "Resistencia", Provincia = "Chaco", CodigoPostal = "3500", Habitantes = 291720 },
        new() { Id = 3, SessionId = Sesion, Nombre = "Goya", Provincia = "Corrientes", CodigoPostal = "3450", Habitantes = 90000 },
        new() { Id = 4, SessionId = Sesion, Nombre = "Bella Vista", Provincia = "Corrientes", CodigoPostal = "3432", Habitantes = 30000 }
    ];

    public Task<IReadOnlyList<Localidad>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(_localidades);

    public Task<Localidad?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_localidades.FirstOrDefault(l => l.Id == id));

    public Task<int> AddAsync(Localidad localidad, CancellationToken cancellationToken = default) => throw new NotSupportedException(Motivo);

    public Task UpdateAsync(Localidad localidad, CancellationToken cancellationToken = default) => throw new NotSupportedException(Motivo);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) => throw new NotSupportedException(Motivo);
}
