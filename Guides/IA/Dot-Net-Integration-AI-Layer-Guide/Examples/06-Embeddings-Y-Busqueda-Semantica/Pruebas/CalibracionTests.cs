// Nuevo (REC-28, 2026-10-28): cableado del conjunto de calibración del umbral (sin red). La calibración
// va aparte del oro de evaluación: ninguna pregunta del oro se repite acá.
using MovilidadUrbana.Asistente.Application;

namespace MovilidadUrbana.Asistente.Pruebas;

public class CalibracionTests
{
    private const string Archivo = "oro/calibracion.v1.json";
    private static readonly IReadOnlyList<Fragmento> Corpus = CorpusDePolicies.Fragmentos();
    private static readonly ConjuntoDeOro Calibracion = ConjuntoDeOro.Leer(Path.Combine(AppContext.BaseDirectory, Archivo));

    private static string Normalizar(string texto) => string.Join(' ', texto.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    [Test]
    public void Calibracion_DiezCasosConLaFormaDelOro()
    {
        Assert.That(Calibracion.Rotulo, Is.EqualTo("escrito para el ejemplo"));
        Assert.That(Calibracion.Plantilla, Is.EqualTo(CorpusDePolicies.Plantilla));
        Assert.That(Calibracion.Preguntas, Has.Count.EqualTo(10));
        Assert.That(Calibracion.Preguntas.Select(p => p.Id).Distinct().Count(), Is.EqualTo(10));
        var respondibles = Calibracion.Preguntas.Where(p => p.Respondible).ToList();
        var no = Calibracion.Preguntas.Where(p => !p.Respondible).ToList();
        Assert.That(respondibles, Has.Count.EqualTo(5));
        Assert.That(no.Count(p => p.Cercania == "cercana"), Is.EqualTo(3));
        Assert.That(no.Count(p => p.Cercania == "lejana"), Is.EqualTo(2));
        var ids = Corpus.Select(f => f.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var p in respondibles)
        {
            Assert.That(p.Relevantes, Is.Not.Empty.And.All.Matches<string>(ids.Contains), p.Id);
            Assert.That(p.Relevantes, Does.Contain(p.Objetivo), p.Id);
        }
        Assert.That(no.SelectMany(p => p.Relevantes), Is.Empty);
        Assert.That(Validaciones.DelCatalogo(Calibracion), Is.Empty);
    }

    [Test]
    public void Calibracion_DisjuntaDeTodoOroDeEvaluacion()
    {
        var oros = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "oro"), "conjunto-de-oro.v*.json");
        Assert.That(oros, Is.Not.Empty);
        foreach (var ruta in oros)
        {
            var oro = ConjuntoDeOro.Leer(ruta);
            var textos = oro.Preguntas.Select(p => Normalizar(p.Texto)).ToHashSet(StringComparer.Ordinal);
            var idsDelOro = oro.Preguntas.Select(p => p.Id).ToHashSet(StringComparer.Ordinal);
            foreach (var p in Calibracion.Preguntas)
            {
                Assert.That(textos, Does.Not.Contain(Normalizar(p.Texto)), $"{Path.GetFileName(ruta)}: {p.Id}");
                Assert.That(idsDelOro, Does.Not.Contain(p.Id), $"{Path.GetFileName(ruta)}: {p.Id}");
            }
        }
    }
}
