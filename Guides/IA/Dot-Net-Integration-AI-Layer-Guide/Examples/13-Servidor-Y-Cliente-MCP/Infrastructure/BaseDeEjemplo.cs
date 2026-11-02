// Nuevo: la base SQLite del ejemplo, con la persistencia de _Comun tal cual (repositorios acotados a
// la sesión y SessionSeeder). Hay dos sesiones: la fija «demo-mcp», la única que alcanza el servidor
// MCP, y una que hace de sesión de la API, con una localidad más (Goya). La sesión la pone el
// CÓDIGO del servidor: ningún pedido MCP trae un id de sesión (H-014).
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;
using MovilidadUrbana.Asistente.Infrastructure.Sessions;

namespace MovilidadUrbana.Asistente.Infrastructure;

public sealed class BaseDeEjemplo : IDisposable
{
    /// <summary>La sesión fija del servidor MCP, declarada (plan §E.2, fila 13).</summary>
    public const string SesionMcp = "demo-mcp";

    /// <summary>Hace de sesión de la API: tiene Goya, que por MCP nunca tiene que aparecer.</summary>
    public const string SesionDeLaApi = "ejemplo-13-api";

    /// <summary>Las localidades que tiene la sesión fija: las 2 sembradas del caso base.</summary>
    public static readonly string[] LocalidadesDeLaSesionMcp = ["Corrientes", "Resistencia"];

    /// <summary>Las que tiene sólo la sesión de la API.</summary>
    public static readonly string[] LocalidadesSoloDeLaApi = ["Goya"];

    private readonly string _archivo;
    private readonly ServiceProvider _servicios;

    private BaseDeEjemplo(string archivo, ServiceProvider servicios)
    {
        _archivo = archivo;
        _servicios = servicios;
    }

    /// <summary>
    /// Crea la base en un archivo temporal, siembra las dos sesiones y agrega Goya a la de la API
    /// (valores del caso base: LocalidadesTests.cs l. 50 @ 8fb4c3a).
    /// </summary>
    public static async Task<BaseDeEjemplo> CrearAsync(CancellationToken cancellationToken = default)
    {
        var archivo = Path.Combine(Path.GetTempPath(), $"ej13-{Guid.NewGuid():n}.db");
        var servicios = new ServiceCollection()
            .AddInfrastructure($"Data Source={archivo};Default Timeout=30")
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        DatabaseInitializer.Initialize(servicios);

        var bd = new BaseDeEjemplo(archivo, servicios);
        using (var mcp = bd.AbrirSesion(SesionMcp))
        {
            await mcp.Localidades.GetAllAsync(cancellationToken); // siembra la sesión fija
        }
        using var api = bd.AbrirSesion(SesionDeLaApi);
        var alta = await api.Localidades.SaveAsync(
            new LocalidadModel { Nombre = "Goya", Provincia = "Corrientes", CodigoPostal = "3450", Habitantes = 90000 },
            cancellationToken);
        if (!alta.IsSuccess) throw new InvalidOperationException($"No se pudo sembrar la sesión de la API: {alta.Message}");
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
