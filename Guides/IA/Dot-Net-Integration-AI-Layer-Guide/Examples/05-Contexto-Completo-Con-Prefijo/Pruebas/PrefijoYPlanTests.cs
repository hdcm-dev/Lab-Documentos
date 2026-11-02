// Nuevo: pruebas sin red de lo que la aplicación controla de la caché del prefijo (H-024).
using MovilidadUrbana.Asistente.Application;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

public class PrefijoYPlanTests
{
    private static readonly List<Pregunta> Preguntas = [.. Enumerable.Range(1, 6).Select(i => new Pregunta($"p0{i}", $"pregunta {i}"))];
    private static readonly string Prefijo = PrefijoEstable.Construir(PadronDeLocalidades.ATexto(PadronDeLocalidades.Generar()));

    private static IReadOnlyList<ConsultaPlanificada> Plan(OrdenDeConsultas orden)
    {
        var n = 0;
        return PlanDeConsultas.Armar(Preguntas, Prefijo, orden, () => $"{++n:x16}");
    }

    [Test]
    public void Alternado_TreceLlamadas_EnOrdenFijo()
    {
        var plan = Plan(OrdenDeConsultas.Alternado);

        Assert.That(plan.Select(c => c.Paso), Is.EqualTo(
            Enumerable.Range(0, 6).SelectMany(_ => new[] { Pasos.SinPrefijo, Pasos.ConPrefijo }).Append(Pasos.VueltaAFrio)));
        Assert.That(plan.Take(12).Select(c => c.EntradaId), Is.EqualTo(Preguntas.SelectMany(p => new[] { p.Id, p.Id })));
        Assert.That(plan[^1].EntradaId, Is.EqualTo("p06"));
    }

    [Test]
    public void ConCalentamiento_CatorceLlamadas_ElCalentamientoPrimeroYDespuesElPlan()
    {
        var plan = Plan(OrdenDeConsultas.AlternadoConCalentamiento);

        Assert.That(plan, Has.Count.EqualTo(14));
        Assert.That(plan[0].Paso, Is.EqualTo(Pasos.Calentamiento));
        Assert.That(plan[0].Sistema, Is.EqualTo(Prefijo));
        Assert.That(plan.Skip(1).Select(c => c.Paso), Is.EqualTo(Plan(OrdenDeConsultas.Alternado).Select(c => c.Paso)));
    }

    [Test]
    public void ConPrefijo_IdenticoByteAByte()
    {
        var textos = Plan(OrdenDeConsultas.Alternado).Where(c => c.Paso == Pasos.ConPrefijo).Select(c => c.Sistema).Distinct(StringComparer.Ordinal);
        Assert.That(textos, Is.EqualTo(new[] { Prefijo }));
    }

    [Test]
    public void SinPrefijo_NonceDistintoAlPrincipio()
    {
        var textos = Plan(OrdenDeConsultas.Alternado).Where(c => c.Paso == Pasos.SinPrefijo).Select(c => c.Sistema).ToList();

        Assert.That(textos.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(6));
        Assert.That(textos, Has.All.Matches<string>(t => t.EndsWith(Prefijo, StringComparison.Ordinal) && t[0] != Prefijo[0]));
    }

    [Test]
    public void VueltaAFrio_SoloElPrimerCaracterCambia()
    {
        var vuelta = Plan(OrdenDeConsultas.Alternado)[^1].Sistema;

        Assert.That(vuelta, Has.Length.EqualTo(Prefijo.Length));
        Assert.That(vuelta[0], Is.Not.EqualTo(Prefijo[0]));
        Assert.That(vuelta[1..], Is.EqualTo(Prefijo[1..]));
    }
}
