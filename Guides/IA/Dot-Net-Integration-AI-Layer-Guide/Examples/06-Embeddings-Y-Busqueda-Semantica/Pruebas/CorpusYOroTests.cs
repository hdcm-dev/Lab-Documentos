// Nuevo: pruebas de cableado del corpus y del oro (sin red).
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Domain.Rules;

namespace MovilidadUrbana.Asistente.Pruebas;

public class CorpusYOroTests
{
    private static readonly IReadOnlyList<Fragmento> Corpus = CorpusDePolicies.Fragmentos();
    private static readonly ConjuntoDeOro Oro = ConjuntoDeOro.Leer();

    [Test]
    public void Corpus_SonLos12LiteralesConIdCalificado()
    {
        Assert.That(Validaciones.DelCorpus(Corpus), Is.Empty);
        Assert.That(Corpus.Select(f => f.Id), Does.Contain("LocalidadPolicy.RequisitoDelNombre"));
        Assert.That(Corpus.Select(f => f.Id), Does.Contain("EncuestaPolicy.RequisitoDelNombre"));
    }

    [Test]
    public void Plantilla_EntidadPropiedadEnPalabrasYLiteral()
    {
        var cp = Corpus.Single(f => f.Id == "LocalidadPolicy.RequisitoDelCodigoPostal");
        Assert.That(cp.Texto, Is.EqualTo($"Localidad · requisito del código postal: {LocalidadRules.DigitosDelCodigoPostal} dígitos."));
        Assert.That(Corpus.Single(f => f.Id == "EncuestaPolicy.RequisitoDeLosMedios").Texto,
            Is.EqualTo("Encuesta · requisito de los medios: Al menos uno."));
    }

    [Test]
    public void Oro_CumpleLaFormaDelPlan()
    {
        Assert.That(Validaciones.DelOro(Oro, Corpus), Is.Empty);
        var nombre = Oro.Preguntas.Single(p => p.Id == Informe.PreguntaDelNombre);
        Assert.That(nombre.Relevantes, Is.EquivalentTo(new[] { "LocalidadPolicy.RequisitoDelNombre", "EncuestaPolicy.RequisitoDelNombre" }));
    }

    [Test]
    public void Oro_ValoresEsperadosSalenDeLasReglas()
    {
        string Valor(string id) => Oro.Preguntas.Single(p => p.Id == id).ValorEsperado!;
        Assert.That(Valor("r01"), Is.EqualTo($"{LocalidadRules.LargoMinimoDelNombre} y {LocalidadRules.LargoMaximoDelNombre}"));
        Assert.That(Valor("r03"), Is.EqualTo($"{LocalidadRules.DigitosDelCodigoPostal}"));
        Assert.That(Valor("r04"), Is.EqualTo($"{LocalidadRules.HabitantesMinimos}"));
        Assert.That(Valor("r05"), Is.EqualTo($"{EncuestaRules.LargoMinimoDelNombre}"));
        Assert.That(Valor("r06"), Is.EqualTo($"{EncuestaRules.EdadMinima} y {EncuestaRules.EdadMaxima}"));
        Assert.That(Valor("r10"), Is.EqualTo($"{EncuestaRules.DistanciaMinima:0} y {EncuestaRules.DistanciaMaxima:0}"));
        Assert.That(Valor("r11"), Is.EqualTo($"{EncuestaRules.MinutosMinimos} y {EncuestaRules.MinutosMaximos}"));
        // Los 5 sin valor verificable que nombra el plan (§E.2, fila de 07).
        Assert.That(Oro.Preguntas.Where(p => p.SinValorVerificable).Select(p => p.Objetivo), Is.EquivalentTo(new[]
        {
            "LocalidadPolicy.RequisitoDeLaProvincia", "EncuestaPolicy.RequisitoDeLaLocalidad", "EncuestaPolicy.RequisitoDeLosMedios",
            "EncuestaPolicy.RequisitoDeLaFrecuencia", "EncuestaPolicy.RequisitoDelMotivo"
        }));
    }

    [Test]
    public void Oro_ValoresDeCatalogoPertenecenACatalogos() =>
        Assert.That(Validaciones.DelCatalogo(Oro), Is.Empty);

    [Test]
    public void Oro_ValorFueraDeCatalogo_SeDetecta()
    {
        var malo = Oro with { Preguntas = [Oro.Preguntas[0] with { Texto = "¿Y en Neuquén?", ValoresDeCatalogo = ["Neuquén"] }] };
        Assert.That(Validaciones.DelCatalogo(malo), Is.Not.Empty);
    }
}
