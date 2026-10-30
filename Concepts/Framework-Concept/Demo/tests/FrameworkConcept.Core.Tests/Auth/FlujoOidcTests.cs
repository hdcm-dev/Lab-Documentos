using System.Text;
using System.Text.RegularExpressions;
using FrameworkConcept.Core.Auth;

namespace FrameworkConcept.Core.Tests.Auth;

/// <summary>Las piezas puras del primer ingreso OIDC code + PKCE (DISENO §3.3; RFC 7636, RFC 8252).</summary>
[TestFixture]
public partial class FlujoOidcTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    private static readonly ConfiguracionOidc Configuracion = new(
        new Uri("https://203.0.113.10:8443/realms/fc-demo"), "fc-demo-app",
        new Uri(ParametrosDeSesion.Redireccion), ParametrosDeSesion.Alcances);

    private static readonly DescubrimientoOidc Emisor = new(
        "https://203.0.113.10:8443/realms/fc-demo",
        new Uri("https://203.0.113.10:8443/realms/fc-demo/protocol/openid-connect/auth"),
        new Uri("https://203.0.113.10:8443/realms/fc-demo/protocol/openid-connect/token"), null, null);

    [GeneratedRegex("^[A-Za-z0-9_-]{43}$")]
    private static partial Regex Base64Url43();

    [Test]
    public void VectorDelApendiceBDeRfc7636() =>
        Assert.That(FlujoOidc.Desafio("dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"),
            Is.EqualTo("E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM"));

    [Test]
    public void ElVerificadorTiene43CaracteresBase64UrlYCambiaCadaVez()
    {
        var a = FlujoOidc.CrearVerificador();
        var b = FlujoOidc.CrearVerificador();
        Assert.That(Base64Url43().IsMatch(a), a);
        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void LaUrlDeAutorizacionLlevaLoQuePideLaRfc()
    {
        var url = FlujoOidc.UrlDeAutorizacion(Emisor, Configuracion, "DESAFIO", "ESTADO");
        var q = System.Web.HttpUtility.ParseQueryString(url.Query);
        Assert.Multiple(() =>
        {
            Assert.That(url.GetLeftPart(UriPartial.Path), Is.EqualTo(Emisor.AuthorizationEndpoint.ToString()));
            Assert.That(q["response_type"], Is.EqualTo("code"));
            Assert.That(q["client_id"], Is.EqualTo("fc-demo-app"));
            Assert.That(q["redirect_uri"], Is.EqualTo("com.frameworkconcept.demo://callback"));
            Assert.That(q["scope"], Is.EqualTo("openid profile"));
            Assert.That(q["state"], Is.EqualTo("ESTADO"));
            Assert.That(q["code_challenge"], Is.EqualTo("DESAFIO"));
            Assert.That(q["code_challenge_method"], Is.EqualTo("S256"));
            // Corrector E03 (P0): la sesión SSO del emisor no alcanza para entrar; siempre se piden las credenciales.
            Assert.That(q["prompt"], Is.EqualTo("login"));
            // El redirect va codificado: «://» sin codificar rompería la consulta.
            Assert.That(url.AbsoluteUri, Does.Contain("redirect_uri=com.frameworkconcept.demo%3A%2F%2Fcallback"));
        });
    }

    [Test]
    public void ElCanjeLlevaElVerificadorYNoLlevaSecreto()
    {
        var cuerpo = FlujoOidc.CuerpoDelCanje(Configuracion, "CODE", "VERIF").ToDictionary();
        Assert.That(cuerpo["grant_type"], Is.EqualTo("authorization_code"));
        Assert.That(cuerpo["code_verifier"], Is.EqualTo("VERIF"));
        Assert.That(cuerpo["redirect_uri"], Is.EqualTo(ParametrosDeSesion.Redireccion));
        Assert.That(cuerpo.ContainsKey("client_secret"), Is.False);
    }

    [Test]
    [Description("Login tradicional: grant_type=password del cliente público, sin secreto, con los alcances de la sesión")]
    public void ElIngresoConClaveEsRopcDelClientePublico()
    {
        var campos = FlujoOidc.CuerpoDelIngresoConClave(Configuracion, "encuestador", "CLAVE");
        Assert.That(campos.Select(c => c.Key), Is.EqualTo(new[] { "grant_type", "client_id", "username", "password", "scope" }));
        var cuerpo = campos.ToDictionary();
        Assert.Multiple(() =>
        {
            Assert.That(cuerpo["grant_type"], Is.EqualTo("password"));
            Assert.That(cuerpo["client_id"], Is.EqualTo(Configuracion.ClienteId));
            Assert.That(cuerpo["username"], Is.EqualTo("encuestador"));
            Assert.That(cuerpo["password"], Is.EqualTo("CLAVE"));
            Assert.That(cuerpo["scope"], Is.EqualTo(Configuracion.Alcances));
            Assert.That(cuerpo.ContainsKey("client_secret"), Is.False);
        });
    }

    [Test]
    public void LeerRespuestaDeTokensCalculaVencimientosConAhora()
    {
        const string json = """{"access_token":"AAA","expires_in":300,"refresh_expires_in":1799,"refresh_token":"RRR","token_type":"Bearer","id_token":"III"}""";
        var r = FlujoOidc.LeerRespuestaDeTokens(json, Ahora);
        Assert.Multiple(() =>
        {
            Assert.That(r.AccessToken, Is.EqualTo("AAA"));
            Assert.That(r.RefreshToken, Is.EqualTo("RRR"));
            Assert.That(r.IdToken, Is.EqualTo("III"));
            Assert.That(r.ExpiraEn, Is.EqualTo(Ahora.AddSeconds(300)));
            Assert.That(r.RefreshExpiraEn, Is.EqualTo(Ahora.AddSeconds(1799)));
        });
    }

    [Test]
    public void SinRefreshExpiresInElVencimientoDelRefreshEsDesconocido() =>
        Assert.That(FlujoOidc.LeerRespuestaDeTokens("""{"access_token":"A","expires_in":60,"refresh_token":"R"}""", Ahora).RefreshExpiraEn, Is.Null);

    [Test]
    public void UnErrorDelEmisorSeLeeComoTal()
    {
        var e = Assert.Throws<ErrorDelEmisorException>(() =>
            FlujoOidc.LeerRespuestaDeTokens("""{"error":"invalid_grant","error_description":"Session not active"}""", Ahora));
        Assert.That(e!.EsInvalidGrant, Is.True);
        Assert.That(FlujoOidc.LeerError("""{"error":"invalid_grant"}""")?.EsInvalidGrant, Is.True);
        Assert.That(FlujoOidc.LeerError("<html>502</html>"), Is.Null);
    }

    [Test]
    public void LeerIdentidadSacaSubYPreferredUsername()
    {
        static string B64(string s) => FlujoOidc.Base64Url(Encoding.UTF8.GetBytes(s));
        // Una carga cuyo base64 necesita relleno, para ver que se decodifica bien sin «=».
        var jwt = B64("""{"alg":"RS256"}""") + "." + B64("""{"sub":"2b1c-ñ","preferred_username":"encuestador","x":1}""") + ".firma";
        Assert.That(FlujoOidc.LeerIdentidad(jwt), Is.EqualTo(("2b1c-ñ", (string?)"encuestador")));
    }

    [TestCase("""{"iss":"https://e/realms/fc-demo","aud":"fc-demo-app","exp":EXP}""", true)]
    [TestCase("""{"iss":"https://e/realms/fc-demo/","aud":["fc-demo-app","account"],"azp":"fc-demo-app","exp":EXP}""", true)]
    [TestCase("""{"iss":"https://otro/realms/fc-demo","aud":"fc-demo-app","exp":EXP}""", false)]
    [TestCase("""{"iss":"https://e/realms/fc-demo","aud":"otra-app","exp":EXP}""", false)]
    [TestCase("""{"iss":"https://e/realms/fc-demo","aud":["fc-demo-app","account"],"azp":"account","exp":EXP}""", false)]
    [TestCase("""{"iss":"https://e/realms/fc-demo","aud":"fc-demo-app","exp":VIEJO}""", false)]
    [TestCase("""{"iss":"https://e/realms/fc-demo","aud":"fc-demo-app"}""", false)]
    public void ElIdTokenTieneQueSerDeEsteEmisorYCliente(string carga, bool propio)
    {
        carga = carga.Replace("EXP", Ahora.AddMinutes(5).ToUnixTimeSeconds().ToString())
                     .Replace("VIEJO", Ahora.AddMinutes(-5).ToUnixTimeSeconds().ToString());
        var jwt = FlujoOidc.Base64Url("{}"u8.ToArray()) + "." + FlujoOidc.Base64Url(Encoding.UTF8.GetBytes(carga)) + ".f";
        void Validar() => FlujoOidc.ValidarIdToken(jwt, "https://e/realms/fc-demo", "fc-demo-app", Ahora);
        if (propio) Assert.DoesNotThrow(Validar);
        else Assert.That(Assert.Throws<ExcepcionDeAutenticacion>(Validar)!.Message, Is.EqualTo("El emisor devolvió un id_token ajeno."));
    }

    [Test]
    public void UnIdTokenSinSubNoEsIdentidad()
    {
        var jwt = FlujoOidc.Base64Url("{}"u8.ToArray()) + "." + FlujoOidc.Base64Url("""{"preferred_username":"x"}"""u8.ToArray()) + ".f";
        Assert.Throws<FormatException>(() => FlujoOidc.LeerIdentidad(jwt));
    }

    [Test]
    public void ElDescubrimientoTieneQueSerDelEmisorConfigurado()
    {
        const string json = """
            {"issuer":"https://203.0.113.10:8443/realms/fc-demo",
             "authorization_endpoint":"https://203.0.113.10:8443/realms/fc-demo/protocol/openid-connect/auth",
             "token_endpoint":"https://203.0.113.10:8443/realms/fc-demo/protocol/openid-connect/token",
             "end_session_endpoint":"https://203.0.113.10:8443/realms/fc-demo/protocol/openid-connect/logout",
             "revocation_endpoint":"https://203.0.113.10:8443/realms/fc-demo/protocol/openid-connect/revoke"}
            """;
        var d = FlujoOidc.LeerDescubrimiento(json, Configuracion.Autoridad);
        Assert.That(d.TokenEndpoint.AbsolutePath, Does.EndWith("/token"));
        Assert.That(d.RevocationEndpoint, Is.Not.Null);
        Assert.Throws<FormatException>(() => FlujoOidc.LeerDescubrimiento(json, new Uri("https://otro/realms/fc-demo")));
    }

    // --- Corrector E03 (H-4): una caída de red durante el ingreso es una falla, no un cuelgue en Ingresando ---

    [Test]
    public void WebExceptionEsUnaFallaDelEmisor() =>
        // En Android la conexión caída llegó como WebException (no HttpRequestException) y escapaba del catch.
        Assert.That(FallasDelEmisor.EsFalla(new System.Net.WebException("conexión caída"), CancellationToken.None), Is.True);

    [TestCase(typeof(HttpRequestException))]
    [TestCase(typeof(IOException))]
    [TestCase(typeof(FormatException))]
    [TestCase(typeof(TaskCanceledException))]   // el Timeout del HttpClient, sin cancelación pedida
    [TestCase(typeof(InvalidOperationException))]
    public void TodaExcepcionQueNoEsDeAutenticacionEsFalla(Type tipo) =>
        Assert.That(FallasDelEmisor.EsFalla((Exception)Activator.CreateInstance(tipo)!, CancellationToken.None), Is.True);

    [Test]
    public void LaExcepcionDeAutenticacionNoEsFalla() =>
        Assert.That(FallasDelEmisor.EsFalla(ExcepcionDeAutenticacion.Cancelada("cerró el navegador"), CancellationToken.None), Is.False);

    [Test]
    public void LaCancelacionPedidaNoEsFalla()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Assert.That(FallasDelEmisor.EsFalla(new OperationCanceledException(cts.Token), cts.Token), Is.False);
    }

    [Test]
    public void IngresandoMasFallaVuelveAlEstadoPrevio()
    {
        // La transición que dispara la falla de red durante el ingreso (antes quedaba Ingresando para siempre).
        foreach (var antes in new[] { EstadoDeSesion.SinCuenta, EstadoDeSesion.Vencida, EstadoDeSesion.Revocada })
            Assert.That(TransicionesDeSesion.Siguiente(EstadoDeSesion.Ingresando, EventoDeSesion.IngresoFallido, antes), Is.EqualTo(antes));
    }
}
