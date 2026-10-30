using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Shell.Red;

namespace FrameworkConcept.Shell.Auth;

/// <summary>
/// P4, D-07 = I1 — Las llamadas HTTP al emisor OIDC: descubrimiento, canje del code, renovación y revocación. Las
/// piezas puras (cuerpos, lectura de respuestas) son de Core/Auth/Oidc.cs; acá sólo está el transporte.
///
/// El HttpClient NO lleva ManejadorDeSesion (todavía no hay token, DISENO §3.3 paso 5) y SÍ ManejadorDeRed: la llamada
/// queda en el registro de Diagnóstico. HttpClientHandler usa el manejador nativo de Android, que confía en la CA de
/// demostración por network_security_config (sólo Debug).
/// </summary>
public sealed class ClienteDelEmisor(ConfiguracionOidc configuracion, IMonitorDeRed red)
{
    private readonly HttpClient _http = new(new ManejadorDeRed(red, "emisor") { InnerHandler = new HttpClientHandler() })
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    private DescubrimientoOidc? _descubrimiento;

    public ConfiguracionOidc Configuracion { get; } = configuracion;

    /// <summary>GET {autoridad}/.well-known/openid-configuration, una vez por proceso.</summary>
    public async Task<DescubrimientoOidc> DescubrirAsync(CancellationToken ct)
    {
        if (_descubrimiento is not null) return _descubrimiento;
        var url = new Uri(Configuracion.Autoridad.OriginalString.TrimEnd('/') + "/.well-known/openid-configuration");
        var json = await _http.GetStringAsync(url, ct);
        return _descubrimiento = FlujoOidc.LeerDescubrimiento(json, Configuracion.Autoridad);
    }

    /// <summary>Canje del code con el code_verifier (RFC 7636 §4.5).</summary>
    public async Task<RespuestaDeTokens> CanjearAsync(string code, string verificador, CancellationToken ct) =>
        await PostearAsync((await DescubrirAsync(ct)).TokenEndpoint, FlujoOidc.CuerpoDelCanje(Configuracion, code, verificador), ct);

    /// <summary>
    /// Login tradicional (first-party, ROPC en el emisor de ensayo; ver FlujoOidc.CuerpoDelIngresoConClave). La clave
    /// sólo viaja en el cuerpo del POST: ni esta clase ni ManejadorDeRed registran cuerpos (sólo URL y código HTTP).
    /// </summary>
    public async Task<RespuestaDeTokens> IngresarConClaveAsync(string usuario, string clave, CancellationToken ct) =>
        await PostearAsync((await DescubrirAsync(ct)).TokenEndpoint, FlujoOidc.CuerpoDelIngresoConClave(Configuracion, usuario, clave), ct);

    /// <summary>grant_type=refresh_token (RFC 6749 §6). invalid_grant llega como <see cref="ErrorDelEmisorException"/>.</summary>
    public async Task<RespuestaDeTokens> RenovarAsync(string refreshToken, CancellationToken ct) =>
        await PostearAsync((await DescubrirAsync(ct)).TokenEndpoint, FlujoOidc.CuerpoDeRenovacion(Configuracion, refreshToken), ct);

    /// <summary>
    /// Cierre de sesión: revoca el refresh token en el emisor (RFC 7009) si el descubrimiento publica el endpoint.
    /// Es de mejor esfuerzo: sin red no se espera (DISENO §3.8) y los tokens locales se borran igual.
    /// </summary>
    public async Task RevocarAsync(string refreshToken, CancellationToken ct)
    {
        var descubrimiento = await DescubrirAsync(ct);
        if (descubrimiento.RevocationEndpoint is not { } endpoint) return;
        using var cuerpo = new FormUrlEncodedContent(
        [
            new("token", refreshToken),
            new("token_type_hint", "refresh_token"),
            new("client_id", Configuracion.ClienteId)
        ]);
        using var respuesta = await _http.PostAsync(endpoint, cuerpo, ct);
        Bitacora.Log($"sesión: revocación en el emisor → HTTP {(int)respuesta.StatusCode}");
    }

    private async Task<RespuestaDeTokens> PostearAsync(Uri endpoint, IReadOnlyList<KeyValuePair<string, string>> campos, CancellationToken ct)
    {
        using var cuerpo = new FormUrlEncodedContent(campos);
        using var respuesta = await _http.PostAsync(endpoint, cuerpo, ct);
        var json = await respuesta.Content.ReadAsStringAsync(ct);
        if (respuesta.IsSuccessStatusCode) return FlujoOidc.LeerRespuestaDeTokens(json, DateTimeOffset.UtcNow);
        // RFC 6749 §5.2: 400/401 con {"error": …}. Otra cosa (5xx, HTML de un proxy) es una falla de red.
        throw (Exception?)FlujoOidc.LeerError(json) ?? new HttpRequestException($"El emisor contestó HTTP {(int)respuesta.StatusCode}.");
    }
}
