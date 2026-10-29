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
    private readonly HybridWebView _vista;
    private int _consultaLanzada;
    private int _marcaLeida;
    private int _cargaIniciada;

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
        _vista = vista;
        vista.WebResourceRequested += servidor.AlPedirRecurso;
        vista.SetInvokeJavaScriptTarget(puente);   // P3 — un solo objeto, un solo método: Call(sobre)
        emisor.Conectar(vista);
        Contenedor.Children.Add(vista);
        Bitacora.Log($"contenido: hybridwebview agregado ({contenido.Version})");

        _despachador.ListoRecibido += AlRecibirListo;
        _sincronizador.ConteoCambiado += AlCambiarElOutbox;
        Dispatcher.StartTimer(EsperaMaximaDelListo, () =>
        {
            // P1, folio 042 — Si a los 10 s el WebView ni pidió el contenido, la actividad no llegó al frente (pantalla
            // apagada o bloqueada): no habrá «primer-render» hasta que se vea. Se deja dicho para quien lee el TPR.
            if (Volatile.Read(ref _cargaIniciada) == 0)
                Bitacora.Log("contenido: el WebView no pidió el contenido a los 10 s (¿la app no llegó al frente? pantalla apagada o bloqueada)");
            LanzarConsulta("sin Listo a los 10 s");
            return false;
        });
    }

    private void AlRecibirListo(VersionSemantica version)
    {
        LeerLaMarcaDelPrimerRender(version);
        LanzarConsulta("tras Listo");
    }

    /// <summary>JavaScript que devuelve la hora (época, ms) de la marca «primer-render», o null si no está.</summary>
    private const string HoraDeLaMarca =
        "(function(){var m=performance.getEntriesByName('primer-render');return m.length?performance.timeOrigin+m[0].startTime:null;})()";

    // P1 — La marca «primer-render» la pone el contenido (puente.js) y la escribe en la consola del WebView, que un
    // teléfono real no pasa a logcat (moto g42, constancia 047). Al llegar «Listo» (el contenido marca antes de
    // avisar), el cascarón lee la hora de la marca y la escribe en su propia bitácora, con el TPR contado desde el
    // inicio del proceso. No pasa por el contrato del puente: es medición, no un método del contenido.
    private void LeerLaMarcaDelPrimerRender(VersionSemantica version)
    {
        if (Interlocked.Exchange(ref _marcaLeida, 1) == 1) return;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                var texto = (await _vista.EvaluateJavaScriptAsync(HoraDeLaMarca))?.Trim('"');
                if (!double.TryParse(texto, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var hora))
                {
                    Bitacora.Log($"contenido: el WebView no tiene la marca primer-render ({version})");
                    return;
                }
                var tpr = (long)Math.Round(hora - InicioDelProcesoEnEpocaMs());
                Bitacora.Log($"{TextoDeLaMarca} contenido={version} tpr={tpr}ms (leída por el cascarón)");
            }
            catch (Exception ex)
            {
                Bitacora.Error("contenido: leer la marca primer-render", ex);
            }
        });
    }

    /// <summary>Texto de la marca en la bitácora: el mismo que escribe puente.js (lo lee «tpr», Tools/LectorDeTpr).</summary>
    private const string TextoDeLaMarca = "FC-MARCA primer-render";

    private static double InicioDelProcesoEnEpocaMs() =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Bitacora.MsDesdeArranque();

    // P2 — El WebView empezó a cargar el contenido: recién ahora este arranque cuenta para el rollback por marca.
    private void AlIniciarLaCarga()
    {
        Volatile.Write(ref _cargaIniciada, 1);
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
