using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Puente;

namespace FrameworkConcept.Core.Tests;

/// <summary>Los tipos del contrato que no dependen del actualizador: versiones, rangos y forma del índice.</summary>
[TestFixture]
public class ContratoTests
{
    [TestCase("1.0.0-alpha", "1.0.0")]
    [TestCase("1.0.0-alpha.1", "1.0.0-alpha.beta")]
    [TestCase("1.0.0-beta.2", "1.0.0-beta.11")]
    [TestCase("1.9.0", "1.10.0")]
    [TestCase("1.10.0", "2.0.0")]
    public void PrecedenciaSemVer(string menor, string mayor) =>
        Assert.That(VersionSemantica.Parse(menor) < VersionSemantica.Parse(mayor), Is.True);

    [Test]
    [Description("El build (+commit) no cuenta para ordenar, pero sí para la identidad (P7)")]
    public void ElBuildNoOrdena()
    {
        var a = VersionSemantica.Parse("1.2.3+abc");
        var b = VersionSemantica.Parse("1.2.3+def");

        Assert.That(a.CompareTo(b), Is.Zero);
        Assert.That(a, Is.Not.EqualTo(b));
    }

    [TestCase("01.0.0")]
    [TestCase("1.0")]
    [TestCase("v1.0.0")]
    public void RechazaVersionesMalFormadas(string texto) =>
        Assert.That(VersionSemantica.TryParse(texto, out _), Is.False);

    [Test]
    [Description("P3 — Rango con piso incluido y techo excluido")]
    public void RangoConTecho()
    {
        var rango = RangoDeVersion.Parse(">=1.2.0 <2.0.0");

        Assert.That(rango.Contiene(VersionSemantica.Parse("1.2.0")), Is.True);
        Assert.That(rango.Contiene(VersionSemantica.Parse("1.9.9+abc")), Is.True);
        Assert.That(rango.Contiene(VersionSemantica.Parse("1.1.9")), Is.False);
        Assert.That(rango.Contiene(VersionSemantica.Parse("2.0.0")), Is.False);
        Assert.That(RangoDeVersion.Desde(VersionSemantica.Parse("3.1.0+x")).ToString(), Is.EqualTo(">=3.1.0 <4.0.0"));
    }

    [TestCase(">=1.0.0")]
    [TestCase(">=1.0.0 <3.0.0")]
    [TestCase("<2.0.0 >=1.0.0")]
    [Description("VPI §2.6 — nunca un piso a secas; el techo es el próximo mayor")]
    public void RechazaRangosNoCanonicos(string texto) =>
        Assert.That(RangoDeVersion.TryParse(texto, out _, out _), Is.False);

    [Test]
    [Description("P3 — El mayor de MAUI llega al transporte desde Directory.Build.props")]
    public void ElTransporteConoceElMayorDeMaui() =>
        Assert.That(TransporteDelPuente.Actual.MauiMayor, Is.EqualTo(10));

    [Test]
    [Description("P12 — Cada método que toca el dispositivo nombra una entrada que existe en el catálogo")]
    public void LasCapacidadesDeLaTablaEstanEnElCatalogo()
    {
        foreach (var metodo in TablaDeMetodos.Metodos.Where(m => m.Capacidad is not null))
        {
            Assert.That(CatalogoDeCapacidades.Buscar(metodo.Capacidad!), Is.Not.Null, metodo.Nombre);
        }
    }

    [Test]
    [Description("P13.5 — Retirar una definición exige 30 días de aviso")]
    public void PlazoDeAvisoDeRetiro()
    {
        var anuncio = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

        Assert.That(new DefinicionRetirada("encuesta-movilidad", 1, anuncio, anuncio.AddDays(29)).RespetaElPlazoDeAviso, Is.False);
        Assert.That(new DefinicionRetirada("encuesta-movilidad", 1, anuncio, anuncio.AddDays(30)).RespetaElPlazoDeAviso, Is.True);
    }

    [Test]
    [Description("P2 — El índice se escribe y se lee con los nombres del contrato y vuelve igual")]
    public void ElIndiceIdaYVuelta()
    {
        var emitido = new DateTimeOffset(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);
        var indice = new IndiceDelCanal(
            IndiceDelCanal.FormatoActual, Canales.Dev, 1, "demo-2026", emitido, emitido + ParametrosDeProducto.VigenciaDelIndice,
            new EntradasDelIndice(
                [new EntradaDeContenido(VersionSemantica.Parse("1.0.0+abc1234"), "contenido/1.0.0.zip", 10, "00", Mandatory: true)],
                [new EntradaDeDefinicion("encuesta-movilidad", 1, "definiciones/encuesta-movilidad/1.json", 5, "11", RangoDeVersion.Parse(">=1.0.0 <2.0.0"))]),
            [VersionSemantica.Parse("0.9.0+old")],
            []);

        var json = JsonDelContrato.Escribir(indice);
        var leido = JsonDelContrato.Leer<IndiceDelCanal>(json);

        Assert.That(json, Does.Contain("\"expires\"").And.Contain("\"keyId\": \"demo-2026\"").And.Contain("\"compatibleConContenido\": \">=1.0.0 <2.0.0\""));
        Assert.That(leido.Expires, Is.EqualTo(indice.Expires));
        Assert.That(leido.Entradas.Contenido[0].Version.ToString(), Is.EqualTo("1.0.0+abc1234"));
        Assert.That(leido.Retiradas[0], Is.EqualTo(VersionSemantica.Parse("0.9.0+old")));
        Assert.That(RutasDelCanal.Indice(leido.Canal, leido.PuenteMayor), Is.EqualTo("dev/puente-1/indice.json"));
    }
}
