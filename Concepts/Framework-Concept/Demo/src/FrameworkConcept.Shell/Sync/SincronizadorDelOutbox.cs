using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Shell.Bridge;
using FrameworkConcept.Shell.Data;

namespace FrameworkConcept.Shell.Sync;

/// <summary>
/// P5 — Envía el outbox cada vez que hay red, sin depender del momento de la actualización (P13.6, P13.7). Es del
/// cascarón: sigue corriendo aunque el contenido no arranque (pantallas «sin contenido sano» o «actualizá el WebView»).
///
/// Reglas:
///   — idempotente: cada fila lleva su clave; reenviar una ya recibida no duplica;
///   — orden por entidad: una fila trabada (carta muerta) sólo frena a las siguientes de su misma entidad;
///   — backoff exponencial ante fallas de red o del servidor: 5 s, 10 s, 20 s… hasta 10 min, sin límite de intentos;
///   — nada se descarta (P13.7): una fila rechazada queda en carta muerta, visible en Diagnóstico;
///   — folio 038: envía sólo con servidor (IMonitorDeRed: EnLinea o EnLineaMedida, borrador §6) y sólo las filas cuyo
///     dueño es la identidad presente con sesión válida (ISesionDelUsuario, P4). Se despierta al volver EnLinea
///     (reconexión automática, borrador §7) y al volver la sesión a Activa.
/// </summary>
public sealed class SincronizadorDelOutbox(IDb db, ClienteDelReceptor receptor, EmisorDeEventos emisor, IMonitorDeRed red, ISesionDelUsuario sesion)
{
    private static readonly TimeSpan EsperaEntrePasadas = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan BackoffInicial = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan BackoffMaximo = TimeSpan.FromMinutes(10);

    private readonly SemaphoreSlim _senal = new(0, 1);
    // Una pasada a la vez: «Enviar ahora» (outbox.sendNow) y el bucle no mandan la misma fila en paralelo.
    private readonly SemaphoreSlim _unaPasada = new(1, 1);
    private int _iniciado;
    // Folio 038 — Volvió el servidor: la próxima pasada no respeta el backoff de las filas (ver Iniciar).
    private volatile bool _reconexion;
    private bool _conServidor;
    private (int, int) _ultimoConteo = (-1, -1);

    /// <summary>Cambió el conteo (pendientes, en carta muerta). La pantalla de contenido muestra la carta muerta (P5).</summary>
    public event Action<int, int>? ConteoCambiado;

    /// <summary>
    /// Arranca el bucle (una sola vez) y se suscribe a la red y a la sesión. El evento network.changed ya no lo emite
    /// el sincronizador: lo emite el monitor de red (Shell/Red/MonitorDeRed.cs).
    /// </summary>
    public void Iniciar()
    {
        if (Interlocked.Exchange(ref _iniciado, 1) == 1) return;

        // Reconexión automática (borrador §7 «al volver EnLinea»): al pasar de sin servidor a con servidor, la pasada
        // sale ya y sin esperar el backoff de cada fila. Ese backoff era por una falla de red que el monitor acaba de
        // dar por resuelta con su chequeo de salud.
        _conServidor = red.Actual.ConServidor;
        red.Cambio += estado =>
        {
            var antes = _conServidor;
            _conServidor = estado.ConServidor;
            if (!estado.ConServidor) return;
            if (!antes)
            {
                _reconexion = true;
                Bitacora.Log("outbox: volvió el servidor; se envía lo pendiente");
            }
            Despertar();
        };
        sesion.Cambio += s => { if (s.Estado == EstadoDeSesion.Activa) Despertar(); };

        _ = Task.Run(BucleAsync);
    }

    /// <summary>Pide una pasada ya (se encoló algo o volvió la red).</summary>
    public void Despertar()
    {
        if (_senal.CurrentCount == 0)
        {
            try { _senal.Release(); }
            catch (SemaphoreFullException) { /* ya había una pasada pedida */ }
        }
    }

    private async Task BucleAsync()
    {
        while (true)
        {
            try
            {
                await PasadaAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                Bitacora.Error("sincronizador", ex);
            }
            await _senal.WaitAsync(EsperaEntrePasadas);
        }
    }

