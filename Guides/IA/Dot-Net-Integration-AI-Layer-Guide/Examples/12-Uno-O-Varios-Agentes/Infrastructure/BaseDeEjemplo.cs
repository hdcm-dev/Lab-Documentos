// Nuevo: la base SQLite del ejemplo, con la persistencia de _Comun tal cual. La sesión la fija el
// CÓDIGO (un scope de DI con su SessionContext) y SessionSeeder la siembra la primera vez con las 20
// encuestas rotuladas «escrito para el ejemplo» (SiembraDeEncuestas, plan §F.2; H-062).
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;
using MovilidadUrbana.Asistente.Infrastructure.Sessions;

namespace MovilidadUrbana.Asistente.Infrastructure;

public sealed class BaseDeEjemplo : IDisposable
{
    public const string Sesion = "ejemplo-12";

    private readonly string _archivo;
    private readonly ServiceProvider _servicios;
    private readonly IServiceScope _alcance;

    private BaseDeEjemplo(string archivo, ServiceProvider servicios)
    {
        _archivo = archivo;
        _servicios = servicios;
        _alcance = servicios.CreateScope();
        _alcance.ServiceProvider.GetRequiredService<SessionContext>().Set(Sesion);
        Resultados = new ResultadosDeEncuestaService(_alcance.ServiceProvider.GetRequiredService<IEncuestaRepository>());
    }

    /// <summary>El caso de uso de lectura de _Comun, ya en la sesión del ejemplo.</summary>
    public ResultadosDeEncuestaService Resultados { get; }

    /// <summary>Crea la base en un archivo temporal; se borra en <see cref="Dispose" />.</summary>
    public static BaseDeEjemplo Crear()
    {
        var archivo = Path.Combine(Path.GetTempPath(), $"ej12-{Guid.NewGuid():n}.db");
        var servicios = new ServiceCollection()
            .AddInfrastructure($"Data Source={archivo};Default Timeout=30")
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        DatabaseInitializer.Initialize(servicios);
        return new BaseDeEjemplo(archivo, servicios);
    }

    public void Dispose()
    {
        _alcance.Dispose();
        _servicios.Dispose();
        SqliteConnection.ClearAllPools();
        foreach (var sufijo in new[] { "", "-wal", "-shm" })
        {
            try { File.Delete(_archivo + sufijo); } catch (IOException) { }
        }
    }
}
