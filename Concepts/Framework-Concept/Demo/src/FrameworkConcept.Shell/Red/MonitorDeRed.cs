using FrameworkConcept.Core.Red;
using FrameworkConcept.Shell.Bridge;

namespace FrameworkConcept.Shell.Red;

/// <summary>
/// Borrador §6 — El estado de red combinado: Connectivity + chequeo de salud propio (GET /salud del receptor, timeout
/// corto, caché) + lo que informan las llamadas (ManejadorDeRed). Emite network.changed al contenido.
///
/// Cómo está armado (DISENO §2.2 y §2.3):
///   — toda señal pasa por la función pura <see cref="TransicionesDeRed.Siguiente"/>; esta clase sólo junta señales;
///   — un bucle propio hace el chequeo de salud: enseguida al volver la red, con backoff 5 → 10 → 20 → 40 → 60 s mientras
///     el servidor no responda (reconexión automática, sin límite de intentos), y cada 60 s en línea para confirmar;
///   — un resultado de salud vale 10 s (caché); una llamada que falla o un cambio de red lo invalidan.
/// Al entrar en línea, quien escucha <see cref="Cambio"/> reacciona: el sincronizador vacía el outbox y la guardia
/// reevalúa las acciones.
/// </summary>
public sealed class MonitorDeRed : IMonitorDeRed
{
    private readonly EmisorDeEventos _emisor;
    private readonly Uri _salud;
    // Cliente PROPIO, sin ManejadorDeRed: si la falla del chequeo pasara por el manejador, se contaría dos veces.
    private readonly HttpClient _http = new() { Timeout = ParametrosDeRed.TimeoutDeSalud };
    private readonly SemaphoreSlim _despertar = new(0, 1);
    private readonly Lock _cerrojo = new();

    private int _iniciado;
    private bool _perfilMedido;
    private TimeSpan _proximoReintento = ParametrosDeRed.ReintentoInicial;
    private (DateTimeOffset Cuando, bool Ok, string Detalle)? _ultimoChequeo;

    public MonitorDeRed(EmisorDeEventos emisor, ConfiguracionDelCascaron configuracion)
    {
        _emisor = emisor;
        _salud = new Uri(configuracion.ReceptorUrl, "salud");
    }

    public EstadoDeRedCombinado Actual { get; private set; } = EstadoDeRedCombinado.Inicial;

    /// <summary>Para Diagnóstico: el último chequeo de salud (cuándo, si respondió y qué).</summary>
    public (DateTimeOffset Cuando, bool Ok, string Detalle)? UltimoChequeo => _ultimoChequeo;

    /// <summary>Para Diagnóstico: si el perfil vigente es celular sin WiFi ni Ethernet.</summary>
    public bool PerfilMedido => _perfilMedido;

    /// <summary>Para Diagnóstico: la URL que se chequea.</summary>
    public Uri UrlDeSalud => _salud;

    public event Action<EstadoDeRedCombinado>? Cambio;

    public void Iniciar()
    {
        if (Interlocked.Exchange(ref _iniciado, 1) == 1) return;

        var conectividad = Connectivity.Current;
        _perfilMedido = EsMedido(conectividad.ConnectionProfiles);
        // Primera lectura: Inicial es SinRed, así que «hay red» lleva a RedSinServidor y el bucle chequea enseguida.
        Aplicar(conectividad.NetworkAccess == NetworkAccess.Internet ? SenalDeRed.SistemaConRed : SenalDeRed.SistemaSinRed,
            conectividad.NetworkAccess == NetworkAccess.Internet);
        conectividad.ConnectivityChanged += AlCambiarConectividad;

        _ = Task.Run(BucleAsync);
    }

    public void Reevaluar() => Despertar();

    public void InformarLlamadaFallida(string detalle)
    {
        lock (_cerrojo) _ultimoChequeo = null;   // el resultado anterior ya no dice nada
        Bitacora.Log($"red: llamada fallida ({detalle})");
        Aplicar(SenalDeRed.LlamadaFallida, Actual.Conectado);
        Despertar();
    }

    public void InformarLlamadaExitosa()
    {
        // Una respuesta del servidor (no 5xx) vale como chequeo de salud: el servidor está.
        lock (_cerrojo) _ultimoChequeo = (DateTimeOffset.UtcNow, true, "una llamada respondió");
        Aplicar(SenalDeRed.SaludOk, Actual.Conectado);
    }

    private void AlCambiarConectividad(object? sender, ConnectivityChangedEventArgs e)
    {
        var hayRed = e.NetworkAccess == NetworkAccess.Internet;
        var medido = EsMedido(e.ConnectionProfiles);
        var cambioDePerfil = medido != _perfilMedido;
        _perfilMedido = medido;
        lock (_cerrojo) _ultimoChequeo = null;   // otra red: el chequeo anterior no vale

        Bitacora.Log($"red: sistema {(hayRed ? "con" : "sin")} internet, perfiles [{string.Join(", ", e.ConnectionProfiles)}]");
        Aplicar(hayRed ? SenalDeRed.SistemaConRed : SenalDeRed.SistemaSinRed, hayRed);
        if (hayRed && cambioDePerfil) Aplicar(medido ? SenalDeRed.PerfilMedido : SenalDeRed.PerfilNoMedido, hayRed);
        Despertar();
    }

