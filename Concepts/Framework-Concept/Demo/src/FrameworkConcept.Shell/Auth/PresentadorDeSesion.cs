using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;

namespace FrameworkConcept.Shell.Auth;

/// <summary>
/// P4, borrador §3 «pantallas mínimas propias (arranque, login, error)» — Pone las pantallas XAML del cascarón encima del
/// contenido cuando la sesión lo pide, y las saca cuando ya no:
///
///   SinCuenta, Ingresando (modo Oidc)  → IngresoPage
///   Bloqueada                          → DesbloqueoPage
///   Activa, Vencida, Revocada          → ninguna: se trabaja en el contenido (Vencida y Revocada muestran una banda de
///                                        reingreso en el contenido, Content/Auth/PuertaDeSesion.razor; P4 «sin red se opera»)
///
/// Sólo actúa después de que el contenido está a la vista (lo marca el primer auth.status): antes, el arranque todavía
/// está decidiendo qué mostrar (P1) y cambiar la pantalla raíz descartaría estas pantallas modales.
/// </summary>
public static class PresentadorDeSesion
{
    private static bool _contenidoVisible;

    /// <summary>El contenido pidió auth.status: ya está a la vista.</summary>
    public static void ContenidoVisible(SesionInformada actual)
    {
        _contenidoVisible = true;
        Actualizar(actual);
    }

    public static void Actualizar(SesionInformada sesion)
    {
        if (!_contenidoVisible || sesion.Modo != ModoDeSesion.Oidc) return;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await AplicarAsync(sesion);
            }
            catch (Exception ex)
            {
                Bitacora.Error("presentador de sesión", ex);
            }
        });
    }

    private static async Task AplicarAsync(SesionInformada sesion)
    {
        if (Application.Current?.Windows.FirstOrDefault()?.Page is not { } raiz) return;
        var navegacion = raiz.Navigation;
        // Cada pantalla propia va dentro de su NavigationPage (para el título): se mira la página de adentro.
        var modal = navegacion.ModalStack.LastOrDefault();
        var arriba = modal is NavigationPage envoltorio ? envoltorio.RootPage : modal;

        Type? buscada = sesion.Estado switch
        {
            EstadoDeSesion.SinCuenta or EstadoDeSesion.Ingresando => typeof(IngresoPage),
            EstadoDeSesion.Bloqueada => typeof(DesbloqueoPage),
            _ => null
        };
        if (arriba?.GetType() == buscada) return;

        if (arriba is IngresoPage or DesbloqueoPage) await navegacion.PopModalAsync(animated: false);
        if (buscada is null) return;

        var servicios = IPlatformApplication.Current!.Services;
        var sesionDelUsuario = servicios.GetRequiredService<ISesionDelUsuario>();
        Page pagina = buscada == typeof(IngresoPage)
            ? new IngresoPage(sesionDelUsuario, servicios.GetRequiredService<IGuardiaDeAcciones>())
            : new DesbloqueoPage(sesionDelUsuario);
        Bitacora.Log($"sesión: pantalla propia {buscada.Name}");
        await navegacion.PushModalAsync(new NavigationPage(pagina) { BarTextColor = Colors.White }, animated: false);
    }
}
