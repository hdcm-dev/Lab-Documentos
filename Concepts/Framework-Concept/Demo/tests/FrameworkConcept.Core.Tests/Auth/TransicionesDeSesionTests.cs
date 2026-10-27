using FrameworkConcept.Core.Auth;
using static FrameworkConcept.Core.Auth.EstadoDeSesion;
using static FrameworkConcept.Core.Auth.EventoDeSesion;

namespace FrameworkConcept.Core.Tests.Auth;

/// <summary>La máquina de la sesión: una fila por renglón de la tabla de DISENO-RED-Y-AUTENTICACION.md §3.2.</summary>
[TestFixture]
public class TransicionesDeSesionTests
{
    // Actual, evento, siguiente — en el orden de la tabla.
    [TestCase(SinCuenta, IngresoIniciado, Ingresando)]
    [TestCase(Vencida, IngresoIniciado, Ingresando)]
    [TestCase(Revocada, IngresoIniciado, Ingresando)]
    [TestCase(Ingresando, IngresoExitoso, Activa)]
    [TestCase(Activa, Bloquear, Bloqueada)]
    [TestCase(Bloqueada, DesbloqueoExitoso, Activa)]
    [TestCase(Bloqueada, DesbloqueoCancelado, Bloqueada)]
    [TestCase(Bloqueada, ClaveInvalidada, SinCuenta)]   // corrector E03: sin prueba local no se abre la entrada
    [TestCase(Activa, ClaveInvalidada, Revocada)]
    [TestCase(Activa, RenovacionRechazada, Revocada)]
    [TestCase(Activa, SesionVencida, Vencida)]
    public void UnaFilaPorRenglon(EstadoDeSesion actual, EventoDeSesion evento, EstadoDeSesion esperado) =>
        Assert.That(TransicionesDeSesion.Siguiente(actual, evento), Is.EqualTo(esperado));

    [TestCase(SinCuenta, SinCuenta)]
    [TestCase(Vencida, Vencida)]
    [TestCase(Revocada, Revocada)]
    public void IngresoFallidoVuelveAlEstadoPrevio(EstadoDeSesion antes, EstadoDeSesion esperado) =>
        Assert.That(TransicionesDeSesion.Siguiente(Ingresando, IngresoFallido, antes), Is.EqualTo(esperado));

    [Test]
    public void IngresoFallidoNuncaVuelveAUnEstadoConEntradaQueNoTenia() =>
        // Si quien llama pasa un previo absurdo (Activa), se vuelve a SinCuenta: no se inventa una entrada abierta.
        Assert.That(TransicionesDeSesion.Siguiente(Ingresando, IngresoFallido, Activa), Is.EqualTo(SinCuenta));

    [TestCase(Ingresando)]
    [TestCase(Activa)]
    [TestCase(Bloqueada)]
    [TestCase(Vencida)]
    [TestCase(Revocada)]
    public void CierreDesdeCadaEstadoVaASinCuenta(EstadoDeSesion actual) =>
        Assert.That(TransicionesDeSesion.Siguiente(actual, Cierre), Is.EqualTo(SinCuenta));

    [Test]
    public void CierreEnSinCuentaNoCambiaNada() =>
        Assert.That(TransicionesDeSesion.Siguiente(SinCuenta, Cierre), Is.EqualTo(SinCuenta));

    // «Otra combinación: sin cambio». Algunas que importan: no se puede ingresar estando activa ni bloqueada, ni
    // desbloquear algo que no está bloqueado, ni vencer una sesión bloqueada (primero hay que abrirla).
    [TestCase(Activa, IngresoIniciado)]
    [TestCase(Bloqueada, IngresoIniciado)]
    [TestCase(Activa, DesbloqueoExitoso)]
    [TestCase(SinCuenta, DesbloqueoExitoso)]
    [TestCase(SinCuenta, Bloquear)]
    [TestCase(Vencida, Bloquear)]
    [TestCase(Bloqueada, SesionVencida)]
    [TestCase(Bloqueada, RenovacionRechazada)]
    [TestCase(SinCuenta, IngresoExitoso)]
    [TestCase(Vencida, ClaveInvalidada)]
    public void OtraCombinacionNoCambia(EstadoDeSesion actual, EventoDeSesion evento) =>
        Assert.That(TransicionesDeSesion.Siguiente(actual, evento), Is.EqualTo(actual));

    [Test]
    public void TodaCombinacionDaUnEstadoConocido()
    {
        foreach (var actual in Enum.GetValues<EstadoDeSesion>())
        foreach (var evento in Enum.GetValues<EventoDeSesion>())
            Assert.That(Enum.IsDefined(TransicionesDeSesion.Siguiente(actual, evento)), $"{actual} + {evento}");
    }

