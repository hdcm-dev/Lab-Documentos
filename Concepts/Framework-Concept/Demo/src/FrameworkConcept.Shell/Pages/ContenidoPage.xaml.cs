using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Update;
using FrameworkConcept.Shell.Actualizacion;
using FrameworkConcept.Shell.Bridge;
using FrameworkConcept.Shell.Hosting;
using FrameworkConcept.Shell.Sync;

namespace FrameworkConcept.Shell.Pages;

public partial class ContenidoPage : ContentPage
{
    /// <summary>Si el contenido no avisa «Listo» en este tiempo, la consulta del arranque en frío se lanza igual.</summary>
    private static readonly TimeSpan EsperaMaximaDelListo = TimeSpan.FromSeconds(10);

    private readonly ConsultasDelCanal _consultas;
    private readonly DespachadorDelPuente _despachador;
    private readonly SincronizadorDelOutbox _sincronizador;
    private readonly ProveedorDelActualizador _proveedor;
    private readonly ContenidoActivo _contenido;
    private int _consultaLanzada;

    public ContenidoPage(ContenidoActivo contenido, ServidorDeContenido servidor, PuenteDelCascaron puente,
        EmisorDeEventos emisor, DespachadorDelPuente despachador, ConsultasDelCanal consultas, SincronizadorDelOutbox sincronizador,
        ProveedorDelActualizador proveedor)
    {
        InitializeComponent();
        _consultas = consultas;
        _despachador = despachador;
        _sincronizador = sincronizador;
        _proveedor = proveedor;
        _contenido = contenido;
        Title = $"Framework Concept · contenido {contenido.Version}";

        // C-1 de E01 — un único HybridWebView; el contenido lo sirve WebResourceRequested desde content/versions/vN.
        // HybridRoot apunta a una página de reserva del APK («hwv»): nunca se usa para el contenido descargado.
        servidor.Raiz = contenido.Directorio;
        servidor.CargaIniciada += AlIniciarLaCarga;
        var vista = new HybridWebView { HybridRoot = "hwv", DefaultFile = "index.html" };
        vista.WebResourceRequested += servidor.AlPedirRecurso;
        vista.SetInvokeJavaScriptTarget(puente);   // P3 — un solo objeto, un solo método: Call(sobre)
        emisor.Conectar(vista);
        Contenedor.Children.Add(vista);
        Bitacora.Log($"contenido: hybridwebview agregado ({contenido.Version})");

        _despachador.ListoRecibido += AlRecibirListo;
        _sincronizador.ConteoCambiado += AlCambiarElOutbox;
        Dispatcher.StartTimer(EsperaMaximaDelListo, () =>
        {
            LanzarConsulta("sin Listo a los 10 s");
            return false;
        });
    }

    private void AlRecibirListo(VersionSemantica version) => LanzarConsulta("tras Listo");

    // P2 — El WebView empezó a cargar el contenido: recién ahora este arranque cuenta para el rollback por marca.
    private void AlIniciarLaCarga()
    {
        if (_contenido.Version is not { } version) return;
        _ = Task.Run(async () =>
        {
            try
            {
                await (await _proveedor.ObtenerAsync()).AnotarCargaIniciadaAsync(version);
                Bitacora.Log($"contenido: carga iniciada ({version})");
            }
            catch (Exception ex)
            {
                Bitacora.Error("contenido: anotar la carga iniciada", ex);
            }
        });
    }

    // P2 — La consulta del arranque en frío, una vez y fuera del camino crítico del primer render (P1).
    private void LanzarConsulta(string motivo)
    {
        if (Interlocked.Exchange(ref _consultaLanzada, 1) == 1) return;
        Bitacora.Log($"contenido: consulta del arranque en frío ({motivo})");
        _consultas.ConsultarEnSegundoPlano(DisparadorDeConsulta.ArranqueEnFrio);
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
