using FrameworkConcept.Core.Domain.Rules;

namespace FrameworkConcept.Core.Tests;

/// <summary>
/// Los bordes de las reglas portadas (relevamiento R1 §3.3). El caso base las probaba sólo a través de los
/// ViewModels; acá se fijan una por una porque la definición de encuesta v1 tiene que decir lo mismo.
/// </summary>
[TestFixture]
public class ReglasTests
{
    [TestCase("Go", false)]
    [TestCase("  Go  ", false)]
    [TestCase("Goy", true)]
    public void NombreDeLocalidad(string nombre, bool valido) =>
        Assert.That(LocalidadRules.NombreValido(nombre), Is.EqualTo(valido));

    [TestCase("3400", true)]
    [TestCase(" 3400 ", true)]
    [TestCase("340", false)]
    [TestCase("34000", false)]
    [TestCase("34a0", false)]
    public void CodigoPostal(string codigo, bool valido) =>
        Assert.That(LocalidadRules.CodigoPostalValido(codigo), Is.EqualTo(valido));

    [TestCase(null, false)]
    [TestCase(0, false)]
    [TestCase(1, true)]
    public void Habitantes(int? habitantes, bool valido) =>
        Assert.That(LocalidadRules.HabitantesValidos(habitantes), Is.EqualTo(valido));

    [Test]
    public void MismaLocalidadIgnoraMayusculasPeroNoLaProvincia()
    {
        Assert.That(LocalidadRules.MismaLocalidad("Goya", "Corrientes", " goya ", "Corrientes"), Is.True);
        Assert.That(LocalidadRules.MismaLocalidad("Goya", "Corrientes", "Goya", "Chaco"), Is.False);
    }

    [TestCase(15, false)]
    [TestCase(16, true)]
    [TestCase(110, true)]
    [TestCase(111, false)]
    [TestCase(null, false)]
    public void Edad(int? edad, bool valida) => Assert.That(EncuestaRules.EdadValida(edad), Is.EqualTo(valida));

    [TestCase(0d, true)]
    [TestCase(500d, true)]
    [TestCase(500.01, false)]
    [TestCase(-0.1, false)]
    public void Distancia(double distancia, bool valida) =>
        Assert.That(EncuestaRules.DistanciaValida(distancia), Is.EqualTo(valida));

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(600, true)]
    [TestCase(601, false)]
    public void Minutos(int minutos, bool validos) => Assert.That(EncuestaRules.MinutosValidos(minutos), Is.EqualTo(validos));
}
