using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FrameworkConcept.Core.Auth;

// P4 — Primer ingreso: OIDC authorization code + PKCE en el navegador del sistema (RFC 8252). El cliente es
// PÚBLICO: no tiene secreto; PKCE (RFC 7636, S256) ata el code al teléfono que lo pidió. D-07 = I1: cualquier
// emisor OIDC, por eso los endpoints salen del documento de descubrimiento y no de rutas de Keycloak.

/// <summary>Lo que el cascarón sabe del emisor, fijado al compilar (-p:FcOidcAutoridad, -p:FcOidcClienteId).</summary>
/// <param name="Autoridad">El issuer, p. ej. https://203.0.113.10:8443/realms/fc-demo.</param>
/// <param name="ClienteId">Cliente público del realm (fc-demo-app).</param>
/// <param name="Redireccion">com.frameworkconcept.demo://callback (esquema privado, RFC 8252 §7.1).</param>
/// <param name="Alcances">openid profile.</param>
public sealed record ConfiguracionOidc(Uri Autoridad, string ClienteId, Uri Redireccion, string Alcances);

/// <summary>Los endpoints que se usan del documento de descubrimiento (/.well-known/openid-configuration).</summary>
public sealed record DescubrimientoOidc(string Issuer, Uri AuthorizationEndpoint, Uri TokenEndpoint, Uri? EndSessionEndpoint, Uri? RevocationEndpoint);

/// <summary>Respuesta del token endpoint, sin interpretar más que lo necesario.</summary>
/// <param name="AccessToken">JWT que el receptor valida contra el JWKS del emisor (DISENO §5).</param>
/// <param name="RefreshToken">NUNCA cruza el puente (P3). Sólo en memoria o sellado por el Keystore.</param>
/// <param name="IdToken">De acá salen sub y preferred_username.</param>
/// <param name="ExpiraEn">Vencimiento del access token.</param>
/// <param name="RefreshExpiraEn">Vencimiento del refresh token (refresh_expires_in de Keycloak; null si el emisor no lo da).</param>
public sealed record RespuestaDeTokens(string AccessToken, string? RefreshToken, string? IdToken, DateTimeOffset ExpiraEn, DateTimeOffset? RefreshExpiraEn);

/// <summary>El emisor contestó un error OAuth (RFC 6749 §5.2) en el token endpoint.</summary>
public sealed class ErrorDelEmisorException(string codigo, string? descripcion)
    : Exception($"El emisor contestó «{codigo}»{(descripcion is null ? "" : $": {descripcion}")}.")
{
    /// <summary>invalid_grant, invalid_client…</summary>
    public string Codigo { get; } = codigo;

    /// <summary>invalid_grant: el refresh token (o el code) ya no vale. Para la sesión es «revocada» o «vencida».</summary>
    public bool EsInvalidGrant => Codigo == "invalid_grant";
}

/// <summary>
/// Corrector E03 (H-4) — Qué excepción de una llamada al emisor es una FALLA (de red, del emisor o de formato) y no
/// una decisión de autenticación. En Android la conexión caída llega como System.Net.WebException, que no es
/// HttpRequestException: por eso no se enumeran tipos. Toda excepción cuenta como falla salvo la de autenticación
/// (la persona canceló, state distinto…) y la cancelación que pidió quien llama.
/// </summary>
public static class FallasDelEmisor
{
    public static bool EsFalla(Exception ex, CancellationToken ct) =>
        ex is not ExcepcionDeAutenticacion && !(ex is OperationCanceledException && ct.IsCancellationRequested);
}

/// <summary>Las piezas puras del flujo (sin red ni navegador), para probarlas en Core.Tests/Auth.</summary>
public static class FlujoOidc
{
    /// <summary>code_verifier: 32 bytes aleatorios en base64url (43 caracteres, RFC 7636 §4.1).</summary>
    public static string CrearVerificador() => Base64Url(RandomNumberGenerator.GetBytes(32));

