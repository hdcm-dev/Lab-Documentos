using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Shell.Hosting;

namespace FrameworkConcept.Shell.Pages;

public partial class WebViewViejoPage : ContentPage
{
    public WebViewViejoPage(MotorWeb motor)
    {
        InitializeComponent();
        Instalada.Text = motor.Version ?? "no se pudo leer";
        Minima.Text = $"{ParametrosDeProducto.WebViewMinimo}";   // P1 — valor de producto confirmado (folio 036)
        Proveedor.Text = motor.Paquete ?? "—";
        Bitacora.Log($"pantalla WebViewViejo: instalada {motor.Version}, mínima {ParametrosDeProducto.WebViewMinimo}");
    }

    private async void AlAbrirTienda(object? sender, EventArgs e)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri("market://details?id=com.google.android.webview"));
        }
        catch (Exception ex)
        {
            Bitacora.Error("abrir Play Store", ex);
            await DisplayAlertAsync("Play Store", "No se pudo abrir Play Store en este teléfono.", "Aceptar");
        }
    }

    private async void AlAbrirDiagnostico(object? sender, EventArgs e) =>
        await Navigation.PushAsync(App.Pantalla<DiagnosticoPage>());
}
