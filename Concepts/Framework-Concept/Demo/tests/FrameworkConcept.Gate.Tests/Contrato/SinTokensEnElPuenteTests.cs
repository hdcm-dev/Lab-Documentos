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
        var campos = TablaDeMetodos.Metodos
            .SelectMany(m => m.Parametros.Concat(m.Respuesta).Select(c => $"{m.Nombre}: {c.Nombre} ({c.Tipo})"))
            .Concat(TablaDeMetodos.Eventos.SelectMany(e => e.Parametros.Select(c => $"evento {e.Nombre}: {c.Nombre} ({c.Tipo})")));

        Assert.That(campos.Where(Prohibido).ToList(), Is.Empty);
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
