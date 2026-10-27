using FrameworkConcept.Core.Update;
using FrameworkConcept.Shell.Actualizacion;

namespace FrameworkConcept.Shell.Pages;

/// <summary>Por qué el cascarón no puede mostrar el contenido.</summary>
public enum MotivoTerminal
{
    /// <summary>P2 — Ni current ni previous están sanos. No se vuelve a la semilla.</summary>
    SinContenidoSano,

    /// <summary>P2 — Política de soporte: este cascarón está bajo la versión mínima soportada.</summary>
    CascaronNoSoportado,

    /// <summary>C-1 — La base del teléfono tiene otro número de esquema.</summary>
    EsquemaIncompatible,

    /// <summary>Un error inesperado del arranque.</summary>
    Error
}

public partial class EstadoTerminalPage : ContentPage
{
    private readonly ConsultasDelCanal _consultas;
    private MotivoTerminal _motivo;

    public EstadoTerminalPage(ConsultasDelCanal consultas)
    {
        InitializeComponent();
        _consultas = consultas;
    }

    public EstadoTerminalPage Configurar(MotivoTerminal motivo, string detalle)
    {
        _motivo = motivo;
        (Titulo.Text, Explicacion.Text) = motivo switch
        {
            MotivoTerminal.SinContenidoSano => ("No hay contenido para mostrar",
                "Las versiones instaladas de las pantallas fallaron o fueron retiradas. La app busca una versión nueva cuando hay red. Tus datos y las encuestas sin enviar siguen guardados."),
            MotivoTerminal.CascaronNoSoportado => ("Actualizá la app",
                "Esta versión de la app ya no está soportada. Instalá la versión nueva. Tus datos y las encuestas sin enviar siguen guardados."),
            MotivoTerminal.EsquemaIncompatible => ("La app no puede abrir sus datos",
                "La base de datos del teléfono es de otra versión de la app. Reinstalá la versión que la creó o una más nueva."),
            _ => ("Algo falló al arrancar", "Cerrá la app y volvé a abrirla. Si sigue pasando, mirá el diagnóstico.")
        };
        Detalle.Text = detalle;
        BotonReintentar.IsVisible = motivo == MotivoTerminal.SinContenidoSano;
        Bitacora.Log($"pantalla EstadoTerminal: {motivo}");

        if (motivo == MotivoTerminal.SinContenidoSano)
        {
            _consultas.Terminada += AlTerminarConsulta;
            Connectivity.Current.ConnectivityChanged += AlCambiarLaRed;   // P2 — reintenta la descarga cuando hay red
        }
        return this;
    }

    private void AlCambiarLaRed(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
            _consultas.ConsultarEnSegundoPlano(DisparadorDeConsulta.ArranqueEnFrio);
    }

    private void AlReintentar(object? sender, EventArgs e)
    {
        EstadoDelReintento.Text = "Buscando una versión nueva…";
        _consultas.ConsultarEnSegundoPlano(DisparadorDeConsulta.ArranqueEnFrio);
    }

    private void AlTerminarConsulta(ResultadoDeConsulta resultado) => MainThread.BeginInvokeOnMainThread(() =>
    {
        EstadoDelReintento.Text = $"Última búsqueda: {resultado.Estado}. {resultado.Detalle}";
        if (resultado.Estado != EstadoDeConsulta.Preparado || resultado.ContenidoPreparado is null) return;

        // Hay una versión verificada en staging/. Como no se está mostrando ningún contenido, volver a correr el
        // arranque equivale a un arranque en frío: la regla «activar sólo antes de mostrar» se cumple igual (P2).
        _consultas.Terminada -= AlTerminarConsulta;
        Connectivity.Current.ConnectivityChanged -= AlCambiarLaRed;
        App.MostrarRaiz(App.Pantalla<ArranquePage>());
    });

    private async void AlAbrirDiagnostico(object? sender, EventArgs e) =>
        await Navigation.PushAsync(App.Pantalla<DiagnosticoPage>());
}
