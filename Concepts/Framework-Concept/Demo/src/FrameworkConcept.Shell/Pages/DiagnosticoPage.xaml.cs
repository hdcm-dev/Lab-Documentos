using System.Globalization;
using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Shell.Actualizacion;
using FrameworkConcept.Shell.Data;
using FrameworkConcept.Shell.Hosting;
using FrameworkConcept.Shell.Red;

namespace FrameworkConcept.Shell.Pages;

/// <summary>
/// La pantalla de diagnóstico. Cada línea dice de dónde sale, para que quien reporta un error pueda copiarla tal cual.
/// Las líneas de versión (P7) son: cascarón, puente, contenido y definición; el canal no es una versión, es un puntero.
/// </summary>
public partial class DiagnosticoPage : ContentPage
{
    private readonly IDb _db;
    private readonly ProveedorDelActualizador _proveedor;
    private readonly ConsultasDelCanal _consultas;
    private readonly ContenidoActivo _contenido;
    private readonly ConfiguracionDelCascaron _configuracion;
    private readonly MotorWeb _motor;
    private readonly IMonitorDeRed _red;
    private readonly IGuardiaDeAcciones _guardia;

    public DiagnosticoPage(IDb db, ProveedorDelActualizador proveedor, ConsultasDelCanal consultas,
        ContenidoActivo contenido, ConfiguracionDelCascaron configuracion, MotorWeb motor,
        IMonitorDeRed red, IGuardiaDeAcciones guardia)
    {
        InitializeComponent();
        _db = db;
        _proveedor = proveedor;
        _consultas = consultas;
        _contenido = contenido;
        _configuracion = configuracion;
        _motor = motor;
        _red = red;
        _guardia = guardia;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await MostrarAsync();
    }

    private async void AlActualizar(object? sender, EventArgs e) => await MostrarAsync();

    private async Task MostrarAsync()
    {
        Filas.Children.Clear();
        try
        {
            await LlenarAsync();
        }
        catch (Exception ex)
        {
            Fila("Error", ex.Message);
        }
    }

