using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;

namespace FrameworkConcept.Shell.Auth;

/// <summary>Ingreso con la cuenta (P4). Ver IngresoPage.xaml.</summary>
public partial class IngresoPage : ContentPage
{
    private readonly ISesionDelUsuario _sesion;
    private readonly IGuardiaDeAcciones _guardia;
    private string? _error;

    public IngresoPage(ISesionDelUsuario sesion, IGuardiaDeAcciones guardia)
    {
        InitializeComponent();
        _sesion = sesion;
        _guardia = guardia;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _sesion.Cambio += AlCambiar;
        _guardia.Cambio += AlCambiarAcciones;
        Refrescar();
    }

    protected override void OnDisappearing()
    {
        _sesion.Cambio -= AlCambiar;
        _guardia.Cambio -= AlCambiarAcciones;
        base.OnDisappearing();
    }

    // Sin cuenta no hay adónde volver: atrás no cierra esta pantalla.
    protected override bool OnBackButtonPressed() => true;

    private void AlCambiar(SesionInformada _) => MainThread.BeginInvokeOnMainThread(Refrescar);

    private void AlCambiarAcciones(IReadOnlyList<EvaluacionDeAccion> _) => MainThread.BeginInvokeOnMainThread(Refrescar);

    private void Refrescar()
    {
        var sesion = _sesion.Actual;
        var ingresando = sesion.Estado == EstadoDeSesion.Ingresando;
        var evaluacion = _guardia.Evaluar(PoliticaDeAcciones.Ingresar);

        Explicacion.Text = ingresando
            ? "Completá el ingreso en el navegador…"
            : sesion.Motivo is { Length: > 0 } motivoDeSesion
                ? motivoDeSesion
                : "Ingresá con tu cuenta. Se abre el navegador del teléfono; al terminar, volvés acá.";
        BotonIngresar.IsEnabled = !ingresando && evaluacion.Permitida;
        Ocupado.IsVisible = Ocupado.IsRunning = ingresando;

        // Borrador §7 «botón deshabilitado + motivo»; si el último intento falló, por qué.
        var motivo = !evaluacion.Permitida && !ingresando ? evaluacion.Motivo : _error;
        Motivo.Text = motivo;
        Motivo.IsVisible = !string.IsNullOrEmpty(motivo);
    }

    private async void AlIngresar(object? remitente, EventArgs e)
    {
        _error = null;
        try
        {
            await _sesion.IngresarAsync(CancellationToken.None);
        }
        catch (ExcepcionDeAutenticacion ex)
        {
            _error = ex.Message;
        }
        Refrescar();
    }
}
