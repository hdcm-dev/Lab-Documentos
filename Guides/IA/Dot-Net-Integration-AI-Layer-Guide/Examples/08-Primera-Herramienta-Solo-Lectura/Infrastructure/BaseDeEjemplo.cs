// Nuevo: la base SQLite del ejemplo, con la persistencia de _Comun tal cual (repositorios acotados a
// la sesión y SessionSeeder). Cada sesión es un scope de DI con su SessionContext puesto por el
// CÓDIGO: el modelo nunca elige ni ve el id de sesión (§8.3, PR-20).
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;
using MovilidadUrbana.Asistente.Infrastructure.Sessions;

namespace MovilidadUrbana.Asistente.Infrastructure;

public sealed class BaseDeEjemplo : IDisposable
{
    /// <summary>La sesión de la persona que pregunta.</summary>
    public const string SesionPropia = "ejemplo-08-a";

    /// <summary>Otra sesión, con una localidad más (Goya), que la propia nunca tiene que alcanzar.</summary>
    public const string SesionAjena = "ejemplo-08-b";

    private readonly string _archivo;
    private readonly ServiceProvider _servicios;

    private BaseDeEjemplo(string archivo, ServiceProvider servicios)
    {
        _archivo = archivo;
        _servicios = servicios;
    }

    /// <summary>
    /// Crea la base en un archivo temporal, siembra las dos sesiones y agrega Goya a la ajena
    /// (valores del caso base: LocalidadesTests.cs l. 50 @ 8fb4c3a).
    /// </summary>
    public static async Task<BaseDeEjemplo> CrearAsync(CancellationToken cancellationToken = default)
    {
        var archivo = Path.Combine(Path.GetTempPath(), $"ej08-{Guid.NewGuid():n}.db");
        var servicios = new ServiceCollection()
            .AddInfrastructure($"Data Source={archivo};Default Timeout=30")
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        DatabaseInitializer.Initialize(servicios);

        var bd = new BaseDeEjemplo(archivo, servicios);
        using var ajena = bd.AbrirSesion(SesionAjena);
        var alta = await ajena.Localidades.SaveAsync(
            new LocalidadModel { Nombre = "Goya", Provincia = "Corrientes", CodigoPostal = "3450", Habitantes = 90000 },
            cancellationToken);
        if (!alta.IsSuccess) throw new InvalidOperationException($"No se pudo sembrar la sesión ajena: {alta.Message}");
        return bd;
    }

    /// <summary>Un scope con la sesión fijada por el código y el caso de uso resuelto en él.</summary>
    public Sesion AbrirSesion(string id)
    {
        var alcance = _servicios.CreateScope();
        alcance.ServiceProvider.GetRequiredService<SessionContext>().Set(id);
        var servicio = new LocalidadService(alcance.ServiceProvider.GetRequiredService<ILocalidadRepository>());
        return new Sesion(id, servicio, alcance);
    }

    public void Dispose()
    {
        _servicios.Dispose();
        SqliteConnection.ClearAllPools();
        foreach (var sufijo in new[] { "", "-wal", "-shm" })
        {
            try { File.Delete(_archivo + sufijo); } catch (IOException) { }
        }
    }
}

public sealed class Sesion(string id, LocalidadService localidades, IServiceScope alcance) : IDisposable
{
    public string Id { get; } = id;

    public LocalidadService Localidades { get; } = localidades;

    public void Dispose() => alcance.Dispose();
}
