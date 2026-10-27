using FrameworkConcept.Core.Auth;

namespace FrameworkConcept.Shell.Auth;

/// <summary>Desbloqueo con la seguridad del teléfono (P4). Ver DesbloqueoPage.xaml.</summary>
public partial class DesbloqueoPage : ContentPage
{
    private readonly ISesionDelUsuario _sesion;
    private bool _pedidoAlAparecer;
    private bool _confirmandoCierre;

    public DesbloqueoPage(ISesionDelUsuario sesion)
    {
        InitializeComponent();
        _sesion = sesion;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Saludo.Text = _sesion.Actual.Usuario is { Length: > 0 } usuario ? $"Hola, {usuario}." : "";
        // La primera vez que aparece, el diálogo del sistema se abre solo: es lo que la persona viene a hacer.
        if (!_pedidoAlAparecer)
        {
            _pedidoAlAparecer = true;
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(400), () => AlDesbloquear(null, EventArgs.Empty));
        }
    }

    // Bloqueada no se sale con atrás.
    protected override bool OnBackButtonPressed() => true;

    private async void AlDesbloquear(object? remitente, EventArgs e)
    {
        Motivo.IsVisible = false;
        try
        {
            await _sesion.DesbloquearAsync(CancellationToken.None);
        }
        catch (ExcepcionDeAutenticacion ex)
        {
            Motivo.Text = ex.Message;
            Motivo.IsVisible = true;
        }
    }

    private async void AlCerrar(object? remitente, EventArgs e)
    {
        // Dos toques: el primero explica que lo pendiente se conserva (P4).
        if (!_confirmandoCierre)
        {
            _confirmandoCierre = true;
            AvisoDeCierre.IsVisible = true;
            BotonCerrar.Text = "Sí, cerrar sesión";
            return;
        }
        await _sesion.CerrarAsync(CancellationToken.None);
    }
}
