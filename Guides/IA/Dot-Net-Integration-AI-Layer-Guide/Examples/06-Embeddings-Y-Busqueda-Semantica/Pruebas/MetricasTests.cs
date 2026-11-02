// Nuevo: métricas, líneas base y regla pareada, con ordenamientos armados a mano (sin red).
using MovilidadUrbana.Asistente.Application;

namespace MovilidadUrbana.Asistente.Pruebas;

public class MetricasTests
{
    private static IReadOnlyList<Posicion> Orden(params string[] ids) => ids.Select((id, i) => new Posicion(id, ids.Length - i)).ToList();

    [Test]
    public void Medir_RangoHitYRecall()
    {
        var m = Metricas.Medir("q", Orden("a", "b", "c", "d"), ["c", "d"]);
        Assert.That(m.Rango, Is.EqualTo(3));
        Assert.That(m.Hit1, Is.False);
        Assert.That(m.Hit3, Is.True);
        Assert.That(m.Rr, Is.EqualTo(1.0 / 3).Within(1e-12));
        Assert.That(m.Recall3, Is.EqualTo(0.5));
    }

    [Test]
    public void Azar_EsperanzaAnaliticaDelPlan()
    {
        // Plan §E.2: con 12 fragmentos y un relevante, hit@1 = 1/12, hit@3 = 25 %, MRR ≈ 0,259.
        Assert.That(ReferenciaAlAzar.HitEsperado(12, 1, 1), Is.EqualTo(1.0 / 12).Within(1e-12));
        Assert.That(ReferenciaAlAzar.HitEsperado(12, 1, 3), Is.EqualTo(0.25).Within(1e-12));
        Assert.That(ReferenciaAlAzar.RrEsperado(12, 1), Is.EqualTo(0.2586).Within(1e-4));
        // Con dos relevantes, la probabilidad de la primera posición se duplica.
        Assert.That(ReferenciaAlAzar.HitEsperado(12, 2, 1), Is.EqualTo(2.0 / 12).Within(1e-12));
    }

    [Test]
    public void Azar_PermutacionEsEstable()
    {
        var corpus = CorpusDePolicies.Fragmentos();
        Assert.That(ReferenciaAlAzar.Ordenar(corpus, 3).Select(p => p.Id), Is.EqualTo(ReferenciaAlAzar.Ordenar(corpus, 3).Select(p => p.Id)));
        Assert.That(ReferenciaAlAzar.Ordenar(corpus, 3).Count, Is.EqualTo(12));
    }

    [Test]
    public void Lexica_GanaElFragmentoQueComparteMasPalabras()
    {
        var orden = ReferenciaLexica.Ordenar(CorpusDePolicies.Fragmentos(), "¿Cuántos dígitos tiene el código postal?");
        Assert.That(orden[0].Id, Is.EqualTo("LocalidadPolicy.RequisitoDelCodigoPostal"));
        Assert.That(ReferenciaLexica.Palabras("Código POSTAL"), Is.EquivalentTo(new[] { "codigo", "postal" }));
    }

    [TestCase(6, 0, 0.03125)]
    [TestCase(5, 0, 0.0625)]
    [TestCase(3, 3, 1.0)]
    [TestCase(0, 0, 1.0)]
    public void Pareada_BinomialExactaBilateral(int b, int c, double p) =>
        Assert.That(ComparacionPareada.BinomialBilateral(b, c), Is.EqualTo(p).Within(1e-12));

    [Test]
    public void Pareada_SeisDiscordantesEnElMismoSentido_SeAfirma()
    {
        var a = Enumerable.Repeat(true, 12).ToList();
        var b = Enumerable.Range(0, 12).Select(i => i >= 6).ToList();
        Assert.That(ComparacionPareada.Comparar("A", "B", "hit@1", a, b).SeAfirmaDiferencia, Is.True);
        var b5 = Enumerable.Range(0, 12).Select(i => i >= 5).ToList();
        Assert.That(ComparacionPareada.Comparar("A", "B", "hit@1", a, b5).SeAfirmaDiferencia, Is.False);
    }

    [Test]
    public void Coseno_Basico()
    {
        Assert.That(Busqueda.Coseno([1, 0], [1, 0]), Is.EqualTo(1));
        Assert.That(Busqueda.Coseno([1, 0], [0, 1]), Is.EqualTo(0));
        Assert.Throws<ArgumentException>(() => Busqueda.Coseno([1, 0], [1, 0, 0]));
    }
}
