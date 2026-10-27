using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Core.Update;
using FrameworkConcept.Shell.Actualizacion;
using FrameworkConcept.Shell.Hosting;
using FrameworkConcept.Shell.Pages;
using FrameworkConcept.Shell.Sync;

namespace FrameworkConcept.Shell;

/// <summary>
/// La aplicación del cascarón. Arranca siempre por su pantalla XAML de arranque (P1): nunca abre el contenido sin que
/// el actualizador haya decidido antes qué versión mostrar.
/// </summary>
public sealed class App(IServiceProvider servicios) : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var ventana = new Window(Navegacion(servicios.GetRequiredService<ArranquePage>()));

        // P2 — Volver desde segundo plano: consulta en segundo plano (el actualizador la omite si no pasaron 15
        // minutos desde la última exitosa) y una pasada del outbox. MAUI llama a Resumed sólo al volver, no al abrir.
        // Folio 038, P4 — Pasar a segundo plano anota la hora; volver después de X minutos bloquea la entrada local.
        ventana.Stopped += (_, _) => servicios.GetRequiredService<ISesionDelUsuario>().AlPasarASegundoPlano();

        ventana.Resumed += (_, _) =>
        {
            Bitacora.Log("app: reanudada");
            servicios.GetRequiredService<ISesionDelUsuario>().AlVolverAlFrente();
            servicios.GetRequiredService<IMonitorDeRed>().Reevaluar();   // borrador §6 — chequeo de salud al volver
            if (servicios.GetRequiredService<ContenidoActivo>().Version is not null)
                servicios.GetRequiredService<ConsultasDelCanal>().ConsultarEnSegundoPlano(DisparadorDeConsulta.Reanudacion);
            servicios.GetRequiredService<SincronizadorDelOutbox>().Despertar();
        };
        return ventana;
    }

    /// <summary>Reemplaza la pantalla raíz (el arranque decide a cuál ir y no se vuelve atrás).</summary>
    public static void MostrarRaiz(Page pagina)
    {
        if (Current?.Windows.FirstOrDefault() is { } ventana)
            ventana.Page = Navegacion(pagina);
    }

    // Texto blanco en la barra: el color de fondo por defecto de MAUI es oscuro y el título no se leía.
    private static NavigationPage Navegacion(Page raiz) => new(raiz) { BarTextColor = Colors.White };

    /// <summary>Una pantalla nueva, creada por el contenedor de servicios.</summary>
    public static T Pantalla<T>() where T : Page =>
        IPlatformApplication.Current!.Services.GetRequiredService<T>();
}
