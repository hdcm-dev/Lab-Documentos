using FrameworkConcept.Core.Red;
using static FrameworkConcept.Core.Red.EstadoDeRed;
using static FrameworkConcept.Core.Red.SenalDeRed;

namespace FrameworkConcept.Core.Tests.Red;

/// <summary>
/// Borrador §6 — La máquina de estados de la red. Una fila por renglón de la tabla de DISENO-RED-Y-AUTENTICACION.md
/// §2.2 (el último argumento es «perfilMedido»: celular sin WiFi ni Ethernet).
/// </summary>
[TestFixture]
public class TransicionesDeRedTests
{
    // --- «cualquiera» + SistemaSinRed → SinRed ---------------------------------------------------------------
    [TestCase(SinRed, false)]
    [TestCase(RedSinServidor, false)]
    [TestCase(EnLinea, false)]
    [TestCase(EnLineaMedida, true)]
    public void SinRedDelSistemaGanaSiempre(EstadoDeRed actual, bool medido) =>
        Assert.That(TransicionesDeRed.Siguiente(actual, SistemaSinRed, medido), Is.EqualTo(SinRed));

    // --- SinRed --------------------------------------------------------------------------------------------
    [TestCase(false)]
    [TestCase(true)]
    [Description("Hay red: todavía no se sabe si llega al servidor; el monitor hace el chequeo de salud enseguida")]
    public void SinRedConRedPasaARedSinServidor(bool medido) =>
        Assert.That(TransicionesDeRed.Siguiente(SinRed, SistemaConRed, medido), Is.EqualTo(RedSinServidor));

    [TestCase(SaludOk)]
    [TestCase(SaludFalla)]
    [TestCase(LlamadaFallida)]
    [TestCase(PerfilMedido)]
    [TestCase(PerfilNoMedido)]
    [Description("arq — Una respuesta que llega con el sistema sin red es vieja: se ignora")]
    public void SinRedIgnoraRespuestasViejas(SenalDeRed senal) =>
        Assert.That(TransicionesDeRed.Siguiente(SinRed, senal, perfilMedido: false), Is.EqualTo(SinRed));

    // --- RedSinServidor ------------------------------------------------------------------------------------
    [TestCase(false, EnLinea)]
    [TestCase(true, EnLineaMedida)]
    public void RedSinServidorConSaludOkQuedaEnLinea(bool medido, EstadoDeRed esperado) =>
        Assert.That(TransicionesDeRed.Siguiente(RedSinServidor, SaludOk, medido), Is.EqualTo(esperado));

    [TestCase(SaludFalla)]
    [TestCase(LlamadaFallida)]
    [TestCase(SistemaConRed)]
    public void RedSinServidorSigueSinServidor(SenalDeRed senal) =>
        Assert.That(TransicionesDeRed.Siguiente(RedSinServidor, senal, perfilMedido: false), Is.EqualTo(RedSinServidor));

    // --- EnLinea / EnLineaMedida ---------------------------------------------------------------------------
    [TestCase(EnLinea, SaludFalla)]
    [TestCase(EnLinea, LlamadaFallida)]
    [TestCase(EnLineaMedida, SaludFalla)]      // arq: el diagrama de §6 no la dibuja
    [TestCase(EnLineaMedida, LlamadaFallida)]
    public void EnLineaConFallaPasaARedSinServidor(EstadoDeRed actual, SenalDeRed senal) =>
        Assert.That(TransicionesDeRed.Siguiente(actual, senal, perfilMedido: actual == EnLineaMedida), Is.EqualTo(RedSinServidor));

    [Test]
    public void EnLineaPorDatosMovilesEsMedida() =>
        Assert.That(TransicionesDeRed.Siguiente(EnLinea, PerfilMedido, perfilMedido: true), Is.EqualTo(EnLineaMedida));

    [Test]
    public void EnLineaMedidaConWiFiVuelveAEnLinea() =>
        Assert.That(TransicionesDeRed.Siguiente(EnLineaMedida, PerfilNoMedido, perfilMedido: false), Is.EqualTo(EnLinea));

    [TestCase(EnLinea, SaludOk, true, EnLineaMedida)]
    [TestCase(EnLinea, SistemaConRed, false, EnLinea)]
    [TestCase(EnLineaMedida, SaludOk, false, EnLinea)]
    [TestCase(EnLineaMedida, SistemaConRed, true, EnLineaMedida)]
    [Description("arq — En línea, una confirmación recalcula con el perfil vigente")]
    public void EnLineaConConfirmacionRecalculaElPerfil(EstadoDeRed actual, SenalDeRed senal, bool medido, EstadoDeRed esperado) =>
        Assert.That(TransicionesDeRed.Siguiente(actual, senal, medido), Is.EqualTo(esperado));

    // --- Propiedades de la máquina ---------------------------------------------------------------------------
    [Test]
    [Description("Toda combinación da un estado definido (la función es total)")]
    public void LaFuncionEsTotal()
    {
        foreach (var actual in Enum.GetValues<EstadoDeRed>())
            foreach (var senal in Enum.GetValues<SenalDeRed>())
                foreach (var medido in new[] { false, true })
                    Assert.That(Enum.IsDefined(TransicionesDeRed.Siguiente(actual, senal, medido)), Is.True, $"{actual} + {senal} ({medido})");
    }

    [Test]
    [Description("Sólo el chequeo de salud (o una llamada que respondió) lleva a estar en línea: «hay red» no alcanza (§6)")]
    public void SoloSaludOkEntraEnLineaDesdeFuera()
    {
        foreach (var actual in new[] { SinRed, RedSinServidor })
            foreach (var senal in Enum.GetValues<SenalDeRed>().Where(s => s != SaludOk))
                Assert.That(TransicionesDeRed.Siguiente(actual, senal, perfilMedido: false) is EnLinea or EnLineaMedida, Is.False, $"{actual} + {senal}");
    }

    [TestCase(SinRed)]
    [TestCase(RedSinServidor)]
    [TestCase(EnLinea)]
    [TestCase(EnLineaMedida)]
    [Description("§2.1 — Fuera de línea siempre hay un texto para la persona; en línea, no")]
    public void ElMotivoAcompanaAlEstado(EstadoDeRed estado)
    {
        var motivo = TransicionesDeRed.MotivoDe(estado);
        if (estado == EnLinea) Assert.That(motivo, Is.Null);
        else Assert.That(motivo, Is.Not.Null.And.Not.Empty);
    }
}
