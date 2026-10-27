using System.Diagnostics;
using System.Globalization;
using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Shell.Bridge;
using FrameworkConcept.Shell.Data;
using FrameworkConcept.Shell.Sync;

namespace FrameworkConcept.Shell.Auth;

/// <summary>
/// P4 — La sesión del usuario: entrada local separada de la sesión con el servidor (DISENO-RED-Y-AUTENTICACION.md §3).
///
///   — Primer ingreso: OIDC code + PKCE en el navegador del sistema (WebAuthenticator, RFC 8252).
///   — Tokens: el access en memoria; el refresh en memoria y, si la persona activa «usar la seguridad del teléfono»,
///     sellado por <see cref="CofreDeSesion"/> (Keystore + BiometricPrompt, API ≥ 30, D-03 = T1).
///   — Ingresos siguientes: desbloqueo con huella, rostro, PIN o patrón; la entrada no espera a la red y la renovación
///     del access corre en segundo plano.
///   — Cerrar o vencer la sesión no borra las filas pendientes: quedan con su dueño («oidc:{sub}»).
///   — El refresh token nunca cruza el puente (P3): al contenido sólo le llega <see cref="SesionInformada"/>.
///
/// Modo: si el APK se compiló sin -p:FcOidcAutoridad, es la sesión por dispositivo del caso base (A1–A3), sin cambios.
/// Los datos no secretos van a la tabla de configuración (claves «sesion.*»); el esquema NO cambia (C-1, DISENO §3.6).
/// </summary>
public sealed class SesionDelUsuario : ISesionDelUsuario
{
    // Claves de la tabla de configuración (DISENO §3.6). Un valor vacío equivale a «no está» (IDb no tiene borrado).
    private const string ClaveSub = "sesion.sub";
    private const string ClaveDueno = "sesion.dueno";
    private const string ClaveUsuario = "sesion.usuario";
    private const string ClaveDesbloqueo = "sesion.desbloqueo";
    private const string ClaveRefreshSellado = "sesion.refreshSellado";
    private const string ClaveRefreshExpira = "sesion.refreshExpira";
    private const string Activado = "activado";

    private readonly IdentidadDelDispositivo _identidad;
    private readonly IDb _db;
    private readonly CofreDeSesion _cofre;
    private readonly EmisorDeEventos _emisor;
    private readonly IMonitorDeRed _red;
    private readonly ClienteDelEmisor? _oidc;
    // Corrector E03 — Un solo cerrojo para la renovación Y para el cambio de identidad (ingreso, cierre): mientras se
    // elige el token de una fila nadie cambia de dueño, y una renovación en vuelo no pisa los tokens de otra cuenta.
    private readonly SemaphoreSlim _renovacion = new(1, 1);
    private readonly object _cerrojo = new();

    // Estado de la máquina y lo que se muestra.
    private EstadoDeSesion _estado = EstadoDeSesion.SinCuenta;
    private string? _motivo;

    // Identidad presente (la de la última cuenta que ingresó en este teléfono).
    private string? _sub;
    private string? _dueno;
    private string? _usuario;
    private bool _desbloqueoActivado;

    // Tokens: SÓLO en memoria. Nunca a disco en claro, nunca al puente.
    private string? _access;
    private DateTimeOffset _accessVence;
    private string? _refresh;
    private DateTimeOffset? _refreshVence;
    private string? _accessRenovadoPor401;

    private DateTimeOffset? _pasoASegundoPlano;

    public SesionDelUsuario(ConfiguracionDelCascaron configuracion, IdentidadDelDispositivo identidad, IDb db, CofreDeSesion cofre,
        EmisorDeEventos emisor, IMonitorDeRed red)
    {
        _identidad = identidad;
        _db = db;
        _cofre = cofre;
        _emisor = emisor;
        _red = red;
        _oidc = configuracion.Oidc is { } oidc ? new ClienteDelEmisor(oidc, red) : null;
        Actual = _oidc is null ? SesionInformada.PorDispositivo : Informar();
    }

