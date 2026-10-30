using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Update;
using FrameworkConcept.Shell.Actualizacion;
using FrameworkConcept.Shell.Bridge;

namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// El HybridWebView del contenido y lo que lo acompaña: el puente, la marca «primer-render» (P1), la carga iniciada
/// (P2) y la consulta del arranque en frío. Sale de ContenidoPage para que la use también la pantalla de arranque con
/// E04 (folio 050), que hospeda el contenido debajo de su capa nativa. La raíz del servidor la fija quien la crea.
/// </summary>
public sealed class VistaDelContenido
{
    /// <summary>Si el contenido no avisa «Listo» en este tiempo, la consulta del arranque en frío se lanza igual.</summary>
    public static readonly TimeSpan EsperaMaximaDelListo = TimeSpan.FromSeconds(10);

    private readonly ServidorDeContenido _servidor;
    private readonly DespachadorDelPuente _despachador;
    private readonly ConsultasDelCanal _consultas;
    private readonly ProveedorDelActualizador _proveedor;
    private readonly ContenidoActivo _contenido;
    private readonly PuertaDelArranque _puerta;
    private readonly HybridWebView _vista;
    private int _consultaLanzada;
    private int _marcaLeida;
    private int _cargaIniciada;
    private int _listoAvisado;

    public VistaDelContenido(ContenidoActivo contenido, ServidorDeContenido servidor, PuenteDelCascaron puente,
        EmisorDeEventos emisor, DespachadorDelPuente despachador, ConsultasDelCanal consultas,
        ProveedorDelActualizador proveedor, PuertaDelArranque puerta)
    {
        _servidor = servidor;
        _despachador = despachador;
        _consultas = consultas;
        _proveedor = proveedor;
        _contenido = contenido;
        _puerta = puerta;

        // C-1 de E01 — un único HybridWebView; el contenido lo sirve WebResourceRequested desde content/versions/vN.
        // HybridRoot apunta a una página de reserva del APK («hwv»): nunca se usa para el contenido descargado.
        servidor.CargaIniciada += AlIniciarLaCarga;
        var vista = new HybridWebView { HybridRoot = "hwv", DefaultFile = "index.html" };
        _vista = vista;
        vista.WebResourceRequested += servidor.AlPedirRecurso;
        vista.SetInvokeJavaScriptTarget(puente);   // P3 — un solo objeto, un solo método: Call(sobre)
        emisor.Conectar(vista);
        vista.HandlerChanged += (_, _) =>
        {
            if (vista.Handler is not null) Bitacora.Log("arranque: HybridWebView adjunto (nativo)");
        };

        _despachador.ListoRecibido += AlRecibirListo;
        vista.Dispatcher.StartTimer(EsperaMaximaDelListo, () =>
        {
            // P1, folio 042 — Si a los 10 s el WebView ni pidió el contenido, la actividad no llegó al frente (pantalla
            // apagada o bloqueada): no habrá «primer-render» hasta que se vea. Se deja dicho para quien lee el TPR.
            if (Volatile.Read(ref _cargaIniciada) == 0)
                Bitacora.Log("contenido: el WebView no pidió el contenido a los 10 s (¿la app no llegó al frente? pantalla apagada o bloqueada)");
            LanzarConsulta("sin Listo a los 10 s");
            return false;
        });
    }

    /// <summary>El control que se agrega a la pantalla.</summary>
    public View Vista => _vista;

    /// <summary>E04 — El contenido confirmó «Listo»: la pantalla de arranque retira su capa nativa. Una vez.</summary>
    public event Action<VersionSemantica>? Listo;

    /// <summary>E04 — La especulación se descartó: vuelve a cargar index.html desde la raíz nueva del servidor.</summary>
    public void Recargar()
    {
        Interlocked.Exchange(ref _cargaIniciada, 0);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try { await _vista.EvaluateJavaScriptAsync("location.reload()"); }
            catch (Exception ex) { Bitacora.Error("contenido: recargar", ex); }
        });
    }

    /// <summary>La vista sale de la pantalla (E04, el arranque no terminó en contenido): deja de escuchar al servidor y al puente.</summary>
    public void Desconectar()
    {
        // La pantalla que sigue (terminal, WebView viejo) lanza su propia consulta: el reloj de 10 s de esta vista ya no.
        Interlocked.Exchange(ref _consultaLanzada, 1);
        _servidor.CargaIniciada -= AlIniciarLaCarga;
        _despachador.ListoRecibido -= AlRecibirListo;
        _vista.WebResourceRequested -= _servidor.AlPedirRecurso;
    }

    private void AlRecibirListo(VersionSemantica version)
    {
        LeerLaMarcaDelPrimerRender(version);
        if (Interlocked.Exchange(ref _listoAvisado, 1) == 0) Listo?.Invoke(version);
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
    // E04 — Con especulación, cuenta sólo si la versión servida es la que decidió el actualizador; si no, esta carga se
    // descarta y la que cuenta es la de la recarga.
    private void AlIniciarLaCarga(string? raizServida)
    {
        Volatile.Write(ref _cargaIniciada, 1);
        _ = Task.Run(async () =>
        {
            try
            {
#if FC_E04
                _ = _contenido;   // con E04 la versión sale de la decisión, no de lo fijado
                var resultado = await _puerta.Preparado;
                if (resultado.Version is not { } version) return;
                if (!string.Equals(raizServida, resultado.Directorio, StringComparison.Ordinal))
                {
                    Bitacora.Log($"contenido: carga de una versión especulada descartada ({raizServida}); no cuenta");
                    return;
                }
#else
                _ = raizServida;
                _ = _puerta;
                if (_contenido.Version is not { } version) return;
#endif
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
}