    /// <summary>
    /// Una pasada: envía lo que toca enviar ahora y avisa al contenido si cambió el conteo. Devuelve cuántas filas
    /// salieron y el conteo visible después (lo usa outbox.sendNow, «Enviar ahora»).
    /// </summary>
    /// <param name="ignorarBackoff">«Enviar ahora» o reconexión: sale también lo que esperaba su próximo intento.</param>
    public async Task<ResultadoDePasada> PasadaAsync(CancellationToken ct, bool ignorarBackoff = false)
    {
        await _unaPasada.WaitAsync(ct);
        try
        {
            var enviadas = 0;
            if (red.Actual.ConServidor)
            {
                if (_reconexion) { _reconexion = false; ignorarBackoff = true; }
                foreach (var fila in await ElegirParaEnviarAsync(ct, ignorarBackoff))
                    if (await EnviarAsync(fila, ct) == ResultadoDeEnvio.Aceptada) enviadas++;
            }
            var (pendientes, rechazadas) = await AvisarSiCambioAsync(ct);
            return new ResultadoDePasada(enviadas, pendientes, rechazadas);
        }
        finally
        {
            _unaPasada.Release();
        }
    }

    /// <summary>La primera fila pendiente de cada entidad, si su entidad no está trabada y ya pasó su backoff.</summary>
    private async Task<IReadOnlyList<FilaDeOutbox>> ElegirParaEnviarAsync(CancellationToken ct, bool ignorarBackoff)
    {
        var sinEnviar = await db.ListarOutboxSinEnviarAsync(ct);   // ordenadas por Id: el orden en que se encolaron
        var ahora = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var elegidas = new List<FilaDeOutbox>();
        var vistas = new HashSet<string>(StringComparer.Ordinal);

        // P4 — Las filas de otro dueño, o sin sesión válida, ni se miran: quedan esperando a su dueño. Se filtran ANTES
        // del orden por entidad, para que una fila ajena no trabe a nadie.
        foreach (var fila in sinEnviar.Where(f => sesion.PuedeEnviarFilaDe(f.Dueno)))
        {
            // La primera fila sin enviar de cada entidad decide: si está en carta muerta, la entidad queda trabada;
            // si está pendiente, es la única de su entidad que puede salir en esta pasada.
            if (!vistas.Add(fila.Entidad)) continue;
            if (fila.Estado == EstadoDeFila.Pendiente && (ignorarBackoff || fila.ProximoIntentoEn <= ahora)) elegidas.Add(fila);
        }
        return elegidas;
    }

    private async Task<ResultadoDeEnvio?> EnviarAsync(FilaDeOutbox fila, CancellationToken ct)
    {
        // Corrector E03 — La fila se eligió al principio de la pasada; la sesión pudo cambiar después (cierre, otra
        // cuenta). Justo antes de enviar se vuelve a validar el dueño y se toma SU token, juntos y bajo el cerrojo de la
        // sesión. Si ya no corresponde, la fila no sale ni cuenta un intento: espera a su dueño.
        string? token = null;
        if (sesion.Actual.Modo == ModoDeSesion.Oidc)
        {
            token = await sesion.ObtenerAccessTokenParaAsync(fila.Dueno, ct);
            if (token is null)
            {
                Bitacora.Log($"outbox: {fila.ClaveIdempotencia} no sale ahora (su dueño no tiene sesión válida)");
                return null;
            }
        }

        var (resultado, detalle) = await receptor.EnviarAsync(fila, token, ct);
        fila.Intentos++;
        fila.UltimoError = resultado == ResultadoDeEnvio.Aceptada ? null : detalle;

        switch (resultado)
        {
            case ResultadoDeEnvio.Aceptada:
                fila.Estado = EstadoDeFila.Enviada;
                fila.EnviadaEn = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                break;
            case ResultadoDeEnvio.Rechazada:
                fila.Estado = EstadoDeFila.Rechazada;
                break;
            default:
                var espera = TimeSpan.FromTicks(Math.Min(BackoffMaximo.Ticks, BackoffInicial.Ticks << Math.Min(fila.Intentos - 1, 16)));
                fila.ProximoIntentoEn = DateTimeOffset.UtcNow.Add(espera).ToUnixTimeMilliseconds();
                break;
        }

        await db.ActualizarFilaAsync(fila, ct);
        Bitacora.Log($"outbox: {fila.ClaveIdempotencia} intento {fila.Intentos} → {fila.Estado} ({detalle})");
        return resultado;
    }

    private async Task<(int Pendientes, int Rechazadas)> AvisarSiCambioAsync(CancellationToken ct)
    {
        // P4 — Sólo las filas del dueño visible: las de otra identidad no se muestran.
        var conteo = await db.ContarOutboxDeAsync(sesion.DuenoVisible, ct);
        if (conteo == _ultimoConteo) return conteo;
        _ultimoConteo = conteo;
        ConteoCambiado?.Invoke(conteo.Pendientes, conteo.Rechazadas);
        emisor.Emitir("outbox.changed", new { pendientes = conteo.Pendientes, rechazadas = conteo.Rechazadas });
        return conteo;
    }
}

/// <summary>Lo que dejó una pasada del sincronizador: filas aceptadas y conteo visible después.</summary>
public sealed record ResultadoDePasada(int Enviadas, int Pendientes, int Rechazadas);
