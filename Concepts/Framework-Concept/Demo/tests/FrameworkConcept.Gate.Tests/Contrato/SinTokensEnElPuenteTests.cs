using System.Reflection;
using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Core.Red;

namespace FrameworkConcept.Gate.Tests.Contrato;

/// <summary>
/// P3 (folio 035) — «El refresh token nunca cruza el puente». Desde el puente 1.1 (folio 038) el contenido ve la sesión,
/// así que la compuerta mira que ningún campo del contrato ni de los tipos que viajan por él nombre un token o una
/// credencial. Es una red gruesa: un nombre inocente que contenga «token» la hace saltar, y entonces se discute en el PR.
/// </summary>
[TestFixture]
public class SinTokensEnElPuenteTests
{
    // «clave» no entra: claveIdempotencia es legítima (P13.6).
    private static readonly string[] Prohibidas = ["token", "secret", "password", "contrasena", "credencial", "verifier", "verificador"];

    private static bool Prohibido(string nombre) =>
        Prohibidas.Any(p => nombre.Contains(p, StringComparison.OrdinalIgnoreCase));

    [Test]
    [Description("P3 — Ningún parámetro, respuesta ni evento de la tabla de métodos nombra un token o una credencial")]
    public void LaTablaNoLlevaTokens()
    {
        // Login tradicional (puente 1.2): la única excepción es el método del ingreso first-party; sus campos se
        // revisan en LaCredencialCruzaUnaSolaVez. Su respuesta sí entra en la revisión general.
        var campos = TablaDeMetodos.Metodos
            .SelectMany(m => (m.Nombre == MetodoDeLaCredencial ? m.Respuesta : m.Parametros.Concat(m.Respuesta))
                .Select(c => $"{m.Nombre.Replace(MetodoDeLaCredencial, "")}: {c.Nombre} ({c.Tipo})"))
            .Concat(TablaDeMetodos.Eventos.SelectMany(e => e.Parametros.Select(c => $"evento {e.Nombre}: {c.Nombre} ({c.Tipo})")));

        Assert.That(campos.Where(Prohibido).ToList(), Is.Empty);
    }

    private const string MetodoDeLaCredencial = "auth.signInWithPassword";

    [Test]
    [Description("Login tradicional, regla 2 — La credencial cruza el puente UNA vez, en un método específico, y nunca vuelve")]
    public void LaCredencialCruzaUnaSolaVez()
    {
        var metodo = TablaDeMetodos.Buscar(MetodoDeLaCredencial);
        Assert.That(metodo, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(metodo!.Parametros.Select(c => c.Nombre), Is.EqualTo(new[] { "usuario", "clave" }));
            Assert.That(metodo.Respuesta.Select(c => c.Tipo), Is.EqualTo(new[] { "SesionInformada" }));
            Assert.That(TablaDeMetodos.Metodos.Where(m => m.Parametros.Any(c => c.Nombre == "clave")).Select(m => m.Nombre),
                Is.EqualTo(new[] { MetodoDeLaCredencial }), "otro método recibe «clave»");
        });
    }

    [TestCase(typeof(SesionInformada))]
    [TestCase(typeof(EvaluacionDeAccion))]
    [TestCase(typeof(EstadoDeRedCombinado))]
    [Description("P3 — Los tipos que viajan por el puente no tienen propiedades de token o credencial")]
    public void LosTiposDelPuenteNoLlevanTokens(Type tipo)
    {
        var propiedades = tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name);
        Assert.That(propiedades.Where(Prohibido).ToList(), Is.Empty, $"{tipo.Name} tiene propiedades prohibidas");
    }
}
