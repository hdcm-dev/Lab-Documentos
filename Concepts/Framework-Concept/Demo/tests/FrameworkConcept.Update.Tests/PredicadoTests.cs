using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Contenido;
using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Core.Update;
using static FrameworkConcept.Update.Tests.Banco;

namespace FrameworkConcept.Update.Tests;

/// <summary>
/// P3 — El predicado único de compatibilidad. P3 pide que cada dimensión rechace al menos un paquete en los rojos
/// provocados (si alguna nunca rechaza, se quita): acá hay un rechazo por dimensión.
/// </summary>
[TestFixture]
public class PredicadoTests
{
    private readonly PredicadoDeCompatibilidad _predicado = new();
    private EntornoDePrueba _entorno = null!;

    // NUnit usa una sola instancia de la clase para todas las pruebas: el entorno se rehace en cada una.
    [SetUp]
    public void SetUp() => _entorno = new EntornoDePrueba();

    private static RequisitoDeCapacidad Requiere(string id, string rango) => new(id, RangoDeVersion.Parse(rango));

    private ResultadoDeCompatibilidad Evaluar(ManifiestoContenido manifiesto) => _predicado.Evaluar(manifiesto, _entorno);

    [Test]
    public void TodoEnRango_Compatible()
    {
        var resultado = Evaluar(Paquetes.Manifiesto(V("1.0.0"),
            requires: [Requiere("device.info", ">=1.0.0 <2.0.0"), Requiere("network.status", ">=1.0.0 <2.0.0")]));

        Assert.That(resultado.EsCompatible, Is.True);
        Assert.That(resultado.Motivos, Is.Empty);
    }

    [TestCase("1.0.0", ">=1.0.0 <2.0.0", true, TestName = "Puente en el piso: compatible")]
    [TestCase("1.9.3", ">=1.2.0 <2.0.0", true, TestName = "Puente dentro del rango: compatible")]
    [TestCase("1.1.0", ">=1.2.0 <2.0.0", false, TestName = "Puente bajo el piso: incompatible")]
    [TestCase("2.0.0", ">=1.0.0 <2.0.0", false, TestName = "Puente en el techo (mayor nuevo): incompatible")]
    public void Puente(string puente, string rango, bool esperado)
    {
        _entorno.VersionDelPuente = V(puente);

        Assert.That(Evaluar(Paquetes.Manifiesto(V("1.0.0"), puente: rango)).EsCompatible, Is.EqualTo(esperado));
    }

    [Test]
    [Description("P12 — requires con un id que el catálogo no tiene: no se activa")]
    public void RequiresAusenteDelCatalogo()
    {
        var resultado = Evaluar(Paquetes.Manifiesto(V("1.0.0"), requires: [Requiere("camera.capture", ">=1.0.0 <2.0.0")]));

        Assert.That(resultado.EsCompatible, Is.False);
        Assert.That(resultado.Motivos, Has.One.Contains("camera.capture"));
    }

    [Test]
    [Description("P12 — requires con la entrada presente pero en otra versión: no se activa")]
    public void RequiresFueraDeRango()
    {
        var resultado = Evaluar(Paquetes.Manifiesto(V("1.0.0"), requires: [Requiere("device.info", ">=1.1.0 <2.0.0")]));

        Assert.That(resultado.EsCompatible, Is.False);
        Assert.That(resultado.Motivos, Has.One.Contains("device.info").And.Contains("1.0.0"));
    }

    [Test]
    [Description("P12 — requires con techo: una entrada del catálogo de un mayor nuevo no sirve")]
    public void RequiresConTecho()
    {
        _entorno.Catalogo = [new EntradaDelCatalogo("device.info", V("2.0.0"), [], [])];

        Assert.That(Evaluar(Paquetes.Manifiesto(V("1.0.0"))).EsCompatible, Is.False);
    }

    [Test]
    [Description("P12 — optional no entra en el predicado: lo resuelve el contenido con el handshake")]
    public void OptionalAusente_NoImpide()
    {
        var resultado = Evaluar(Paquetes.Manifiesto(V("1.0.0"), optional: [Requiere("camera.capture", ">=1.0.0 <2.0.0")]));

        Assert.That(resultado.EsCompatible, Is.True);
    }

    [TestCase(94, 94, true, TestName = "WebView 94, paquete pide 94: compatible")]
    [TestCase(93, 94, false, TestName = "WebView 93: incompatible")]
    [TestCase(100, 110, false, TestName = "El paquete pide más que el piso del producto")]
    [TestCase(94, 80, true, TestName = "Un paquete no puede pedir menos que 94: rige el piso")]
    [TestCase(90, 80, false, TestName = "WebView 90 con un paquete que pide 80: rige el piso 94")]
    public void Motor(int webView, int pideElPaquete, bool esperado)
    {
        _entorno.VersionMayorDelWebView = webView;

        Assert.That(Evaluar(Paquetes.Manifiesto(V("1.0.0"), webView: pideElPaquete)).EsCompatible, Is.EqualTo(esperado));
    }

    [Test]
    [Description("Si no se pudo leer la versión del WebView, no se activa: no se sabe si alcanza")]
    public void MotorDesconocido()
    {
        _entorno.VersionMayorDelWebView = null;

        Assert.That(Evaluar(Paquetes.Manifiesto(V("1.0.0"))).EsCompatible, Is.False);
    }

    [Test]
    [Description("El diagnóstico dice todo lo que falta, no sólo lo primero")]
    public void TodasLasDimensionesALaVez()
    {
        _entorno.VersionDelPuente = V("2.0.0");
        _entorno.VersionMayorDelWebView = 83;

        var resultado = Evaluar(Paquetes.Manifiesto(V("1.0.0"), requires: [Requiere("camera.capture", ">=1.0.0 <2.0.0")]));

        Assert.That(resultado.Motivos, Has.Count.EqualTo(3));
    }
}
