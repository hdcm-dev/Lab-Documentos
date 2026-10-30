using FrameworkConcept.Shell.Hosting;
using FrameworkConcept.Shell.Sync;

namespace FrameworkConcept.Shell.Pages;

/// <summary>
/// La pantalla de contenido del arranque de siempre (sin E04): monta una <see cref="VistaDelContenido"/> con la versión
/// que decidió el arranque y conserva el aviso de carta muerta (P5) y el diagnóstico. Con E04 el contenido lo hospeda
/// la pantalla de arranque y esta no se apila.
/// </summary>
public partial class ContenidoPage : ContentPage
{
    public ContenidoPage(ContenidoActivo contenido, ServidorDeContenido servidor, VistaDelContenido vista,
        SincronizadorDelOutbox sincronizador)
    {
        InitializeComponent();
        Title = $"Framework Concept · contenido {contenido.Version}";

        servidor.Raiz = contenido.Directorio;
        Contenedor.Children.Add(vista.Vista);
        Bitacora.Log($"contenido: hybridwebview agregado ({contenido.Version})");
        Bitacora.Log($"arranque: HybridWebView adjunto (especulada ninguna)");

        sincronizador.ConteoCambiado += AlCambiarElOutbox;
    }

    // P5 — La carta muerta se muestra al usuario.
    private void AlCambiarElOutbox(int pendientes, int rechazadas) => MainThread.BeginInvokeOnMainThread(() =>
    {
        AvisoCartaMuerta.IsVisible = rechazadas > 0;
        TextoCartaMuerta.Text = $"{rechazadas} respuesta(s) no se pudieron enviar y quedaron guardadas. Tocá para ver el detalle.";
    });

    private async void AlAbrirDiagnostico(object? sender, EventArgs e) =>
        await Navigation.PushAsync(App.Pantalla<DiagnosticoPage>());
}
