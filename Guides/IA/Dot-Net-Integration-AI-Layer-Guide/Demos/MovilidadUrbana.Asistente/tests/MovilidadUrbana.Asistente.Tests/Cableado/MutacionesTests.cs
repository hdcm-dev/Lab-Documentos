// Nuevo. Casos rojos de la quinta regla de ArquitecturaTests (REC-26; patrón de los casos rojos de la
// laboratorio hasta el 2026-10-28, adaptado a la regla de Presentation). Las mutaciones de Cableado/Mutantes/ se compilan
// en este ensamblado y se inspeccionan con el mismo predicado de la regla: si la regla dejara de verlas,
// estas pruebas fallan.
using System.Reflection;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
public class MutacionesTests
{
    private static readonly Assembly Pruebas = typeof(MutacionesTests).Assembly;

    private static IReadOnlyList<Type> Mutante(string nombre)
    {
        var tipos = Pruebas.GetTypes().Where(t => t.Namespace?.EndsWith(".Mutantes." + nombre, StringComparison.Ordinal) == true).ToList();
        Assert.That(tipos, Is.Not.Empty, $"no se compiló la mutación {nombre}");
        return tipos;
    }

    [Test]
    [Description("Mutación «Cuerpo»: un tipo de Infrastructure.AI usado sólo en el cuerpo de un método. La firma no lo ve; con el IL la quinta regla da rojo")]
    public void Cuerpo_SoloConLaFirmaNoSeVe_ConElIlDaRojo()
    {
        var tipos = Mutante("Cuerpo");
        Assert.That(ArquitecturaTests.ViolacionesDePresentacion(tipos, leerCuerpo: false), Is.Empty, "sólo con la firma, el uso en el cuerpo no se ve");

        var conIl = ArquitecturaTests.ViolacionesDePresentacion(tipos);
        foreach (var v in conIl) TestContext.Out.WriteLine(v);
        Assert.That(conIl, Has.Some.EndsWith("MovilidadUrbana.Asistente.Infrastructure.AI.ModoDelAsistente"));
    }

    [Test]
    [Description("Mutación «Atributo»: un tipo de Infrastructure.AI nombrado sólo en un atributo. La quinta regla da rojo, con y sin leer el cuerpo")]
    public void Atributo_DaRojo()
    {
        var tipos = Mutante("Atributo");
        foreach (var leerCuerpo in new[] { false, true })
        {
            var v = ArquitecturaTests.ViolacionesDePresentacion(tipos, leerCuerpo);
            foreach (var x in v) TestContext.Out.WriteLine(x);
            Assert.That(v, Has.Some.EndsWith("MovilidadUrbana.Asistente.Infrastructure.AI.Chat.EstadoDelPedido"), $"leerCuerpo={leerCuerpo}");
        }
    }

    [Test]
    [Description("Mutación «Comun»: un tipo fuera de Infrastructure.AI y Program que usa Comun.AI en el cuerpo de un método. La sexta regla da rojo")]
    public void Comun_FueraDeInfrastructureAI_DaRojo()
    {
        var v = ArquitecturaTests.ViolacionesDeComun(Mutante("Comun"));
        foreach (var x in v) TestContext.Out.WriteLine(x);
        Assert.That(v, Has.Some.EndsWith("MovilidadUrbana.Asistente.Comun.AI.OpcionesDelProveedor"));
    }
}
