// Nuevo. AC-11, humo del host (ADR-13): estilos, páginas de estado por zona y cookie sólo en documentos.
using System.Net;
using MovilidadUrbana.Asistente.Infrastructure.Sessions;
using MovilidadUrbana.Asistente.Sessions;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
public class HostTests : PruebaConHost
{
    [Test]
    [Description("GET / → 200 con la página de chat; enlaza MovilidadUrbana.Asistente.styles.css y esa hoja responde 200; /api/v1/inexistente → página de estado de la API; /inexistente → no encontrado de Blazor")]
    public async Task Host_Humo_EstilosYEstados()
    {
        var cliente = Fabrica.CreateClient();

        var raiz = await cliente.GetAsync("/");
        var html = await raiz.Content.ReadAsStringAsync();
        Assert.That(raiz.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        // Los atributos de aislamiento de CSS (b-xxxx) van entre el data-testid y el cierre de la etiqueta.
        Assert.That(html, Does.Match("data-testid=\"titulo\"[^>]*>Asistente<").And.Contain("data-testid=\"modo\""));
        var hoja = System.Text.RegularExpressions.Regex.Match(html, "href=\"(?<h>[^\"]*MovilidadUrbana\\.Asistente[^\"]*\\.styles\\.css)\"");
        Assert.That(hoja.Success, Is.True, "el HTML enlaza la hoja de estilos aislados del ensamblado nuevo");
        var estilos = await cliente.GetAsync("/" + hoja.Groups["h"].Value.TrimStart('/'));
        Assert.That(estilos.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await estilos.Content.ReadAsStringAsync(), Does.Contain(".chat-mensajes"));

        var api = await cliente.GetAsync("/api/v1/inexistente");
        Assert.That(api.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(api.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"), "la página de estado de /api");

        var web = await cliente.GetAsync("/inexistente");
        Assert.That(web.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(await web.Content.ReadAsStringAsync(), Does.Match("data-testid=\"titulo\"[^>]*>No encontrado<"), "la página de no encontrado de Blazor, no la de estado");
    }

    [Test]
    [Description("La cookie sesion-movilidad se emite en los documentos y nunca en las respuestas de /api, que usan X-Sesion-Id")]
    public async Task Host_CookieSoloEnDocumentos()
    {
        var cliente = Fabrica.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = false });

        var documento = await cliente.GetAsync("/");
        Assert.That(documento.Headers.TryGetValues("Set-Cookie", out var cookies) && cookies.Any(c => c.StartsWith(SessionContext.CookieName + "=", StringComparison.Ordinal)), Is.True);

        var api = await cliente.GetAsync("/api/v1/localidades");
        Assert.That(api.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(api.Headers.TryGetValues("Set-Cookie", out var deApi) && deApi.Any(c => c.StartsWith(SessionContext.CookieName, StringComparison.Ordinal)), Is.False);
        Assert.That(api.Headers.Contains(SessionHeaderMiddleware.HeaderName), Is.True);

        var estatico = await cliente.GetAsync("/css/Tokens.css");
        Assert.That(estatico.Headers.Contains("Set-Cookie"), Is.False, "los recursos estáticos no emiten cookie (caso base)");
    }
}
