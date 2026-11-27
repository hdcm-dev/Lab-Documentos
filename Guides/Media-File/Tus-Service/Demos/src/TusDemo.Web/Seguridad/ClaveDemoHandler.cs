using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TusDemo.Web.Archivos;

namespace TusDemo.Web.Seguridad;

/// <summary>
/// Autenticación mínima de la demo: el pedido trae la cabecera X-Clave-Demo con la clave compartida.
/// Sirve para mostrar dónde se engancha la autenticación; un producto usa JWT, cookies u otro esquema real.
/// </summary>
public sealed class ClaveDemoHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<OpcionesArchivos> archivos)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string Esquema = "ClaveDemo";
    public const string Cabecera = "X-Clave-Demo";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Cabecera, out var valor))
            return Task.FromResult(AuthenticateResult.NoResult());

        var esperada = Encoding.UTF8.GetBytes(archivos.Value.ClaveDemo);
        var recibida = Encoding.UTF8.GetBytes(valor.ToString());
        if (esperada.Length == 0 || !CryptographicOperations.FixedTimeEquals(esperada, recibida))
            return Task.FromResult(AuthenticateResult.Fail("Clave inválida"));

        var identidad = new ClaimsIdentity([new Claim(ClaimTypes.Name, "demo")], Esquema);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identidad), Esquema);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
