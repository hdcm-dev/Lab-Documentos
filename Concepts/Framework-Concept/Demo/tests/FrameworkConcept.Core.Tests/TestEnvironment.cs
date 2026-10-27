using System.Globalization;
using FrameworkConcept.Core.Application.Abstractions;
using FrameworkConcept.Core.Application.Encuestas;
using FrameworkConcept.Core.Application.Localidades;
using FrameworkConcept.Core.Domain.Entities;

namespace FrameworkConcept.Core.Tests;

/// <summary>
/// Compone los servicios reales de la aplicación sobre repositorios en memoria. Cada instancia es un
/// dispositivo nuevo, con las dos localidades que el caso base siembra en cada sesión.
///
/// Adaptado del TestEnvironment del caso base, que usaba EF Core sobre un SQLite temporal: en la demo la
/// base es del cascarón (P5) y el contenido llega a ella por el puente, así que las reglas de aplicación
/// se prueban contra la interfaz del repositorio, sin base.
/// </summary>
public sealed class TestEnvironment
{
    public TestEnvironment()
    {
        // El caso base fija es-AR al arrancar; los formatos del resumen («12,5 km») dependen de eso.
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("es-AR");
        Localidades = new LocalidadService(LocalidadesGuardadas);
        Encuestas = new EncuestaService(EncuestasGuardadas);
    }

    public InMemoryLocalidadRepository LocalidadesGuardadas { get; } = new();
    public InMemoryEncuestaRepository EncuestasGuardadas { get; } = new();

    public LocalidadService Localidades { get; }
    public EncuestaService Encuestas { get; }
}

/// <summary>Repositorio de localidades en memoria, sembrado como SessionSeeder del caso base.</summary>
public sealed class InMemoryLocalidadRepository : ILocalidadRepository
{
    private readonly List<Localidad> _localidades =
    [
        new() { Id = 1, Nombre = "Corrientes", Provincia = "Corrientes", CodigoPostal = "3400", Habitantes = 346334 },
        new() { Id = 2, Nombre = "Resistencia", Provincia = "Chaco", CodigoPostal = "3500", Habitantes = 291720 }
    ];

    private int _ultimoId = 2;

    public Task<IReadOnlyList<Localidad>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Localidad>>([.. _localidades.OrderBy(l => l.Id).Select(Copia)]);

    public Task<Localidad?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_localidades.Where(l => l.Id == id).Select(Copia).FirstOrDefault());

    public Task<int> AddAsync(Localidad localidad, CancellationToken cancellationToken = default)
    {
        localidad.Id = ++_ultimoId;
        _localidades.Add(Copia(localidad));
        return Task.FromResult(localidad.Id);
    }

    public Task UpdateAsync(Localidad localidad, CancellationToken cancellationToken = default)
    {
        var indice = _localidades.FindIndex(l => l.Id == localidad.Id);
        if (indice >= 0) _localidades[indice] = Copia(localidad);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _localidades.RemoveAll(l => l.Id == id);
        return Task.CompletedTask;
    }

    // Se devuelven copias, como hace EF con AsNoTracking: modificar lo leído no cambia lo guardado.
    private static Localidad Copia(Localidad l) => new()
    {
        Id = l.Id, SessionId = l.SessionId, Nombre = l.Nombre, Provincia = l.Provincia,
        CodigoPostal = l.CodigoPostal, Habitantes = l.Habitantes
    };
}

public sealed class InMemoryEncuestaRepository : IEncuestaRepository
{
    public List<RespuestaDeEncuesta> Guardadas { get; } = [];

    public Task<int> AddAsync(RespuestaDeEncuesta respuesta, CancellationToken cancellationToken = default)
    {
        respuesta.Id = Guardadas.Count + 1;
        Guardadas.Add(respuesta);
        return Task.FromResult(respuesta.Id);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(Guardadas.Count);
}

/// <summary>Modelos de encuesta ya completados, como los CompletarPaso1..3 del caso base.</summary>
public static class EncuestasDePrueba
{
    public static EncuestaModel Completa()
    {
        var model = new EncuestaModel { EncuestaId = "encuesta-movilidad", VersionDefinicion = 1 };
        model.Nombre = "Ana Pérez"; model.Edad = 34; model.Localidad = "Corrientes";
        model.AlternarMedio("colectivo", true); model.AlternarMedio("bicicleta", true); model.Frecuencia = "diaria";
        model.Distancia = 12.5; model.Minutos = 45; model.Motivo = "trabajo";
        return model;
    }
}
