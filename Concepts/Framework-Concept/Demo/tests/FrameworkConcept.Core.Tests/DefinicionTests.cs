using FrameworkConcept.Core.Application.Definiciones;
using FrameworkConcept.Core.Application.Encuestas;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Core.Domain;
using FrameworkConcept.Core.Domain.Rules;

namespace FrameworkConcept.Core.Tests;

/// <summary>
/// La encuesta de tres pasos del caso base, escrita como datos (P1, P11): definiciones/encuesta-movilidad.
/// Estas pruebas atan la definición v1 al dominio portado, para que «encuesta como datos» no cambie en
/// silencio lo que la encuesta pedía.
/// </summary>
[TestFixture]
public class DefinicionTests
{
    private static DefinicionEncuesta Leer(string archivo) =>
        DefinicionEncuesta.Leer(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "definiciones", "encuesta-movilidad", archivo)));

    private static DefinicionEncuesta V1 => Leer("v1.json");
    private static DefinicionEncuesta V2 => Leer("v2.json");

    [Test]
    public void LasDosVersionesSeLeenConElContrato()
    {
        Assert.That(V1.Referencia, Is.EqualTo(new ReferenciaDeDefinicion("encuesta-movilidad", 1)));
        Assert.That(V2.Referencia, Is.EqualTo(new ReferenciaDeDefinicion("encuesta-movilidad", 2)));
        Assert.That(V1.CompatibleConContenido.Contiene(VersionSemantica.Parse("1.0.0+abc1234")), Is.True);
        Assert.That(V1.CompatibleConContenido.Contiene(VersionSemantica.Parse("2.0.0")), Is.False);
    }

    [Test]
    [Description("La v1 dice lo mismo que EncuestaRules y los catálogos del caso base")]
    public void V1CoincideConElDominio()
    {
        var v1 = V1;
        var pregunta = v1.Preguntas.ToDictionary(p => p.Id);

        Assert.That(v1.Pasos, Has.Count.EqualTo(EncuestaRules.TotalDePasos));
        Assert.That(pregunta["nombre"].Reglas!.LargoMinimo, Is.EqualTo(EncuestaRules.LargoMinimoDelNombre));
        Assert.That(pregunta["edad"].Reglas!.Minimo, Is.EqualTo(EncuestaRules.EdadMinima));
        Assert.That(pregunta["edad"].Reglas!.Maximo, Is.EqualTo(EncuestaRules.EdadMaxima));
        Assert.That(pregunta["distancia"].Reglas!.Minimo, Is.EqualTo(EncuestaRules.DistanciaMinima));
        Assert.That(pregunta["distancia"].Reglas!.Maximo, Is.EqualTo(EncuestaRules.DistanciaMaxima));
        Assert.That(pregunta["minutos"].Reglas!.Minimo, Is.EqualTo(EncuestaRules.MinutosMinimos));
        Assert.That(pregunta["minutos"].Reglas!.Maximo, Is.EqualTo(EncuestaRules.MinutosMaximos));

        Assert.That(pregunta["medios"].Opciones!.Select(o => (o.Clave, o.Etiqueta)), Is.EqualTo(Catalogos.Medios));
        Assert.That(pregunta["frecuencia"].Opciones!.Select(o => (o.Clave, o.Etiqueta)), Is.EqualTo(Catalogos.Frecuencias));
        Assert.That(pregunta["motivo"].Opciones!.Select(o => (o.Clave, o.Etiqueta)), Is.EqualTo(Catalogos.Motivos));

        Assert.That(pregunta["edad"].Requisito, Is.EqualTo(EncuestaPolicy.RequisitoDeLaEdad));
        Assert.That(pregunta["distancia"].Requisito, Is.EqualTo(EncuestaPolicy.RequisitoDeLaDistancia));
        Assert.That(pregunta["minutos"].Requisito, Is.EqualTo(EncuestaPolicy.RequisitoDeLosMinutos));
    }

    private static IEnumerable<TestCaseData> Modelos()
    {
        yield return new TestCaseData(new EncuestaModel()).SetName("vacía");
        yield return new TestCaseData(EncuestasDePrueba.Completa()).SetName("completa");
        var invalida = EncuestasDePrueba.Completa();
        invalida.Nombre = "Al"; invalida.Edad = 111; invalida.Distancia = -1; invalida.Minutos = 0;
        yield return new TestCaseData(invalida).SetName("fuera de rango");
        var bordes = EncuestasDePrueba.Completa();
        bordes.Edad = 16; bordes.Distancia = 0; bordes.Minutos = 600; bordes.Nombre = " Ana ";
        yield return new TestCaseData(bordes).SetName("en los bordes");
    }

    [TestCaseSource(nameof(Modelos))]
    [Description("Con la v1, el validador de definiciones da los mismos errores que EncuestaService.ValidarPaso")]
    public void ElValidadorEquivaleAlCasoBase(EncuestaModel model)
    {
        var servicio = new TestEnvironment().Encuestas;
        var valores = new Dictionary<string, object?>
        {
            ["nombre"] = model.Nombre, ["edad"] = model.Edad, ["localidad"] = model.Localidad,
            ["medios"] = model.Medios, ["frecuencia"] = model.Frecuencia,
            ["distancia"] = model.Distancia, ["minutos"] = model.Minutos, ["motivo"] = model.Motivo
        };

        for (var paso = 1; paso <= EncuestaRules.TotalDePasos; paso++)
        {
            Assert.That(DefinicionValidator.ValidarPaso(V1, paso, valores),
                Is.EquivalentTo(servicio.ValidarPaso(paso, model)), $"paso {paso}");
        }
    }

    [Test]
    [Description("P13.8 — La v2 es una evolución compatible: no cambia ni el id ni el tipo de lo publicado")]
    public void V2EsEvolucionCompatibleDeV1()
    {
        var v1 = V1;
        var v2 = V2;
        var enV2 = v2.Preguntas.ToDictionary(p => p.Id);

        Assert.That(v2.EncuestaId, Is.EqualTo(v1.EncuestaId));
        Assert.That(v2.Version, Is.GreaterThan(v1.Version));
        foreach (var publicada in v1.Preguntas)
        {
            Assert.That(enV2.ContainsKey(publicada.Id), Is.True, publicada.Id);
            Assert.That(enV2[publicada.Id].Tipo, Is.EqualTo(publicada.Tipo), publicada.Id);
        }
    }

    [Test]
    [Description("La pregunta nueva de la v2 es opcional: vacía vale; con texto, pide 3 caracteres")]
    public void ComentarioOpcionalDeV2()
    {
        var comentario = V2.Preguntas.Single(p => p.Id == "comentario");

        Assert.That(DefinicionValidator.EsValida(comentario, null), Is.True);
        Assert.That(DefinicionValidator.EsValida(comentario, "  "), Is.True);
        Assert.That(DefinicionValidator.EsValida(comentario, "ok"), Is.False);
        Assert.That(DefinicionValidator.EsValida(comentario, "bien"), Is.True);
    }
}
