using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FrameworkConcept.Receptor;

/// <summary>
/// Folio 038, P4 — El receptor valida el access token del cascarón: JWT firmado por el emisor OIDC, verificado contra
/// su JWKS (descubrimiento en {emisor}/.well-known/openid-configuration), con issuer, audiencia «fc-receptor» y
/// vencimiento. La identidad del dueño es el claim «sub»; queda guardada con la respuesta (DISENO §5).
///
/// Opciones (sin --oidc-emisor el receptor acepta sin token, como en A1–A3):
///   --oidc-emisor    https://203.0.113.10:8443/realms/fc-demo
///   --oidc-audiencia fc-receptor                          (por defecto)
///   --oidc-ca        claves/tls/ca_demo_tls.crt           CA con la que se confía en el TLS del emisor
///
/// Qué exige token: POST /respuestas (lo que envía el outbox) y, desde el corrector E03, GET /respuestas y
/// GET /respuestas/{clave}, que muestran sólo las del «sub» del token. /salud y el canal estático (índice firmado, P2)
/// quedan libres. El operador lee el almacén en disco («guion.sh respuestas»).
/// </summary>
internal static class AutenticacionDelReceptor
{
    /// <summary>Nombre de la política que exige un token válido (la usa RecepcionDeRespuestas).</summary>
    public const string Politica = "token-del-emisor";

    /// <summary>Registra la autenticación si hay --oidc-emisor. Devuelve si quedó activada.</summary>
    public static bool Configurar(WebApplicationBuilder builder)
    {
        var emisor = builder.Configuration["oidc-emisor"];
        if (string.IsNullOrEmpty(emisor)) return false;
        var audiencia = builder.Configuration["oidc-audiencia"] is { Length: > 0 } a ? a : "fc-receptor";
        var ca = builder.Configuration["oidc-ca"];

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                // El descubrimiento y el JWKS se piden al emisor; la firma del token se verifica con esas claves.
                o.Authority = emisor;
                o.Audience = audiencia;
                o.RequireHttpsMetadata = true;
                // Los claims quedan con sus nombres de JWT («sub», «preferred_username»), sin traducir a los de WS-*.
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = emisor,
                    ValidateAudience = true,
                    ValidAudience = audiencia,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "preferred_username"
                };
                // TLS hacia el emisor: la CA de demostración como única raíz de confianza. No se desactiva la validación.
                if (!string.IsNullOrEmpty(ca)) o.BackchannelHttpHandler = ConCaPropia(X509Certificate2.CreateFromPem(File.ReadAllText(ca)));
                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>()
                            .LogWarning("token rechazado: {Motivo}", c.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Politica, p => p.RequireAuthenticatedUser().RequireClaim("sub"));
        return true;
    }

    /// <summary>Agrega el middleware de autenticación y autorización si quedó activada.</summary>
    public static void Usar(WebApplication app, bool activada)
    {
        if (!activada) return;
        app.UseAuthentication();
        app.UseAuthorization();
        app.Logger.LogInformation("receptor: POST /respuestas exige un token de {Emisor} (audiencia {Audiencia})",
            app.Configuration["oidc-emisor"], app.Configuration["oidc-audiencia"] is { Length: > 0 } a ? a : "fc-receptor");
    }

    /// <summary>
    /// Un manejador HTTP que confía sólo en <paramref name="ca"/> (X509ChainTrustMode.CustomRootTrust). El nombre del
    /// servidor se sigue comprobando: un certificado de la CA emitido para otro host no vale.
    /// </summary>
    private static HttpClientHandler ConCaPropia(X509Certificate2 ca) => new()
    {
        ServerCertificateCustomValidationCallback = (_, certificado, _, errores) =>
        {
            if (certificado is null || (errores & SslPolicyErrors.RemoteCertificateNameMismatch) != 0
                                    || (errores & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
                return false;
            using var cadena = new X509Chain();
            cadena.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            cadena.ChainPolicy.CustomTrustStore.Add(ca);
            cadena.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;   // la CA de demostración no publica CRL
            return cadena.Build(certificado);
        }
    };
}