    // --- Arranque en frío (DISENO §3.2, tabla «Estado al arrancar en frío») ---

    private static readonly DateTimeOffset Ahora = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public void ArranqueSinIdentidadEsSinCuenta() =>
        Assert.That(TransicionesDeSesion.AlArrancar(false, true), Is.EqualTo(SinCuenta));

    [Test]
    public void ArranqueConIdentidadYDesbloqueoEsBloqueada() =>
        Assert.That(TransicionesDeSesion.AlArrancar(true, true), Is.EqualTo(Bloqueada));

    // Corrector E03 — Sin desbloqueo activado no hay prueba local posible: la entrada NO se abre (antes, Vencida con
    // la entrada abierta y lo registrado quedaba a nombre del último dueño sin que nadie probara quién era).
    [Test]
    public void ArranqueConIdentidadSinDesbloqueoPideLaCuenta() =>
        Assert.That(TransicionesDeSesion.AlArrancar(true, false), Is.EqualTo(SinCuenta));

    [Test]
    public void EnFrioNuncaSeAbreLaEntradaSinPruebaLocal()
    {
        foreach (var identidad in new[] { false, true })
        foreach (var desbloqueo in new[] { false, true })
        {
            var estado = TransicionesDeSesion.AlArrancar(identidad, desbloqueo);
            var entrada = EvaluadorDeAccionesAbre(estado);
            Assert.That(entrada, Is.False, $"identidad={identidad} desbloqueo={desbloqueo} → {estado}");
        }
    }

    private static bool EvaluadorDeAccionesAbre(EstadoDeSesion estado) =>
        FrameworkConcept.Core.Red.EvaluadorDeAcciones.Evaluar(
            FrameworkConcept.Core.Red.PoliticaDeAcciones.Buscar("localidades.guardar")!,
            FrameworkConcept.Core.Red.EstadoDeRed.SinRed,
            new SesionInformada(ModoDeSesion.Oidc, estado, null, DesbloqueoLocal.Disponible)).Permitida;

    // --- Volver de segundo plano (borrador §4) ---

    [Test]
    public void SeBloqueaAlVolverDespuesDelUmbral() =>
        Assert.That(TransicionesDeSesion.DebeBloquearAlVolver(Activa, true, Ahora.AddMinutes(-5), Ahora, TimeSpan.FromMinutes(5)), Is.True);

    [Test]
    public void NoSeBloqueaAntesDelUmbral() =>
        Assert.That(TransicionesDeSesion.DebeBloquearAlVolver(Activa, true, Ahora.AddMinutes(-4), Ahora, TimeSpan.FromMinutes(5)), Is.False);

    [Test]
    public void SinDesbloqueoActivadoNoHayNadaQueBloquear() =>
        Assert.That(TransicionesDeSesion.DebeBloquearAlVolver(Activa, false, Ahora.AddHours(-1), Ahora, TimeSpan.FromMinutes(5)), Is.False);

    [TestCase(Vencida)]
    [TestCase(SinCuenta)]
    [TestCase(Bloqueada)]
    public void SoloSeBloqueaUnaSesionActiva(EstadoDeSesion actual) =>
        Assert.That(TransicionesDeSesion.DebeBloquearAlVolver(actual, true, Ahora.AddHours(-1), Ahora, TimeSpan.FromMinutes(5)), Is.False);

    // --- D-03 = T1 ---

    [Test]
    public void ApiViejaNoOfreceElDesbloqueo() =>
        Assert.That(ReglaDelDesbloqueo.Evaluar(29, true, false), Is.EqualTo((DesbloqueoLocal.NoDisponible, ReglaDelDesbloqueo.MotivoApiVieja)));

    [Test]
    public void SinBloqueoDePantallaNoOfreceElDesbloqueo() =>
        Assert.That(ReglaDelDesbloqueo.Evaluar(33, false, false), Is.EqualTo((DesbloqueoLocal.NoDisponible, ReglaDelDesbloqueo.MotivoSinBloqueo)));

    [Test]
    public void SinBloqueoDePantallaNoValeNiActivado() =>
        // Quitar el bloqueo de pantalla invalida la clave del Keystore: aunque estuviera activado, ya no se ofrece.
        Assert.That(ReglaDelDesbloqueo.Evaluar(33, false, true).Desbloqueo, Is.EqualTo(DesbloqueoLocal.NoDisponible));

    [Test]
    public void ConApi30YBloqueoSeOfrece() =>
        Assert.That(ReglaDelDesbloqueo.Evaluar(30, true, false), Is.EqualTo((DesbloqueoLocal.Disponible, (string?)null)));

    [Test]
    public void ActivadoSeInforma() =>
        Assert.That(ReglaDelDesbloqueo.Evaluar(33, true, true).Desbloqueo, Is.EqualTo(DesbloqueoLocal.Activado));
}