    /// <summary>state: 16 bytes aleatorios en base64url. Ata la vuelta del navegador al pedido (RFC 6749 §10.12).</summary>
    public static string CrearState() => Base64Url(RandomNumberGenerator.GetBytes(16));

    /// <summary>code_challenge = base64url(SHA-256(verificador)), método S256 (RFC 7636 §4.2).</summary>
    public static string Desafio(string verificador) => Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verificador)));

    /// <summary>La URL de autorización con response_type=code, client_id, redirect_uri, scope, state, code_challenge y S256.</summary>
    public static Uri UrlDeAutorizacion(DescubrimientoOidc emisor, ConfiguracionOidc configuracion, string desafio, string state)
    {
        var parametros = new[]
        {
            ("response_type", "code"),
            ("client_id", configuracion.ClienteId),
            ("redirect_uri", configuracion.Redireccion.OriginalString),
            ("scope", configuracion.Alcances),
            ("state", state),
            ("code_challenge", desafio),
            ("code_challenge_method", "S256"),
            // Corrector E03 (P0) — Cada ingreso interactivo vuelve a pedir las credenciales (OIDC Core §3.1.2.1). Sin
            // esto, la sesión SSO viva del emisor en el navegador entra sola después de un Cierre o de una Revocada.
            ("prompt", ParametrosDeSesion.PromptDelIngreso)
        };
        var baseUrl = emisor.AuthorizationEndpoint.ToString();
        var separador = baseUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return new Uri(baseUrl + separador + Consulta(parametros));
    }

    /// <summary>Cuerpo del canje del code (RFC 6749 §4.1.3 + RFC 7636 §4.5). Cliente público: sin secreto.</summary>
    public static IReadOnlyList<KeyValuePair<string, string>> CuerpoDelCanje(ConfiguracionOidc configuracion, string code, string verificador) =>
    [
        new("grant_type", "authorization_code"),
        new("code", code),
        new("redirect_uri", configuracion.Redireccion.OriginalString),
        new("client_id", configuracion.ClienteId),
        new("code_verifier", verificador)
    ];

    /// <summary>
    /// Cuerpo del ingreso con usuario y clave (login tradicional, 2026-10-30): grant_type=password, cliente público
    /// sin secreto, scope = Alcances.
    ///
    /// El estándar para el login propio de una app first-party es el borrador IETF «OAuth 2.0 for First-Party
    /// Applications» (draft-ietf-oauth-first-party-apps: endpoint de autorización por desafío, que devuelve un code
    /// canjeable con PKCE y admite pasos extra como MFA). ROPC (RFC 6749 §4.3) está desaconsejado por el BCP de
    /// seguridad (RFC 9700 §2.4) y se usa acá SÓLO porque Keycloak no trae ese endpoint y este es un emisor de ensayo
    /// (Direct Access Grant habilitado en infra/keycloak/realm-fc-demo.json). Cambiar de emisor cambia este cuerpo y
    /// nada más: el contenido sigue mandando usuario y clave por auth.signInWithPassword.
    /// La clave no se registra: esta lista vive lo que dura el POST.
    /// </summary>
    public static IReadOnlyList<KeyValuePair<string, string>> CuerpoDelIngresoConClave(ConfiguracionOidc configuracion, string usuario, string clave) =>
    [
        new("grant_type", "password"),
        new("client_id", configuracion.ClienteId),
        new("username", usuario),
        new("password", clave),
        new("scope", configuracion.Alcances)
    ];

    /// <summary>Cuerpo de la renovación (RFC 6749 §6).</summary>
    public static IReadOnlyList<KeyValuePair<string, string>> CuerpoDeRenovacion(ConfiguracionOidc configuracion, string refreshToken) =>
    [
        new("grant_type", "refresh_token"),
        new("refresh_token", refreshToken),
        new("client_id", configuracion.ClienteId)
    ];

    /// <summary>
    /// Lee el documento de descubrimiento y comprueba que su issuer sea la autoridad configurada (OIDC Discovery §4.3):
    /// si no coincide, el documento no es de ese emisor.
    /// </summary>
    public static DescubrimientoOidc LeerDescubrimiento(string json, Uri autoridad)
    {
        using var doc = JsonDocument.Parse(json);
        var r = doc.RootElement;
        var issuer = Texto(r, "issuer") ?? throw new FormatException("El descubrimiento no trae «issuer».");
        if (!string.Equals(issuer.TrimEnd('/'), autoridad.OriginalString.TrimEnd('/'), StringComparison.Ordinal))
            throw new FormatException($"El issuer del descubrimiento es «{issuer}» y se esperaba «{autoridad}».");
        return new DescubrimientoOidc(
            issuer,
            new Uri(Texto(r, "authorization_endpoint") ?? throw new FormatException("Falta authorization_endpoint.")),
            new Uri(Texto(r, "token_endpoint") ?? throw new FormatException("Falta token_endpoint.")),
            Texto(r, "end_session_endpoint") is { } fin ? new Uri(fin) : null,
            Texto(r, "revocation_endpoint") is { } rev ? new Uri(rev) : null);
    }

    /// <summary>Lee la respuesta JSON del token endpoint. <paramref name="ahora"/> para calcular los vencimientos.</summary>
    public static RespuestaDeTokens LeerRespuestaDeTokens(string json, DateTimeOffset ahora)
    {
        using var doc = JsonDocument.Parse(json);
        var r = doc.RootElement;
        if (Texto(r, "error") is { } error) throw new ErrorDelEmisorException(error, Texto(r, "error_description"));

        var access = Texto(r, "access_token") ?? throw new FormatException("La respuesta del emisor no trae access_token.");
        var expiraEn = ahora.AddSeconds(Segundos(r, "expires_in") ?? 0);
        // Keycloak informa refresh_expires_in; 0 quiere decir «sin vencimiento propio» (offline). Otro emisor puede no
        // informarlo: entonces el vencimiento se conoce sólo cuando el emisor rechaza la renovación.
        var refreshSegundos = Segundos(r, "refresh_expires_in");
        DateTimeOffset? refreshExpiraEn = refreshSegundos is > 0 ? ahora.AddSeconds(refreshSegundos.Value) : null;
        return new RespuestaDeTokens(access, Texto(r, "refresh_token"), Texto(r, "id_token"), expiraEn, refreshExpiraEn);
    }

    /// <summary>
    /// Un error OAuth del token endpoint (cuerpo de un 400/401), o null si el cuerpo no lo es.
    /// </summary>
    public static ErrorDelEmisorException? LeerError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object && Texto(doc.RootElement, "error") is { } codigo
                ? new ErrorDelEmisorException(codigo, Texto(doc.RootElement, "error_description"))
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>sub y preferred_username de las claims del id_token (sin validar firma: llegó por TLS del token endpoint, OIDC Core §3.1.3.7).</summary>
    public static (string Sub, string? Usuario) LeerIdentidad(string idToken)
    {
        var partes = idToken.Split('.');
        if (partes.Length < 2) throw new FormatException("El id_token no es un JWT.");
        using var doc = JsonDocument.Parse(DesdeBase64Url(partes[1]));
        var sub = Texto(doc.RootElement, "sub");
        if (string.IsNullOrEmpty(sub)) throw new FormatException("El id_token no trae «sub».");
        return (sub, Texto(doc.RootElement, "preferred_username"));
    }

    /// <summary>
    /// OIDC Core §3.1.3.7 (puntos 2, 3 y 9): aunque la firma no se verifique (llegó por TLS del token endpoint), el
    /// id_token tiene que ser de este emisor (iss), para este cliente (aud; azp si hay varias audiencias) y vigente
    /// (exp, con un minuto de tolerancia de reloj). El ingreso con clave no manda nonce: esto es lo único que ata el
    /// id_token a este canje. Lanza AutenticacionFallida si no cumple.
    /// </summary>
    public static void ValidarIdToken(string idToken, string issuer, string clienteId, DateTimeOffset ahora)
    {
        var partes = idToken.Split('.');
        if (partes.Length < 2) throw ExcepcionDeAutenticacion.Fallida("El emisor devolvió un id_token ajeno.");
        using var doc = JsonDocument.Parse(DesdeBase64Url(partes[1]));
        var r = doc.RootElement;

        var iss = Texto(r, "iss");
        var audiencias = r.TryGetProperty("aud", out var aud)
            ? aud.ValueKind == JsonValueKind.Array
                ? aud.EnumerateArray().Where(a => a.ValueKind == JsonValueKind.String).Select(a => a.GetString()!).ToArray()
                : aud.ValueKind == JsonValueKind.String ? [aud.GetString()!] : []
            : [];
        var azp = Texto(r, "azp");
        var exp = r.TryGetProperty("exp", out var e) && e.TryGetInt64(out var segundos) ? DateTimeOffset.FromUnixTimeSeconds(segundos) : (DateTimeOffset?)null;

        var propio = string.Equals(iss?.TrimEnd('/'), issuer.TrimEnd('/'), StringComparison.Ordinal)
            && audiencias.Contains(clienteId, StringComparer.Ordinal)
            && (audiencias.Length == 1 || azp == clienteId)
            && exp is { } vence && vence + TimeSpan.FromMinutes(1) > ahora;
        if (!propio) throw ExcepcionDeAutenticacion.Fallida("El emisor devolvió un id_token ajeno.");
    }

    public static string Base64Url(byte[] datos) =>
        Convert.ToBase64String(datos).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] DesdeBase64Url(string texto)
    {
        var s = texto.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(s.PadRight(s.Length + (4 - s.Length % 4) % 4, '='));
    }

    private static string Consulta(IEnumerable<(string Clave, string Valor)> parametros) =>
        string.Join("&", parametros.Select(p => Uri.EscapeDataString(p.Clave) + "=" + Uri.EscapeDataString(p.Valor)));

    private static string? Texto(JsonElement r, string nombre) =>
        r.TryGetProperty(nombre, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static double? Segundos(JsonElement r, string nombre) =>
        r.TryGetProperty(nombre, out var v) ? v.ValueKind switch
        {
            JsonValueKind.Number => v.GetDouble(),
            JsonValueKind.String when double.TryParse(v.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
            _ => null
        } : null;
}

/// <summary>Criterios de la demo para la sesión. No son valores de producto del folio 036.</summary>
public static class ParametrosDeSesion
{
    /// <summary>D-03 = T1 — API mínima del desbloqueo local (setUserAuthenticationParameters).</summary>
    public const int ApiMinimaDelDesbloqueoLocal = 30;

    /// <summary>Borrador §4 «re-pedir la seguridad del teléfono al volver de segundo plano después de X minutos».</summary>
    public static readonly TimeSpan BloqueoTrasSegundoPlano = TimeSpan.FromMinutes(5);

    /// <summary>El access token se renueva en segundo plano cuando le falta menos que esto.</summary>
    public static readonly TimeSpan RenovarConAnticipacion = TimeSpan.FromSeconds(30);

    /// <summary>Redirect URI registrado en el realm de ensayo (infra/keycloak/realm-fc-demo.json).</summary>
    public const string Redireccion = "com.frameworkconcept.demo://callback";

    public const string ClienteIdPorDefecto = "fc-demo-app";

    public const string Alcances = "openid profile";

    /// <summary>invalid_grant al ingreso con clave (login tradicional).</summary>
    public const string MotivoClaveIncorrecta = "Usuario o clave incorrectos.";

    /// <summary>prompt=login: el emisor re-autentica aunque tenga una sesión SSO viva (corrector E03, P0).</summary>
    public const string PromptDelIngreso = "login";

    /// <summary>Audiencia que el receptor exige en el access token (mapper de audiencia del realm).</summary>
    public const string AudienciaDelReceptor = "fc-receptor";
}