    public SesionInformada Actual { get; private set; }

    public event Action<SesionInformada>? Cambio;

    private bool ModoOidc => _oidc is not null;

    // --- Arranque -------------------------------------------------------------------------------------------

    public async Task IniciarAsync()
    {
        if (!ModoOidc) return;   // modo Dispositivo: como en A3

        _sub = await LeerAsync(ClaveSub);
        _dueno = await LeerAsync(ClaveDueno);
        _usuario = await LeerAsync(ClaveUsuario);
        _desbloqueoActivado = await LeerAsync(ClaveDesbloqueo) == Activado;
        _refreshVence = await LeerAsync(ClaveRefreshExpira) is { } ms && long.TryParse(ms, CultureInfo.InvariantCulture, out var v)
            ? DateTimeOffset.FromUnixTimeMilliseconds(v) : null;

        // D-03 = T1 — Si quitaron el bloqueo de pantalla, la clave del Keystore ya no sirve: reingreso con la cuenta.
        // Corrector E03 — Sin prueba local la entrada no se abre: SinCuenta (antes, Revocada con la entrada abierta).
        if (_desbloqueoActivado && _cofre.Evaluar(activado: true).Desbloqueo != DesbloqueoLocal.Activado)
        {
            await QuitarDesbloqueoAsync();
            OlvidarIdentidadEnMemoria();
            Fijar(EstadoDeSesion.SinCuenta, "La seguridad del teléfono cambió: ingresá de nuevo con tu cuenta. Lo pendiente se conserva.");
            return;
        }

        // Corrector E03 — En frío la entrada exige prueba local: Bloqueada (desbloqueo) o SinCuenta (la cuenta).
        var huboIdentidad = _dueno is not null;
        var inicial = TransicionesDeSesion.AlArrancar(huboIdentidad, _desbloqueoActivado);
        if (inicial == EstadoDeSesion.SinCuenta) OlvidarIdentidadEnMemoria();
        Fijar(inicial, inicial switch
        {
            EstadoDeSesion.Bloqueada => "Desbloqueá con la seguridad del teléfono.",
            EstadoDeSesion.SinCuenta when huboIdentidad => "Tu sesión venció: ingresá con tu cuenta. Lo pendiente se conserva y sale con ella.",
            _ => null
        });
    }

    // --- Dueño de las filas del outbox (P4, P5) ---------------------------------------------------------------

    public async Task<string> ObtenerDuenoAsync()
    {
        if (!ModoOidc) return await _identidad.ObtenerAsync();
        // La guardia no deja llegar acá sin entrada abierta; si llegara, no se inventa un dueño.
        return _dueno ?? throw ExcepcionDeAutenticacion.Fallida("Ingresá con tu cuenta para registrar.");
    }

    // "" no coincide con ninguna fila: sin identidad no se ve ninguna (las de otra cuenta son invisibles).
    public string? DuenoVisible => ModoOidc ? _dueno ?? "" : null;

    public bool PuedeEnviarFilaDe(string dueno) =>
        !ModoOidc || PropiedadDelOutbox.PuedeEnviarse(dueno, _dueno, Actual);

    // --- Tokens ------------------------------------------------------------------------------------------------

    public async Task<string?> ObtenerAccessTokenAsync(CancellationToken ct)
    {
        if (!ModoOidc || _estado != EstadoDeSesion.Activa) return null;
        if (_access is not null && _accessVence - ParametrosDeSesion.RenovarConAnticipacion > DateTimeOffset.UtcNow) return _access;
        return await RenovarAsync(ct);
    }

    public async Task<string?> ObtenerAccessTokenParaAsync(string dueno, CancellationToken ct)
    {
        if (!ModoOidc) return null;
        await _renovacion.WaitAsync(ct);
        try
        {
            // Corrector E03 — Dueño y token se deciden juntos, bajo el cerrojo del cambio de identidad.
            if (!PuedeEnviarFilaDe(dueno)) return null;
            if (_access is not null && _accessVence - ParametrosDeSesion.RenovarConAnticipacion > DateTimeOffset.UtcNow) return _access;
            return await RenovarConCerrojoAsync(ct);
        }
        finally
        {
            _renovacion.Release();
        }
    }