    private async Task LlenarAsync()
    {
        // --- Las cuatro versiones (P7) y el canal ---
        Seccion("Versiones");
        Fila("Cascarón", $"{AppInfo.Current.VersionString} (build {AppInfo.Current.BuildString})");
        Fila("Puente", ContratoDelPuente.Version.ToString());
        Fila("Contenido", _contenido.Version?.ToString() ?? "ninguno activo");
        var definiciones = await _db.ListarDefinicionesAsync();
        if (definiciones.Count == 0)
            Fila("Definición", "ninguna guardada");
        foreach (var grupo in definiciones.GroupBy(d => d.EncuestaId))
            Fila($"Definición {grupo.Key}", $"v{grupo.Max(d => d.Version)} (guardadas: {string.Join(", ", grupo.Select(d => "v" + d.Version))})");
        Fila("Canal", $"{_configuracion.Canal} — {_configuracion.CanalUrl}");
        Fila("Huella del contrato", ContratoDelPuente.Huella());

        // --- Actualizador (P2) ---
        Seccion("Actualizador");
        var puntero = await _proveedor.Almacen.LeerPunteroAsync();
        Fila("current / previous", $"{puntero?.Current?.ToString() ?? "—"} / {puntero?.Previous?.ToString() ?? "—"}");
        Fila("Marca «pendiente»", puntero is null ? "—"
            : puntero.Pendiente?.ToString() ?? "no (el contenido avisó Listo)");
        Fila("En staging", (await _proveedor.Almacen.VersionEnStagingAsync())?.ToString() ?? "nada (se activa en el próximo arranque en frío)");
        var fallidas = await _proveedor.Almacen.LeerFallidasAsync();
        Fila("Fallidas en este teléfono", fallidas.Count == 0 ? "ninguna" : string.Join(", ", fallidas));
        Fila("Última consulta exitosa", puntero?.UltimaConsultaExitosa?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "nunca");
        if (_consultas.Ultima is { } ultima)
            Fila("Última consulta", $"{ultima.Cuando:HH:mm:ss} {ultima.Resultado.Estado} {ultima.Resultado.Motivo} — {ultima.Resultado.Detalle}");
        Fila("Semilla del APK", _proveedor.Semilla?.Cargada?.Version.ToString()
            ?? (puntero?.SemillaRevisada is { } revisada ? $"revisada para {revisada}; no se cargó en este arranque" : "no trae o no verificó"));
        if (puntero?.Pendiente is not null)
            Fila("Cargas sin Listo", $"{puntero.CargasSinListo} de {Core.Update.Actualizador.CargasSinListoParaFallar} (con {Core.Update.Actualizador.CargasSinListoParaFallar}, el arranque siguiente vuelve a previous)");
        Fila("Claves embebidas", $"{_proveedor.ClavesEmbebidas} (keyId {_configuracion.KeyId})");
        foreach (var nota in _contenido.NotasDelArranque)
            Fila("Arranque", nota);

        // --- Motor web (C-3) y catálogo (P12) ---
        Seccion("Dispositivo");
        Fila("WebView", $"{_motor.Version ?? "no se pudo leer"} ({_motor.Paquete ?? "—"}); mínimo {Core.Contracts.ParametrosDeProducto.WebViewMinimo}");
        Fila("Catálogo", string.Join(", ", CatalogoDeCapacidades.Entradas.Select(c => $"{c.Id} {c.Version}")));
        Fila("Base local", $"{EsquemaDelCascaron.NombreDelArchivo}, esquema {EsquemaDelCascaron.Version}");

        // --- Red y acciones (folio 038, borrador §6 «Actividad» y §7) ---
        Seccion("Red");
        var red = _red.Actual;
        Fila("Estado", $"{red.Estado}{(red.Motivo is { } motivo ? " — " + motivo : "")}");
        Fila("Sistema", red.Conectado ? "con internet" : "sin internet");
        if (_red is MonitorDeRed monitor)
        {
            Fila("Perfil", monitor.PerfilMedido ? "datos móviles (medido)" : "WiFi, Ethernet o ninguno");
            Fila("Chequeo de salud", monitor.UltimoChequeo is { } chequeo
                ? $"{chequeo.Cuando.ToLocalTime():HH:mm:ss} {(chequeo.Ok ? "respondió" : "no respondió")} ({chequeo.Detalle}) — {monitor.UrlDeSalud}"
                : $"todavía no (o se invalidó) — {monitor.UrlDeSalud}");
        }
        foreach (var e in _guardia.EvaluarTodas())
            Fila($"Acción {e.Accion}", $"{e.Clase}: {(e.Permitida ? e.QuedaPendiente ? "queda pendiente" : "permitida" : "bloqueada")}{(e.Motivo is { } m ? " — " + m : "")}");

        // El registro circular de llamadas (en memoria; las más recientes primero).
        Fila("Llamadas registradas", $"{RegistroDeLlamadas.Total} (máximo {ParametrosDeRed.RegistroDeLlamadas})");
        foreach (var llamada in RegistroDeLlamadas.Ultimas(20))
            Fila($"{llamada.Cuando:HH:mm:ss} {llamada.Cliente}",
                $"{llamada.Metodo} {llamada.Url?.AbsolutePath} → {llamada.Resultado} en {llamada.Duracion.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture)} ms, " +
                $"{llamada.BytesEnviados.ToString(CultureInfo.InvariantCulture)} B enviados, {llamada.BytesRecibidos?.ToString(CultureInfo.InvariantCulture) ?? "?"} B recibidos");

        // --- Outbox (P5) ---
        Seccion("Outbox");
        var (pendientes, rechazadas) = await _db.ContarOutboxAsync();
        Fila("Pendientes de envío", pendientes.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Fila("En carta muerta", rechazadas.ToString(System.Globalization.CultureInfo.InvariantCulture));
        foreach (var fila in (await _db.ListarOutboxSinEnviarAsync()).Where(f => f.Estado is EstadoDeFila.Rechazada or EstadoDeFila.Error))
            Fila($"Carta muerta {fila.EncuestaId} v{fila.VersionDefinicion}", $"{fila.ClaveIdempotencia}: {fila.UltimoError}");
        foreach (var evento in await _db.UltimosEventosDeContenidoAsync(5))
            Fila("Historial", $"{DateTimeOffset.FromUnixTimeMilliseconds(evento.En).ToLocalTime():MM-dd HH:mm:ss} {evento.Evento} {evento.Version}");
    }

    private void Seccion(string titulo) =>
        Filas.Children.Add(new Label { Text = titulo, FontSize = 18, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 12, 0, 4) });

    private void Fila(string titulo, string valor) =>
        Filas.Children.Add(new Label
        {
            FormattedText = new FormattedString
            {
                Spans =
                {
                    new Span { Text = titulo + ": ", FontAttributes = FontAttributes.Bold },
                    new Span { Text = valor }
                }
            },
            FontSize = 13
        });
}
