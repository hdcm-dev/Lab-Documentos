using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Update;
using FrameworkConcept.Shell.Actualizacion;
using FrameworkConcept.Shell.Data;
using FrameworkConcept.Shell.Hosting;
using FrameworkConcept.Shell.Sync;

namespace FrameworkConcept.Shell.Pages;

/// <summary>
/// El arranque en frío: corre <see cref="PreparacionDelArranque"/> (base, red, sesión, outbox, actualizador; sin red,
/// P2) y va a la pantalla que corresponde. La consulta al canal se lanza después, en segundo plano.
///
/// E04 (folio 050, -p:FcE04=V1) — Arranque superpuesto: antes de preparar, la pantalla adjunta el HybridWebView
/// debajo de su capa nativa y lo deja cargar la versión especulada (la «current» de puntero.json, leída sin tocarla).
/// El puente retiene el saludo hasta la decisión (<see cref="PuertaDelArranque"/>). Si el actualizador decide otra
/// versión, se recarga; si no hay contenido, la vista se quita y se sigue como siempre.
/// </summary>
public partial class ArranquePage : ContentPage
{
    private readonly PreparacionDelArranque _preparacion;
    private readonly ConsultasDelCanal _consultas;
    private readonly ConfiguracionDelCascaron _configuracion;
    private readonly ProveedorDelActualizador _proveedor;
    private readonly ServidorDeContenido _servidor;
    private readonly SincronizadorDelOutbox _sincronizador;
    private readonly ContenidoActivo _contenido;
    private bool _iniciado;
#if FC_E04
    private VistaDelContenido? _vista;
    private int _capaRetirada;
#endif

    public ArranquePage(PreparacionDelArranque preparacion, ConsultasDelCanal consultas, ConfiguracionDelCascaron configuracion,
        ProveedorDelActualizador proveedor, ServidorDeContenido servidor, SincronizadorDelOutbox sincronizador,
        ContenidoActivo contenido)
    {
        InitializeComponent();
        _preparacion = preparacion;
        _consultas = consultas;
        _configuracion = configuracion;
        _proveedor = proveedor;
        _servidor = servidor;
        _sincronizador = sincronizador;
        _contenido = contenido;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_iniciado) return;
        _iniciado = true;
        Bitacora.Log("arranque: pantalla visible");
        Bitacora.Log($"arranque: variante E04={_configuracion.VarianteE04}");

        try
        {
#if FC_E04
            var especulada = await Task.Run(LeerVersionEspeculadaAsync);
            if (especulada is not null) _servidor.Raiz = _proveedor.Almacen.DirectorioDe(especulada);
            AdjuntarLaVista(especulada);
            var resultado = await Task.Run(_preparacion.PrepararAsync);
            IrE04(resultado, especulada);
#else
            _ = _proveedor;
            _ = _servidor;
            _ = _sincronizador;
            _ = _contenido;
            var resultado = await Task.Run(_preparacion.PrepararAsync);
            Ir(resultado);
#endif
        }
        catch (EsquemaIncompatibleException ex)
        {
            Bitacora.Error("arranque", ex);
            QuitarLaVista();
            App.MostrarRaiz(App.Pantalla<EstadoTerminalPage>().Configurar(MotivoTerminal.EsquemaIncompatible, ex.Message));
        }
        catch (Exception ex)
        {
            Bitacora.Error("arranque", ex);
            QuitarLaVista();
            App.MostrarRaiz(App.Pantalla<EstadoTerminalPage>().Configurar(MotivoTerminal.Error, ex.Message));
        }
    }

    private void Ir(ResultadoDeArranque resultado)
    {
        switch (resultado.Estado)
        {
            case EstadoDeArranque.ContenidoListo:
                // La consulta del arranque en frío la lanza la pantalla de contenido cuando el contenido avisa «Listo»:
                // así queda fuera del camino crítico del primer render (P1, umbral TPR).
                App.MostrarRaiz(App.Pantalla<ContenidoPage>());
                break;

            case EstadoDeArranque.MotorDesactualizado:
                App.MostrarRaiz(App.Pantalla<WebViewViejoPage>());
                _consultas.ConsultarEnSegundoPlano(DisparadorDeConsulta.ArranqueEnFrio);
                break;

            case EstadoDeArranque.CascaronNoSoportado:
                App.MostrarRaiz(App.Pantalla<EstadoTerminalPage>().Configurar(MotivoTerminal.CascaronNoSoportado, string.Join("\n", resultado.Notas)));
                break;

            default:
                App.MostrarRaiz(App.Pantalla<EstadoTerminalPage>().Configurar(MotivoTerminal.SinContenidoSano, string.Join("\n", resultado.Notas)));
                _consultas.ConsultarEnSegundoPlano(DisparadorDeConsulta.ArranqueEnFrio);
                break;
        }
    }

