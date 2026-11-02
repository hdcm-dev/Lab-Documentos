using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Audiencias;

// Defectos que se pueden plantar para la mitad roja de cada par.
public record Opciones(bool IgnorarAlcance = false, bool EndpointSinGrupo = false, bool OpenApiAbierto = false);

// Sesión simulada: el rol y la aplicación llegan en encabezados. Sin encabezado, no hay sesión.
public class SesionDemo(IOptionsMonitor<AuthenticationSchemeOptions> o, ILoggerFactory l, UrlEncoder e)
    : AuthenticationHandler<AuthenticationSchemeOptions>(o, l, e)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Demo-Rol", out var rol)) return Task.FromResult(AuthenticateResult.NoResult());
        var claims = new List<Claim> { new(ClaimTypes.Role, rol!), new("app", Request.Headers["X-Demo-App"].ToString()) };
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity(claims, "Demo")), "Demo")));
    }
}

public static class Servidor
{
    public const string NoEntrega = "404";  // Ayuda: 404, elegido y declarado (A-06, R3-05); no revela que la página existe.

    public static async Task<WebApplication> Levantar(Catalogo cat, Opciones op)
    {
        var b = WebApplication.CreateBuilder();
        b.Logging.ClearProviders();
        b.WebHost.UseTestServer();
        b.Services.AddAuthentication("Demo").AddScheme<AuthenticationSchemeOptions, SesionDemo>("Demo", null);
        b.Services.AddAuthorization(o => o.AddPolicy("exposicion", p => p.RequireAssertion(ctx =>
            op.OpenApiAbierto || (ctx.Resource as HttpContext)?.GetRouteValue("documentName") as string != "integradores"
            || ctx.User.IsInRole("Integrator"))));
        // E-4: un documento OpenAPI por exposición.
        b.Services.AddOpenApi("publico");
        b.Services.AddOpenApi("integradores");
        var app = b.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapOpenApi().RequireAuthorization("exposicion");

        app.MapGet("/api/avisos", () => new[] { "FICTICIO: corte programado" }).WithGroupName("publico");
        app.MapGet("/api/integracion/eventos", () => new[] { "FICTICIO: evento de integración" })
            .WithGroupName("integradores").RequireAuthorization(new AuthorizeAttribute { Roles = "Integrator" });
        if (op.EndpointSinGrupo)
            app.MapGet("/api/diagnostico", () => "FICTICIO: diagnóstico");

        // E-2: el filtro corre en el servidor, con la audiencia y la aplicación de la sesión.
        app.MapGet("/ayuda/{**slug}", (string slug, ClaimsPrincipal u) =>
        {
            var aud = cat.DelRol(u.FindFirstValue(ClaimTypes.Role));
            if (aud is null || !cat.Paginas.TryGetValue(slug, out var p) || !cat.Expandir(aud.Id).Overlaps(p.Meta.Delivery))
                return Results.NotFound();
            if (!op.IgnorarAlcance && aud.Alcance == "por-aplicacion" && p.App is { } a && a != u.FindFirstValue("app"))
                return Results.NotFound();
            return Results.Text(p.Fuente, "text/markdown; charset=utf-8");
        }).ExcludeFromDescription();

        await app.StartAsync();
        return app;
    }

    public static async Task<(int Codigo, string Cuerpo)> Get(WebApplication app, string ruta, string? rol = null, string? aplicacion = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, ruta);
        if (rol is not null) req.Headers.Add("X-Demo-Rol", rol);
        if (aplicacion is not null) req.Headers.Add("X-Demo-App", aplicacion);
        var r = await app.GetTestClient().SendAsync(req);
        return ((int)r.StatusCode, await r.Content.ReadAsStringAsync());
    }

    // contrato-por-exposicion: todo punto final descripto tiene que pertenecer a un grupo.
    public static List<string> SinGrupo(WebApplication app) =>
        app.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.IExcludeFromDescriptionMetadata>() is not { ExcludeFromDescription: true }
                        && e.Metadata.GetMetadata<IEndpointGroupNameMetadata>() is null)
            .Select(e => $"contrato-por-exposicion: {e.RoutePattern.RawText} sin grupo cae en todos los documentos").ToList();
}
