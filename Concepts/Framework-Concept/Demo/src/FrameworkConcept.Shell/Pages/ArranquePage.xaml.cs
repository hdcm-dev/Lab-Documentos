using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Core.Update;
using FrameworkConcept.Shell.Actualizacion;
using FrameworkConcept.Shell.Catalogo;
using FrameworkConcept.Shell.Data;
using FrameworkConcept.Shell.Hosting;
using FrameworkConcept.Shell.Sync;

namespace FrameworkConcept.Shell.Pages;

/// <summary>
/// El arranque en frío, en orden:
///   1. abrir la base y comprobar su número de esquema (C-1: si no coincide, no arranca);
///   2. arrancar el monitor de red, restaurar la sesión (folio 038) y la sincronización del outbox (es del cascarón:
///      corre pase lo que pase con el contenido, P5);
///   3. preparar el arranque con el actualizador, ANTES de mostrar y sin red (P2): rollback por marca, retiradas,
///      activación de lo verificado en staging/, semilla en el primer arranque, WebView ≥ 94 (C-3);
///   4. ir a la pantalla que corresponde. La consulta al canal se lanza después, en segundo plano.
/// </summary>
public partial class ArranquePage : ContentPage
{
    private readonly IDb _db;
    private readonly ProveedorDelActualizador _proveedor;
    private readonly ContenidoActivo _contenido;
    private readonly SincronizadorDelOutbox _sincronizador;
    private readonly ConsultasDelCanal _consultas;
    private readonly IMonitorDeRed _red;
    private readonly ISesionDelUsuario _sesion;
    private bool _iniciado;

    public ArranquePage(IDb db, ProveedorDelActualizador proveedor, ContenidoActivo contenido,
        SincronizadorDelOutbox sincronizador, ConsultasDelCanal consultas, IMonitorDeRed red, ISesionDelUsuario sesion)
    {
        _red = red;
        _sesion = sesion;
        InitializeComponent();
        _db = db;
        _proveedor = proveedor;
        _contenido = contenido;
        _sincronizador = sincronizador;
        _consultas = consultas;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_iniciado) return;
        _iniciado = true;
        Bitacora.Log("arranque: pantalla visible");

        try
        {
            var resultado = await Task.Run(PrepararAsync);
            Ir(resultado);
        }
        catch (EsquemaIncompatibleException ex)
        {
            Bitacora.Error("arranque", ex);
            App.MostrarRaiz(App.Pantalla<EstadoTerminalPage>().Configurar(MotivoTerminal.EsquemaIncompatible, ex.Message));
        }
        catch (Exception ex)
        {
            Bitacora.Error("arranque", ex);
            App.MostrarRaiz(App.Pantalla<EstadoTerminalPage>().Configurar(MotivoTerminal.Error, ex.Message));
        }
    }

    private async Task<ResultadoDeArranque> PrepararAsync()
    {
        await _db.AbrirAsync();
        _red.Iniciar();                 // borrador §6 — antes que el sincronizador, que lee su estado
        await _sesion.IniciarAsync();   // P4 — SinCuenta, Bloqueada o Vencida; no toca la red
        _sincronizador.Iniciar();

        foreach (var diferencia in ImplementacionesDelCatalogo.Diferencias())
            Bitacora.Log("catálogo: " + diferencia);

        var actualizador = await _proveedor.ObtenerAsync();
        var resultado = await actualizador.PrepararArranqueAsync();

        // Una sola vez por proceso: no hay activación en caliente (P2).
        _contenido.Fijar(resultado.Version, resultado.Directorio, resultado.Notas);
        foreach (var nota in resultado.Notas) Bitacora.Log("actualizador: " + nota);
        Bitacora.Log($"arranque: {resultado.Estado} {resultado.Version}");
        if (resultado.Version is { } version)
            await _db.AnotarVersionDeContenidoAsync(version.ToString(), "arranque");
        return resultado;
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
}