#if FC_E04
    // E04 — La «current» de puntero.json, si su directorio existe. Sólo lectura y sin candado: el puntero se reemplaza
    // con un único rename, así que se lee entero o no se lee. No necesita claves ni base. null en el primer arranque (R1).
    private async Task<VersionSemantica?> LeerVersionEspeculadaAsync()
    {
        try
        {
            var puntero = await _proveedor.Almacen.LeerPunteroAsync();
            return puntero?.Current is { } actual && _proveedor.Almacen.ExisteVersion(actual) ? actual : null;
        }
        catch (Exception ex)
        {
            Bitacora.Error("arranque: leer la versión especulada", ex);
            return null;
        }
    }

    private void AdjuntarLaVista(VersionSemantica? especulada)
    {
        var vista = IPlatformApplication.Current!.Services.GetRequiredService<VistaDelContenido>();
        _vista = vista;
        vista.Listo += _ => RetirarLaCapa("Listo");
        Contenedor.Children.Add(vista.Vista);
        Bitacora.Log($"arranque: HybridWebView adjunto (especulada {especulada?.ToString() ?? "ninguna"})");

        // El contenido que no avisa «Listo» también tiene que verse: a los 10 s la capa se retira igual.
        Dispatcher.StartTimer(VistaDelContenido.EsperaMaximaDelListo, () =>
        {
            RetirarLaCapa("sin Listo a los 10 s");
            return false;
        });
    }

    private void IrE04(ResultadoDeArranque resultado, VersionSemantica? especulada)
    {
        if (resultado.Estado != EstadoDeArranque.ContenidoListo)
        {
            // Sin contenido que mostrar: la vista se va y sigue el camino de siempre (terminal, WebView viejo y su consulta).
            QuitarLaVista();
            _servidor.Raiz = null;
            Ir(resultado);
            return;
        }

        if (especulada is not null && !especulada.Equals(resultado.Version))
        {
            // La versión servida no es la decidida (rollback, activación, semilla): el saludo de esa página nunca se
            // respondió. Se sirve la decidida y se vuelve a cargar index.html.
            Bitacora.Log($"arranque: especulación descartada (servida {especulada}, decidida {resultado.Version}); recarga");
            _servidor.Raiz = resultado.Directorio;
            _servidor.Reiniciar();
            _vista?.Recargar();
        }
        // Si coincide (o no había especulada y el servidor esperaba la raíz), no hay nada que hacer: la puerta ya liberó el saludo.
    }

    // E04 — Con «Listo» (o a los 10 s) se retira la capa nativa y la pantalla pasa a ser la del contenido: barra, título,
    // diagnóstico y aviso de carta muerta (P5), como ContenidoPage.
    private void RetirarLaCapa(string motivo) => MainThread.BeginInvokeOnMainThread(() =>
    {
        if (_vista is null || Interlocked.Exchange(ref _capaRetirada, 1) == 1) return;
        CapaNativa.IsVisible = false;
        NavigationPage.SetHasNavigationBar(this, true);
        Title = $"Framework Concept · contenido {_contenido.Version}";
        ToolbarItems.Add(new ToolbarItem("Diagnóstico", null, () => AlAbrirDiagnostico(null, EventArgs.Empty)));
        _sincronizador.ConteoCambiado += AlCambiarElOutbox;
        Bitacora.Log($"arranque: capa nativa retirada ({motivo})");
    });

    // P5 — La carta muerta se muestra al usuario.
    private void AlCambiarElOutbox(int pendientes, int rechazadas) => MainThread.BeginInvokeOnMainThread(() =>
    {
        AvisoCartaMuerta.IsVisible = rechazadas > 0;
        TextoCartaMuerta.Text = $"{rechazadas} respuesta(s) no se pudieron enviar y quedaron guardadas. Tocá para ver el detalle.";
    });
#endif

    private void QuitarLaVista()
    {
#if FC_E04
        if (_vista is null) return;
        _vista.Desconectar();
        _vista = null;
        Contenedor.Children.Clear();
#endif
    }

    private async void AlAbrirDiagnostico(object? sender, EventArgs e) =>
        await Navigation.PushAsync(App.Pantalla<DiagnosticoPage>());
}