    public async Task InformarNoAutorizadoAsync(string tokenRechazado, CancellationToken ct)
    {
        if (!ModoOidc || _estado != EstadoDeSesion.Activa) return;
        // Se renueva UNA vez por token: si el token recién renovado también recibe 401, no se insiste (la fila se
        // reintenta con el backoff del sincronizador, DISENO §3.5). Corrector E03: un 401 a un token que ya no es el
        // de la sesión (otra cuenta ingresó en el medio) no toca la sesión actual.
        var rechazado = _access;
        if (rechazado is null || rechazado != tokenRechazado || rechazado == _accessRenovadoPor401) return;
        Bitacora.Log("sesión: el servidor contestó 401; se renueva el token una vez");
        _access = null;
        if (await RenovarAsync(ct) is { } nuevo) _accessRenovadoPor401 = nuevo;
    }

    /// <summary>
    /// Renueva con el refresh token. invalid_grant → Vencida (si pasó su vencimiento) o Revocada; sin red, null y la
    /// sesión sigue Activa (P4: sin red se opera; la renovación se reintenta en el próximo envío).
    /// </summary>
    private async Task<string?> RenovarAsync(CancellationToken ct)
    {
        await _renovacion.WaitAsync(ct);
        try
        {
            return await RenovarConCerrojoAsync(ct);
        }
        finally
        {
            _renovacion.Release();
        }
    }

    /// <summary>La renovación propiamente dicha; quien llama ya tiene <see cref="_renovacion"/>.</summary>
    private async Task<string?> RenovarConCerrojoAsync(CancellationToken ct)
    {
        if (_estado != EstadoDeSesion.Activa) return null;
        if (_access is not null && _accessVence - ParametrosDeSesion.RenovarConAnticipacion > DateTimeOffset.UtcNow) return _access;

        if (_refresh is null || _refreshVence is { } vence && vence <= DateTimeOffset.UtcNow)
        {
            await PerderSesionAsync(EventoDeSesion.SesionVencida, "Tu sesión venció: ingresá para enviar lo pendiente.");
            return null;
        }

        try
        {
            var tokens = await _oidc!.RenovarAsync(_refresh, ct);
            Guardar(tokens);
            Bitacora.Log($"sesión: access renovado (vence {tokens.ExpiraEn:HH:mm:ss}Z)");
            return _access;
        }
        catch (ErrorDelEmisorException ex) when (ex.EsInvalidGrant)
        {
            Bitacora.Log("sesión: el emisor rechazó la renovación (invalid_grant)");
            var vencida = _refreshVence is { } v && v <= DateTimeOffset.UtcNow;
            await PerderSesionAsync(vencida ? EventoDeSesion.SesionVencida : EventoDeSesion.RenovacionRechazada,
                vencida ? "Tu sesión venció: ingresá para enviar lo pendiente."
                        : "El servidor cerró tu sesión: ingresá de nuevo para enviar lo pendiente.");
            return null;
        }
        catch (Exception ex) when (FallasDelEmisor.EsFalla(ex, ct))
        {
            // Corrector E03 (H-4) — Cualquier falla (WebException en Android incluida): sin token, sigue Activa.
            Bitacora.Error("sesión: renovación sin respuesta del emisor", ex);
            return null;
        }
    }

    // --- auth.signIn: OIDC code + PKCE en el navegador del sistema ----------------------------------------------

