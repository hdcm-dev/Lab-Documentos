using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Core.Red;

namespace FrameworkConcept.Core.Tests.Red;

/// <summary>La tabla de la política de la demo (DISENO §4.4): bien formada y atada a la tabla de métodos del puente.</summary>
[TestFixture]
public class PoliticaDeAccionesTests
{
    [Test]
    public void LosIdsSonUnicos() =>
        Assert.That(PoliticaDeAcciones.Acciones.Select(a => a.Id), Is.Unique);

    [Test]
    [Description("Cada método que ejecuta una acción existe en el contrato del puente")]
    public void CadaMetodoExisteEnLaTablaDeMetodos()
    {
        foreach (var accion in PoliticaDeAcciones.Acciones)
            foreach (var metodo in accion.MetodosDelPuente)
                Assert.That(TablaDeMetodos.Buscar(metodo), Is.Not.Null, $"{accion.Id}: «{metodo}»");
    }

    [Test]
    [Description("Un método es de UNA acción a lo sumo: si no, el despachador no sabría cuál evaluar")]
    public void NingunMetodoEnDosAcciones() =>
        Assert.That(PoliticaDeAcciones.Acciones.SelectMany(a => a.MetodosDelPuente), Is.Unique);

    [TestCase("localidades.guardar", ClaseDeAccion.SoloLocal, "documents.put")]
    [TestCase("localidades.guardar", ClaseDeAccion.SoloLocal, "documents.delete")]
    [TestCase("encuesta.enviar", ClaseDeAccion.Encolable, "encuestas.enqueueRespuesta")]
    [TestCase("outbox.enviarAhora", ClaseDeAccion.SoloEnLinea, "outbox.sendNow")]
    [TestCase("sesion.ingresar", ClaseDeAccion.SoloEnLinea, "auth.signIn")]
    public void LasCuatroAccionesDeLaDemo(string id, ClaseDeAccion clase, string metodo)
    {
        var accion = PoliticaDeAcciones.Buscar(id);
        Assert.That(accion, Is.Not.Null);
        Assert.That(accion!.Clase, Is.EqualTo(clase));
        Assert.That(PoliticaDeAcciones.DelMetodo(metodo), Is.SameAs(accion));
    }

    [TestCase("bridge.handshake")]
    [TestCase("documents.get")]
    [TestCase("network.status")]
    [TestCase("actions.policy")]
    [Description("Leer no es una acción del negocio: nunca se bloquea")]
    public void LasLecturasNoSonAcciones(string metodo) =>
        Assert.That(PoliticaDeAcciones.DelMetodo(metodo), Is.Null);
}
