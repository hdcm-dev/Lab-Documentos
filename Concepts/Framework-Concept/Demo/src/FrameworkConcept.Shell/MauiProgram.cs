using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Shell.Actualizacion;
using FrameworkConcept.Shell.Auth;
using FrameworkConcept.Shell.Bridge;
using FrameworkConcept.Shell.Data;
using FrameworkConcept.Shell.Hosting;
using FrameworkConcept.Shell.Pages;
using FrameworkConcept.Shell.Red;
using FrameworkConcept.Shell.Sync;

namespace FrameworkConcept.Shell;

/// <summary>
/// Composición del cascarón: qué objeto usa cada pieza. Casi todo es singleton porque hay una sola de cada cosa por
/// proceso (una base, un actualizador, un puente, un HybridWebView). Las pantallas se crean cada vez.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        var s = builder.Services;

        s.AddSingleton<ConfiguracionDelCascaron>();

        // P5 — Datos: EF Core SQLite detrás de IDb, en los datos privados de la app.
        s.AddSingleton<IDb>(_ => new DbSqlite(Path.Combine(FileSystem.AppDataDirectory, EsquemaDelCascaron.NombreDelArchivo)));
        s.AddSingleton<AlmacenDeDefinicionesSqlite>();

        // P2 — Actualizador (Core) con sus adaptadores de Android.
        s.AddSingleton<MotorWeb>();
        s.AddSingleton<EntornoDelCascaron>();
        s.AddSingleton<ClienteDeCanalHttp>();
        s.AddSingleton<ProveedorDelActualizador>();
        s.AddSingleton<ConsultasDelCanal>();

        // P1, P3 — Servidor de contenido, puente y eventos.
        s.AddSingleton<ContenidoActivo>();
        s.AddSingleton<ServidorDeContenido>();
        s.AddSingleton<EmisorDeEventos>();
        s.AddSingleton<DespachadorDelPuente>();
        s.AddSingleton<PuenteDelCascaron>();

        // Folio 038, borrador §6–§7 — Red combinada y política de acciones (Shell/Red/, DISENO §2 y §4).
        s.AddSingleton<IMonitorDeRed, MonitorDeRed>();
        s.AddSingleton<IGuardiaDeAcciones, GuardiaDeAcciones>();
        s.AddSingleton<AtencionDeRed>();

        // Folio 038, P4 — Sesión: OIDC + PKCE y desbloqueo con la seguridad del teléfono (Shell/Auth/, DISENO §3).
        s.AddSingleton<CofreDeSesion>();
        s.AddSingleton<ISesionDelUsuario, SesionDelUsuario>();
        s.AddSingleton<AtencionDeSesion>();

        // Borrador §6 «Actividad» — Todo HttpClient del framework sale de la fábrica, con sus DelegatingHandler.
        s.AddSingleton<FabricaHttp>();

        // P5 — Outbox y sincronización.
        s.AddSingleton<IdentidadDelDispositivo>();
        s.AddSingleton<ClienteDelReceptor>();
        s.AddSingleton<SincronizadorDelOutbox>();

        // Pantallas XAML propias del cascarón (P1).
        s.AddTransient<ArranquePage>();
        s.AddTransient<ContenidoPage>();
        s.AddTransient<WebViewViejoPage>();
        s.AddTransient<EstadoTerminalPage>();
        s.AddTransient<DiagnosticoPage>();

        return builder.Build();
    }
}