    /// <summary>Una señal → la función pura → si cambió algo, se publica.</summary>
    private void Aplicar(SenalDeRed senal, bool conectado)
    {
        EstadoDeRedCombinado nuevo;
        lock (_cerrojo)
        {
            var siguiente = TransicionesDeRed.Siguiente(Actual.Estado, senal, _perfilMedido);
            nuevo = new EstadoDeRedCombinado(siguiente, conectado, TransicionesDeRed.MotivoDe(siguiente));
            if (nuevo == Actual) return;
            if (siguiente == EstadoDeRed.RedSinServidor && Actual.Estado != EstadoDeRed.RedSinServidor)
                _proximoReintento = ParametrosDeRed.ReintentoInicial;   // empieza el backoff de nuevo
            Actual = nuevo;
        }
        Bitacora.Log($"red: {nuevo.Estado} (señal {senal}; sistema {(nuevo.Conectado ? "con" : "sin")} internet)");
        Publicar(nuevo);
    }

    private void Publicar(EstadoDeRedCombinado estado)
    {
        try
        {
            Cambio?.Invoke(estado);
        }
        catch (Exception ex)
        {
            Bitacora.Error("red: quien escucha Cambio", ex);
        }
        // P3 — network.changed: «conectado» como en el puente 1.0; «estado» y «motivo», opcionales desde el 1.1.
        _emisor.Emitir("network.changed", new { conectado = estado.Conectado, estado = estado.Estado, motivo = estado.Motivo });
    }

    private void Despertar()
    {
        if (_despertar.CurrentCount > 0) return;
        try { _despertar.Release(); }
        catch (SemaphoreFullException) { /* ya estaba pedido */ }
    }

    /// <summary>
    /// El bucle de la reconexión automática. Duerme lo que corresponde al estado (o hasta que alguien lo despierte) y
    /// hace el chequeo de salud. Sin red no chequea: espera a ConnectivityChanged.
    /// </summary>
    private async Task BucleAsync()
    {
        while (true)
        {
            try
            {
                if (Actual.Estado != EstadoDeRed.SinRed) await ChequearAsync();

                var espera = Actual.Estado switch
                {
                    EstadoDeRed.SinRed => Timeout.InfiniteTimeSpan,
                    EstadoDeRed.RedSinServidor => SiguienteReintento(),
                    _ => ParametrosDeRed.ChequeoEnLinea
                };
                await _despertar.WaitAsync(espera);
            }
            catch (Exception ex)
            {
                Bitacora.Error("red: bucle de salud", ex);
                await Task.Delay(ParametrosDeRed.ReintentoInicial);
            }
        }
    }

    private TimeSpan SiguienteReintento()
    {
        lock (_cerrojo)
        {
            var esta = _proximoReintento;
            var doble = esta + esta;
            _proximoReintento = doble < ParametrosDeRed.ReintentoMaximo ? doble : ParametrosDeRed.ReintentoMaximo;
            return esta;
        }
    }

    /// <summary>GET /salud con timeout corto. Si hay un resultado de menos de 10 s, se usa ése (caché).</summary>
    private async Task ChequearAsync()
    {
        var previo = _ultimoChequeo;
        if (previo is { } p && DateTimeOffset.UtcNow - p.Cuando < ParametrosDeRed.CacheDeSalud)
        {
            Aplicar(p.Ok ? SenalDeRed.SaludOk : SenalDeRed.SaludFalla, Actual.Conectado);
            return;
        }

        bool ok;
        string detalle;
        try
        {
            using var respuesta = await _http.GetAsync(_salud, HttpCompletionOption.ResponseHeadersRead);
            ok = respuesta.IsSuccessStatusCode;
            detalle = $"HTTP {(int)respuesta.StatusCode}";
        }
        catch (Exception ex)   // red caída, DNS, TLS, timeout: para el chequeo todo es «no respondió»
        {
            ok = false;
            detalle = ex is TaskCanceledException ? $"sin respuesta en {ParametrosDeRed.TimeoutDeSalud.TotalSeconds:0} s" : ex.GetType().Name + ": " + ex.Message;
        }

        lock (_cerrojo) _ultimoChequeo = (DateTimeOffset.UtcNow, ok, detalle);
        Bitacora.Log($"red: salud {(ok ? "ok" : "falla")} ({detalle})");
        Aplicar(ok ? SenalDeRed.SaludOk : SenalDeRed.SaludFalla, Actual.Conectado);
    }

    /// <summary>Medida = hay celular y no hay WiFi ni Ethernet (los datos móviles pueden costar).</summary>
    private static bool EsMedido(IEnumerable<ConnectionProfile> perfiles)
    {
        var lista = perfiles.ToList();
        return lista.Contains(ConnectionProfile.Cellular)
            && !lista.Contains(ConnectionProfile.WiFi)
            && !lista.Contains(ConnectionProfile.Ethernet);
    }
}
