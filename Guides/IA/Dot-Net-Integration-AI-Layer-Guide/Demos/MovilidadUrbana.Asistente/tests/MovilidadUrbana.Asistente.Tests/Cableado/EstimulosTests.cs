// Nuevo. Regla de estímulo de prueba (plan §E.1, condición i): los estímulos de la demo usan sólo
// entidades y valores del caso base o derivados por una regla del caso.
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.AI.Ayuda;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
public class EstimulosTests
{
    [Test]
    [Description("Las entradas están rotuladas, y sus números y localidades vienen del caso base (LocalidadesTests.cs l. 50 y l. 102, SessionSeeder.cs)")]
    public void Entradas_UsanValoresDelCasoBase()
    {
        Assert.That(File.ReadAllText(Entradas.Ruta), Does.Contain("\"rotulo\": \"escrito para el ejemplo\""));
        var permitidos = new HashSet<double> { 3450, 90000, 3432, 34, 8, 35 };
        foreach (var e in Entradas.Todas())
        {
            Assert.That(Infrastructure.AI.Extraccion.GarantiaNumerica.NumerosDe(e.Texto), Is.SubsetOf(permitidos), e.Id);
        }
        var localidades = new[] { "Corrientes", "Resistencia", "Goya", "Bella Vista" };
        Assert.That(Entradas.Todas().Where(e => e.Paso is "PD-3" or "PD-5" or "PD-6").Select(e => e.Texto),
            Has.All.Matches<string>(t => t.StartsWith("Perdón", StringComparison.Ordinal) || localidades.Any(t.Contains)));
    }

    [Test]
    [Description("Todo valor de catálogo de los estímulos se normaliza a una clave de Catalogos")]
    public void Estimulos_NormalizanACatalogos()
    {
        var n = new NormalizadorDeCatalogo();
        Assert.That(new[] { "bondi", "bici" }.Select(n.Medio), Is.SubsetOf(Catalogos.Medios.Select(m => m.Clave)));
        Assert.That(n.Frecuencia("todos los días"), Is.AnyOf(Catalogos.Frecuencias.Select(f => (object)f.Clave).ToArray()));
        Assert.That(n.Motivo("trabajo"), Is.AnyOf(Catalogos.Motivos.Select(m => (object)m.Clave).ToArray()));
        Assert.That(n.Provincia("corrientes"), Is.EqualTo("Corrientes"));
        Assert.That(n.Medio("avion"), Is.Not.AnyOf(Catalogos.Medios.Select(m => (object)m.Clave).ToArray()), "un medio inventado no se salva");
    }

    [Test]
    [Description("El corpus de la ayuda son las 12 propiedades de LocalidadPolicy (4) y EncuestaPolicy (8), con id calificado por clase")]
    public void Corpus_DoceRequisitos()
    {
        var corpus = CorpusDeAyuda.Fragmentos();
        Assert.That(corpus, Has.Count.EqualTo(12));
        Assert.That(corpus.Count(f => f.Id.StartsWith("LocalidadPolicy.", StringComparison.Ordinal)), Is.EqualTo(4));
        Assert.That(corpus.Select(f => f.Id), Is.Unique);
        Assert.That(corpus.Single(f => f.Id == "LocalidadPolicy.RequisitoDelCodigoPostal").Texto, Is.EqualTo("Alta de una localidad, código postal: 4 dígitos."));
    }
}
