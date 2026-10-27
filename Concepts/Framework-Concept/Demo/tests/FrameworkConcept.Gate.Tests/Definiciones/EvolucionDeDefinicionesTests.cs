using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Gate.Tests.Infraestructura;

namespace FrameworkConcept.Gate.Tests.Definiciones;

/// <summary>Las reglas de evolución, caso por caso, sobre copias modificadas de la v1 real.</summary>
[TestFixture]
public class EvolucionDeDefinicionesTests
{
    private static DefinicionEncuesta V1 =>
        DefinicionEncuesta.Leer(File.ReadAllText(Repositorio.Ruta("definiciones/encuesta-movilidad/v1.json")));

    private static DefinicionEncuesta CambiarPregunta(DefinicionEncuesta d, string id, Func<PreguntaDeEncuesta, PreguntaDeEncuesta?> cambio) =>
        d with
        {
            Pasos = [.. d.Pasos.Select(p => p with { Preguntas = [.. p.Preguntas.Select(q => q.Id == id ? cambio(q) : q).OfType<PreguntaDeEncuesta>()] })]
        };

    [Test]
    public void CorregirUnTextoSubiendoLaVersionVale()
    {
        var v1 = V1;
        var nueva = CambiarPregunta(v1, "edad", q => q with { Etiqueta = "Edad (años cumplidos)" }) with { Version = 2 };
        Assert.That(EvolucionDeDefiniciones.Violaciones(v1, nueva), Is.Empty);
    }

    [Test]
    [Description("El rojo de P8: editar una pregunta sin subir version")]
    public void EditarSinSubirLaVersionNoVale()
    {
        var v1 = V1;
        var nueva = CambiarPregunta(v1, "edad", q => q with { Etiqueta = "Edad (años cumplidos)" });
        Assert.That(EvolucionDeDefiniciones.Violaciones(v1, nueva), Has.Some.Contains("no es estrictamente mayor"));
    }

    [Test]
    public void BorrarUnaPreguntaPublicadaNoVale()
    {
        var v1 = V1;
        var nueva = CambiarPregunta(v1, "motivo", _ => null) with { Version = 2 };
        Assert.That(EvolucionDeDefiniciones.Violaciones(v1, nueva), Has.Some.Contains("se borró la pregunta publicada «motivo»"));
    }

    [Test]
    public void CambiarElTipoDeUnaPreguntaPublicadaNoVale()
    {
        var v1 = V1;
        var nueva = CambiarPregunta(v1, "edad", q => q with { Tipo = TipoDePregunta.Texto }) with { Version = 2 };
        Assert.That(EvolucionDeDefiniciones.Violaciones(v1, nueva), Has.Some.Contains("cambió de tipo"));
    }

    [Test]
    public void CambiarElEncuestaIdNoEsUnaVersion()
    {
        var v1 = V1;
        Assert.That(EvolucionDeDefiniciones.Violaciones(v1, v1 with { EncuestaId = "otra", Version = 2 }), Has.Some.Contains("encuestaId"));
    }

    [Test]
    public void BajarLaVersionNoVale()
    {
        var v1 = V1 with { Version = 3 };
        Assert.That(EvolucionDeDefiniciones.Violaciones(v1, v1 with { Version = 2 }), Has.Some.Contains("no es estrictamente mayor"));
    }
}
