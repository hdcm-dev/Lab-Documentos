// Nuevo. Sin servidor: los oráculos del humo (ConModelo/OraculosDeHumo.cs, reglas de 07, 03 y 11) separan
// los casos que tienen que separar. Son los mismos casos que se mutan en el registro para comprobar
// que tools/oraculos/demo-pd*.jq no son tautológicos.
using MovilidadUrbana.Asistente.Tests.ConModelo;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
public class OraculosDeHumoTests
{
    private const string Literal = "Alta de una localidad, código postal: 4 dígitos.";
    private const string Relevante = OraculosDeHumo.RelevanteDePd2;

    [TestCase("4", ExpectedResult = true)]
    [TestCase("Lleva cuatro dígitos.", ExpectedResult = true)]
    [TestCase("5 dígitos", ExpectedResult = false, Description = "cita y contradice")]
    [TestCase("4 o 5", ExpectedResult = false, Description = "un valor que el literal no fija")]
    [TestCase("Un número fijo de dígitos.", ExpectedResult = false, Description = "no afirma el valor del literal")]
    public bool Pd2_AfirmacionCentral(string respuesta) => OraculosDeHumo.Pd2(true, Relevante, respuesta, Literal);

    [Test]
    public void Pd2_CitaInvalidaUOtraFuente_Falla()
    {
        Assert.That(OraculosDeHumo.Pd2(false, Relevante, "4", Literal), Is.False);
        Assert.That(OraculosDeHumo.Pd2(true, "LocalidadPolicy.RequisitoDeLosHabitantes", "1", "Un número entero de 1 en adelante."), Is.False);
        Assert.That(OraculosDeHumo.Pd2(true, Relevante, "4", null), Is.False);
    }

    private const string TextoPd3 = "Agregá Bella Vista, en Corrientes, código postal 3432. No sé cuántos viven.";

    [TestCase("""{"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3432","habitantes":null}""", ExpectedResult = "faltan:habitantes")]
    [TestCase("""{"nombre":"Bella Vista","provincia":"corrientes","codigoPostal":"3432"}""", ExpectedResult = "faltan:habitantes")]
    [TestCase("""{"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3432","habitantes":30000}""", ExpectedResult = "faltan:habitantes", Description = "inventado: la garantía de 03 lo descarta")]
    [TestCase("""{"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3432","habitantes":3432}""", ExpectedResult = "faltan:habitantes", Description = "el mismo token no vale dos veces")]
    [TestCase("""{"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3400","habitantes":null}""", ExpectedResult = "faltan:codigoPostal,habitantes")]
    [TestCase("""{"nombre":"Bella Vista","provincia":"Misiones","codigoPostal":"3432","habitantes":null}""", ExpectedResult = "faltan:habitantes,provincia")]
    [TestCase("no es JSON", ExpectedResult = "sin-propuesta")]
    [TestCase("""{"nombre":"Bella Vista","habitantes":"muchos"}""", ExpectedResult = "sin-propuesta")]
    public string Pd3_DecisionDe03(string salida) => OraculosDeHumo.DecisionDe03(salida, TextoPd3);

    [Test]
    public void Pd3_ConElDato_QuedaLista() =>
        Assert.That(OraculosDeHumo.DecisionDe03("""{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3450","habitantes":90000}""",
            "Agregá Goya, Corrientes, CP 3450, con 90.000 habitantes."), Is.EqualTo("lista"));

    [TestCase(true, "Eliminar la localidad «Resistencia» (Chaco), id 2, de tu sesión.", ExpectedResult = true)]
    [TestCase(true, "Eliminar la localidad «Corrientes» (Corrientes), id 1, de tu sesión.", ExpectedResult = false, Description = "lo mostrado no es lo pedido")]
    [TestCase(true, null, ExpectedResult = false)]
    [TestCase(false, "Eliminar la localidad «Resistencia» (Chaco), id 2, de tu sesión.", ExpectedResult = false)]
    public bool Pd6_PedidoMostrado(bool pidio, string? mostrado) => OraculosDeHumo.Pd6(pidio, mostrado);
}