    public async Task<SesionInformada> IngresarAsync(CancellationToken ct)
    {
        if (_oidc is null) throw ExcepcionDeAutenticacion.Fallida("Esta app no usa cuenta.");
        if (_estado is not (EstadoDeSesion.SinCuenta or EstadoDeSesion.Vencida or EstadoDeSesion.Revocada)) return Actual;

        var antes = _estado;
        Transicionar(EventoDeSesion.IngresoIniciado, "Completá el ingreso en el navegador…");
        try
        {
            var descubrimiento = await _oidc.DescubrirAsync(ct);
            var verificador = FlujoOidc.CrearVerificador();
            var state = FlujoOidc.CrearState();
            var url = FlujoOidc.UrlDeAutorizacion(descubrimiento, _oidc.Configuracion, FlujoOidc.Desafio(verificador), state);

            // RFC 8252 — El navegador del sistema (Custom Tabs si hay), nunca un WebView embebido. Vuelve por
            // com.frameworkconcept.demo://callback a CallbackDeAutenticacion.
            Bitacora.Log("sesión: abriendo el navegador del sistema");
            WebAuthenticatorResult vuelta;
            try
            {
                // Corrector E03 (H-3, ANR) — ConfigureAwait(false): lo que sigue a la vuelta del navegador (canje HTTP,
                // escrituras en SQLite, avisos) NO corre en el hilo principal, que en ese momento está devolviendo el foco.
                vuelta = await MainThread.InvokeOnMainThreadAsync(() =>
                    WebAuthenticator.Default.AuthenticateAsync(new WebAuthenticatorOptions { Url = url, CallbackUrl = _oidc.Configuracion.Redireccion }))
                    .ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // WebAuthenticator cancela cuando la persona cierra el navegador sin terminar.
                throw ExcepcionDeAutenticacion.Cancelada("Cerraste el navegador sin terminar el ingreso.");
            }

            if (vuelta.Properties.TryGetValue("error", out var error))
                throw ExcepcionDeAutenticacion.Fallida($"El emisor contestó «{error}».");
            if (!vuelta.Properties.TryGetValue("state", out var stateVuelto) || stateVuelto != state)
                throw ExcepcionDeAutenticacion.Fallida("La respuesta del navegador no corresponde a este ingreso (state distinto).");
            if (!vuelta.Properties.TryGetValue("code", out var code))
                throw ExcepcionDeAutenticacion.Fallida("El navegador volvió sin código de autorización.");

            Bitacora.Log($"sesión: volvió el navegador (hilo principal: {MainThread.IsMainThread})");
            var reloj = Stopwatch.StartNew();
            var tokens = await _oidc.CanjearAsync(code, verificador, ct).ConfigureAwait(false);
            var canje = reloj.ElapsedMilliseconds;
            var (sub, usuario) = FlujoOidc.LeerIdentidad(tokens.IdToken ?? throw ExcepcionDeAutenticacion.Fallida("El emisor no devolvió id_token."));

            // Corrector E03 — El cambio de identidad va bajo el cerrojo del envío: una fila elegida con otro dueño no
            // puede salir con estos tokens.
            await _renovacion.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // Un ingreso nuevo deja el refresh viejo sin uso: el desbloqueo se vuelve a ofrecer (se sella el nuevo).
                if (_desbloqueoActivado) await QuitarDesbloqueoAsync().ConfigureAwait(false);
                _sub = sub;
                _dueno = PropiedadDelOutbox.DuenoOidc(sub);
                _usuario = usuario;
                _accessRenovadoPor401 = null;
                await _db.EscribirConfiguracionAsync(ClaveSub, sub, ct).ConfigureAwait(false);
                await _db.EscribirConfiguracionAsync(ClaveDueno, _dueno, ct).ConfigureAwait(false);
                await _db.EscribirConfiguracionAsync(ClaveUsuario, usuario ?? "", ct).ConfigureAwait(false);
                Guardar(tokens);
            }
            finally
            {
                _renovacion.Release();
            }
            Bitacora.Log($"sesión: ingresó {usuario} ({_dueno}); canje {canje} ms, escrituras {reloj.ElapsedMilliseconds - canje} ms");
            Transicionar(EventoDeSesion.IngresoExitoso, null);
            return Actual;
        }
        catch (ExcepcionDeAutenticacion)
        {
            Transicionar(EventoDeSesion.IngresoFallido, MotivoDe(antes), antes);
            throw;
        }
        catch (Exception ex) when (FallasDelEmisor.EsFalla(ex, ct))
        {
            // Corrector E03 (H-4) — Toda falla que no sea de autenticación (la WebException de Android incluida)
            // devuelve el estado previo: nunca queda Ingresando para siempre.
            Bitacora.Error("sesión: ingreso", ex);
            Transicionar(EventoDeSesion.IngresoFallido, MotivoDe(antes), antes);
            throw ExcepcionDeAutenticacion.Fallida("No se pudo completar el ingreso: " + ex.Message, ex);
        }
    }

    // --- auth.enableUnlock / auth.unlock: la seguridad del teléfono ------------------------------------------

    public async Task<SesionInformada> ActivarDesbloqueoAsync(CancellationToken ct)
    {
        if (!ModoOidc) throw ExcepcionDeAutenticacion.Fallida("Esta app no usa cuenta.");
        var (desbloqueo, motivo) = _cofre.Evaluar(_desbloqueoActivado);
        if (desbloqueo == DesbloqueoLocal.Activado) return Actual;
        if (desbloqueo == DesbloqueoLocal.NoDisponible || !OperatingSystem.IsAndroidVersionAtLeast(30))
            throw ExcepcionDeAutenticacion.Fallida(motivo ?? ReglaDelDesbloqueo.MotivoApiVieja);
        if (_estado != EstadoDeSesion.Activa || _refresh is null)
            throw ExcepcionDeAutenticacion.Fallida("Ingresá con tu cuenta antes de activar la seguridad del teléfono.");

        var sellado = await _cofre.SellarAsync(_refresh, ct);
        await _db.EscribirConfiguracionAsync(ClaveRefreshSellado, sellado, ct);
        await _db.EscribirConfiguracionAsync(ClaveRefreshExpira,
            _refreshVence?.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture) ?? "", ct);
        await _db.EscribirConfiguracionAsync(ClaveDesbloqueo, Activado, ct);
        _desbloqueoActivado = true;
        Bitacora.Log("sesión: desbloqueo con la seguridad del teléfono activado");
        Publicar();
        return Actual;
    }

    public async Task<SesionInformada> DesbloquearAsync(CancellationToken ct)
    {
        if (!ModoOidc) throw ExcepcionDeAutenticacion.Fallida("Esta app no usa cuenta.");
        if (_estado != EstadoDeSesion.Bloqueada) return Actual;
        if (!OperatingSystem.IsAndroidVersionAtLeast(30)) throw ExcepcionDeAutenticacion.Fallida(ReglaDelDesbloqueo.MotivoApiVieja);

        string abierto;
        try
        {
            var sellado = await LeerAsync(ClaveRefreshSellado) ?? throw new ClaveInvalidadaException("No hay sesión guardada en este teléfono.");
            abierto = await _cofre.AbrirAsync(sellado, ct);
        }
        catch (ClaveInvalidadaException ex)
        {
            // Borrador §4 — huella nueva, bloqueo quitado o copia restaurada: se borra todo lo sellado y se pide la cuenta.
            Bitacora.Log("sesión: clave invalidada — " + ex.Message);
            await QuitarDesbloqueoAsync();
            // Corrector E03 — Desde Bloqueada no hubo prueba local: SinCuenta (la entrada no se abre); las filas quedan.
            OlvidarIdentidadEnMemoria();
            var motivo = ex.Message + " Ingresá de nuevo con tu cuenta.";
            Transicionar(EventoDeSesion.ClaveInvalidada, motivo);
            throw ExcepcionDeAutenticacion.Fallida(motivo, ex);
        }
        catch (ExcepcionDeAutenticacion ex) when (ex.Codigo == Core.Contracts.Puente.CodigosDeError.AutenticacionCancelada)
        {
            Transicionar(EventoDeSesion.DesbloqueoCancelado, "Desbloqueá con la seguridad del teléfono.");
            throw;
        }

        // Si el proceso siguió vivo (bloqueo al volver de segundo plano) el refresh de memoria es más nuevo que el sellado.
        _refresh ??= abierto;
        Bitacora.Log("sesión: desbloqueada con la seguridad del teléfono");
        Transicionar(EventoDeSesion.DesbloqueoExitoso, null);

        // Corrector E03 — El refresh sellado ya venció: hubo prueba local (la entrada queda abierta), la sesión con el
        // servidor no. Antes esto se decidía en frío y abría la entrada sin desbloqueo.
        if (_refreshVence is { } vence && vence <= DateTimeOffset.UtcNow)
        {
            await PerderSesionAsync(EventoDeSesion.SesionVencida, "Tu sesión venció: ingresá para enviar lo pendiente.");
            return Actual;
        }

        // P4 — La entrada no espera a la red: la renovación corre en segundo plano.
        _ = Task.Run(() => ObtenerAccessTokenAsync(CancellationToken.None));
        return Actual;
    }

    // --- auth.signOut ------------------------------------------------------------------------------------------

    public async Task<(SesionInformada Sesion, int PendientesConservados)> CerrarAsync(CancellationToken ct)
    {
        if (_oidc is null) throw ExcepcionDeAutenticacion.Fallida("Esta app no usa cuenta.");

        // Corrector E03 — El cambio de identidad va bajo el cerrojo del envío (ver ObtenerAccessTokenParaAsync).
        await _renovacion.WaitAsync(ct);
        try
        {
            // P4 — Las filas pendientes NO se tocan: quedan con su dueño y salen cuando ese dueño vuelva con red.
            var pendientes = _dueno is null ? 0 : (await _db.ContarOutboxDeAsync(_dueno, ct)).Pendientes;

            // Revocación en el emisor: de mejor esfuerzo; sin servidor no se espera.
            if (_refresh is { } refresh && _red.Actual.ConServidor)
            {
                try
                {
                    using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    limite.CancelAfter(TimeSpan.FromSeconds(3));
                    await _oidc.RevocarAsync(refresh, limite.Token);
                }
                catch (Exception ex) when (FallasDelEmisor.EsFalla(ex, ct))
                {
                    // Corrector E03 (H-4) — WebException incluida: la revocación es de mejor esfuerzo.
                    Bitacora.Error("sesión: revocación", ex);
                }
            }

            _access = _refresh = null;
            _refreshVence = null;
            await QuitarDesbloqueoAsync();
            foreach (var clave in new[] { ClaveSub, ClaveDueno, ClaveUsuario })
                await _db.EscribirConfiguracionAsync(clave, "", ct);
            Bitacora.Log($"sesión: cerrada por la persona ({_dueno}); pendientes conservados {pendientes}");
            OlvidarIdentidadEnMemoria();
            Transicionar(EventoDeSesion.Cierre, pendientes switch
            {
                0 => null,
                1 => "Quedó 1 pendiente guardado: sale cuando vuelvas a ingresar con esa cuenta.",
                _ => $"Quedaron {pendientes} pendientes guardados: salen cuando vuelvas a ingresar con esa cuenta."
            });
            return (Actual, pendientes);
        }
        finally
        {
            _renovacion.Release();
        }
    }

    // --- Segundo plano (borrador §4 «re-pedir la seguridad del teléfono después de X minutos») -----------------

    public void AlPasarASegundoPlano() => _pasoASegundoPlano = DateTimeOffset.UtcNow;

    public void AlVolverAlFrente()
    {
        if (!ModoOidc) return;
        if (TransicionesDeSesion.DebeBloquearAlVolver(_estado, _desbloqueoActivado, _pasoASegundoPlano, DateTimeOffset.UtcNow,
                ParametrosDeSesion.BloqueoTrasSegundoPlano))
        {
            Bitacora.Log("sesión: bloqueada al volver de segundo plano");
            Transicionar(EventoDeSesion.Bloquear, "Desbloqueá con la seguridad del teléfono.");
        }
        _pasoASegundoPlano = null;
    }

    // --- Auxiliares --------------------------------------------------------------------------------------------

    private void Guardar(RespuestaDeTokens tokens)
    {
        _access = tokens.AccessToken;
        _accessVence = tokens.ExpiraEn;
        // Algunos emisores no rotan el refresh: si no vino uno nuevo, sigue el anterior.
        if (tokens.RefreshToken is { } refresh)
        {
            _refresh = refresh;
            _refreshVence = tokens.RefreshExpiraEn;
        }
    }

    /// <summary>Vencida o Revocada por el emisor: el refresh no sirve más, ni el de memoria ni el sellado.</summary>
    private async Task PerderSesionAsync(EventoDeSesion evento, string motivo)
    {
        _access = _refresh = null;
        if (_desbloqueoActivado) await QuitarDesbloqueoAsync();
        Transicionar(evento, motivo);
    }

    /// <summary>Sin identidad presente: nadie es dueño de filas nuevas y no se ve el conteo de nadie (las filas quedan en la base).</summary>
    private void OlvidarIdentidadEnMemoria()
    {
        _sub = _dueno = _usuario = null;
        _access = _refresh = null;
        _refreshVence = null;
    }

    private async Task QuitarDesbloqueoAsync()
    {
        _cofre.Borrar();
        _desbloqueoActivado = false;
        await _db.EscribirConfiguracionAsync(ClaveRefreshSellado, "");
        await _db.EscribirConfiguracionAsync(ClaveRefreshExpira, "");
        await _db.EscribirConfiguracionAsync(ClaveDesbloqueo, "");
    }

    private async Task<string?> LeerAsync(string clave) =>
        await _db.LeerConfiguracionAsync(clave) is { Length: > 0 } valor ? valor : null;

    private static string? MotivoDe(EstadoDeSesion estado) => estado switch
    {
        EstadoDeSesion.Vencida => "Tu sesión venció: ingresá para enviar lo pendiente.",
        EstadoDeSesion.Revocada => "Tu sesión fue cerrada: ingresá de nuevo para enviar lo pendiente.",
        _ => null
    };

    private void Transicionar(EventoDeSesion evento, string? motivo, EstadoDeSesion antesDeIngresar = EstadoDeSesion.SinCuenta)
    {
        lock (_cerrojo)
        {
            var siguiente = TransicionesDeSesion.Siguiente(_estado, evento, antesDeIngresar);
            Bitacora.Log($"sesión: {_estado} --{evento}--> {siguiente}");
            _estado = siguiente;
            _motivo = motivo;
        }
        Publicar();
    }

    private void Fijar(EstadoDeSesion estado, string? motivo)
    {
        lock (_cerrojo)
        {
            _estado = estado;
            _motivo = motivo;
        }
        Bitacora.Log($"sesión: al arrancar {estado}");
        Publicar();
    }

    private SesionInformada Informar()
    {
        var (desbloqueo, motivoDelDesbloqueo) = _cofre.Evaluar(_desbloqueoActivado);
        var usuario = _estado == EstadoDeSesion.SinCuenta ? null : _usuario;
        // El motivo del estado manda; sin él, por qué no se ofrece el desbloqueo (D-03 = T1). Corrector E03: en SinCuenta
        // sólo el del estado (por qué se pide la cuenta), así la pantalla de ingreso lo puede mostrar.
        var motivo = _estado == EstadoDeSesion.SinCuenta ? _motivo : _motivo ?? motivoDelDesbloqueo;
        return new SesionInformada(ModoDeSesion.Oidc, _estado, usuario, desbloqueo, motivo);
    }

    /// <summary>Cambia el estado informado, avisa a los suscriptores (guardia, sincronizador) y emite auth.changed.</summary>
    private void Publicar()
    {
        var nueva = Informar();
        Actual = nueva;
        Cambio?.Invoke(nueva);
        _emisor.Emitir("auth.changed", new { sesion = nueva });
        PresentadorDeSesion.Actualizar(nueva);
    }
}
