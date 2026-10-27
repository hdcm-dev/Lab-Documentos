using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Puente;
using static FrameworkConcept.Core.Contracts.Puente.ContratoDelPuente;

namespace FrameworkConcept.Gate.Tests.Contrato;

/// <summary>
/// La clasificación de P3, caso por caso. Si el clasificador se equivoca, la instantánea dejaría pasar un
/// cambio mayor con una subida menor: por eso se prueba aparte, sobre una instantánea de ejemplo FIJA (no la
/// tabla real), para que estas pruebas no cambien cuando el contrato real evoluciona.
/// </summary>
[TestFixture]
public class ClasificadorDeCambiosTests
{
    private static Instantanea Base => new(
        VersionSemantica.Parse("1.0.0"),
        new TransporteDelPuente("_framework/hybridwebview.js", "POST /__hwvInvokeDotNet", "Call(sobre)", "HybridWebView.InvokeJavaScriptAsync", 10),
        [
            new FilaDeMetodo("device.info", [], [new("modelo", "string")], "device.info"),
            new FilaDeMetodo("documents.get", [new("coleccion", "string"), new("id", "string")], [new("documento", "Documento", Opcional: true)], null),
            new FilaDeMetodo("documents.list", [new("coleccion", "string")], [new("documentos", "Documento[]")], null),
            new FilaDeMetodo("outbox.status", [], [new("pendientes", "int"), new("rechazadas", "int")], null)
        ],
        [
            new FilaDeEvento("network.changed", [new("conectado", "bool")]),
            new FilaDeEvento("outbox.changed", [new("pendientes", "int")])
        ],
        [new EntradaVersionada("device.info", VersionSemantica.Parse("1.0.0"))]);

    private static Instantanea CambiarMetodo(string nombre, Func<FilaDeMetodo, FilaDeMetodo?> cambio)
    {
        var b = Base;
        var metodos = b.Metodos.Select(m => m.Nombre == nombre ? cambio(m) : m).OfType<FilaDeMetodo>().ToList();
        return b with { Metodos = metodos };
    }

    private static NivelDeCambio Nivel(Instantanea despues) => ClasificadorDeCambios.Clasificar(Base, despues).Nivel;

    [Test]
    public void SinCambiosEsNinguno() => Assert.That(Nivel(Base), Is.EqualTo(NivelDeCambio.Ninguno));

    [Test]
    public void MetodoNuevoEsMenor()
    {
        var b = Base;
        var despues = b with { Metodos = [.. b.Metodos, new FilaDeMetodo("encuestas.list", [], [new("ids", "string[]")], null)] };
        Assert.That(Nivel(despues), Is.EqualTo(NivelDeCambio.Menor));
    }

    [Test]
    public void MetodoQuitadoEsMayor() =>
        Assert.That(Nivel(CambiarMetodo("outbox.status", _ => null)), Is.EqualTo(NivelDeCambio.Mayor));

    [Test]
    public void CampoOpcionalNuevoEnLaRespuestaEsMenor() =>
        Assert.That(Nivel(CambiarMetodo("outbox.status", m => m with { Respuesta = [.. m.Respuesta, new("enviando", "int", Opcional: true)] })),
            Is.EqualTo(NivelDeCambio.Menor));

    [Test]
    public void CampoObligatorioNuevoEnLaRespuestaEsMayor() =>
        Assert.That(Nivel(CambiarMetodo("outbox.status", m => m with { Respuesta = [.. m.Respuesta, new("enviando", "int")] })),
            Is.EqualTo(NivelDeCambio.Mayor));

    [Test]
    public void ParametroObligatorioNuevoEsMayor() =>
        Assert.That(Nivel(CambiarMetodo("documents.list", m => m with { Parametros = [.. m.Parametros, new("desde", "int")] })),
            Is.EqualTo(NivelDeCambio.Mayor));

    [Test]
    public void CambioDeTipoEsMayor() =>
        Assert.That(Nivel(CambiarMetodo("documents.get", m => m with { Parametros = [m.Parametros[0], new("id", "int")] })),
            Is.EqualTo(NivelDeCambio.Mayor));

    [Test]
    public void CampoQuePasaAOpcionalEsMayor() =>
        Assert.That(Nivel(CambiarMetodo("outbox.status", m => m with { Respuesta = [m.Respuesta[0], m.Respuesta[1] with { Opcional = true }] })),
            Is.EqualTo(NivelDeCambio.Mayor));

    [Test]
    public void CambioDelTransporteEsMayor() =>
        Assert.That(Nivel(Base with { Transporte = Base.Transporte with { MauiMayor = Base.Transporte.MauiMayor + 1 } }),
            Is.EqualTo(NivelDeCambio.Mayor));

    [Test]
    public void EntradaNuevaDelCatalogoEsMenor()
    {
        var b = Base;
        var despues = b with { Catalogo = [.. b.Catalogo, new EntradaVersionada("camera.capture", VersionSemantica.Parse("1.0.0"))] };
        Assert.That(Nivel(despues), Is.EqualTo(NivelDeCambio.Menor));
    }

    [TestCase("1.1.0", NivelDeCambio.Menor)]
    [TestCase("2.0.0", NivelDeCambio.Mayor)]
    [TestCase("0.9.0", NivelDeCambio.Mayor)]
    public void VersionDeUnaEntradaDelCatalogo(string version, NivelDeCambio esperado)
    {
        var b = Base;
        var despues = b with { Catalogo = [.. b.Catalogo.Select(e => e.Id == "device.info" ? e with { Version = VersionSemantica.Parse(version) } : e)] };
        Assert.That(Nivel(despues), Is.EqualTo(esperado));
    }

    [Test]
    public void EventoQuitadoEsMayorYEventoNuevoEsMenor()
    {
        var b = Base;
        Assert.That(Nivel(b with { Eventos = [.. b.Eventos.Skip(1)] }), Is.EqualTo(NivelDeCambio.Mayor));
        Assert.That(Nivel(b with { Eventos = [.. b.Eventos, new FilaDeEvento("definicion.changed", [])] }), Is.EqualTo(NivelDeCambio.Menor));
    }

    [TestCase(NivelDeCambio.Ninguno, "1.0.0", "1.0.0", true)]
    [TestCase(NivelDeCambio.Ninguno, "1.0.0", "1.0.1", false)]   // subió sin cambio
    [TestCase(NivelDeCambio.Ninguno, "1.0.0", "1.1.0", false)]
    [TestCase(NivelDeCambio.Menor, "1.0.0", "1.1.0", true)]
    [TestCase(NivelDeCambio.Menor, "1.0.0", "1.0.0", false)]      // cambió sin subir
    [TestCase(NivelDeCambio.Menor, "1.0.0", "2.0.0", false)]      // subió de más: parte el canal sin necesidad
    [TestCase(NivelDeCambio.Mayor, "1.0.0", "2.0.0", true)]
    [TestCase(NivelDeCambio.Mayor, "1.0.0", "1.1.0", false)]      // subió de menos
    [TestCase(NivelDeCambio.Mayor, "2.0.0", "1.0.0", false)]      // bajó
    public void ReglaDeLaVersion(NivelDeCambio requerido, string antes, string despues, bool valido) =>
        Assert.That(ClasificadorDeCambios.ProblemaDeVersion(requerido, VersionSemantica.Parse(antes), VersionSemantica.Parse(despues)) is null,
            Is.EqualTo(valido));
}
