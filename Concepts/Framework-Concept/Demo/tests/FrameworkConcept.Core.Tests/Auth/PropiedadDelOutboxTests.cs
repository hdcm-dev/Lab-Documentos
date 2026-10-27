using FrameworkConcept.Core.Auth;

namespace FrameworkConcept.Core.Tests.Auth;

/// <summary>P4 — El outbox tiene dueño (DISENO §3.5): nunca sale una fila de otro dueño; sin sesión válida no sale nada.</summary>
[TestFixture]
public class PropiedadDelOutboxTests
{
    private static readonly string Encuestador = PropiedadDelOutbox.DuenoOidc("sub-encuestador");
    private static readonly string Supervisora = PropiedadDelOutbox.DuenoOidc("sub-supervisora");

    private static SesionInformada Oidc(EstadoDeSesion estado) => new(ModoDeSesion.Oidc, estado, "x", DesbloqueoLocal.Disponible);

    [Test]
    public void ElDuenoOidcLlevaElPrefijo() =>
        Assert.That(Encuestador, Is.EqualTo("oidc:sub-encuestador"));

    [Test]
    public void SaleLaFilaPropiaConSesionActiva() =>
        Assert.That(PropiedadDelOutbox.PuedeEnviarse(Encuestador, Encuestador, Oidc(EstadoDeSesion.Activa)), Is.True);

    [Test]
    public void NuncaSaleUnaFilaDeOtroDueno()
    {
        foreach (var estado in Enum.GetValues<EstadoDeSesion>())
            Assert.That(PropiedadDelOutbox.PuedeEnviarse(Encuestador, Supervisora, Oidc(estado)), Is.False, estado.ToString());
    }

    [TestCase(EstadoDeSesion.Vencida)]
    [TestCase(EstadoDeSesion.Revocada)]
    [TestCase(EstadoDeSesion.Bloqueada)]
    [TestCase(EstadoDeSesion.Ingresando)]
    [TestCase(EstadoDeSesion.SinCuenta)]
    public void SinSesionValidaNoSaleNiLaPropia(EstadoDeSesion estado) =>
        Assert.That(PropiedadDelOutbox.PuedeEnviarse(Encuestador, Encuestador, Oidc(estado)), Is.False);

    [Test]
    public void SinIdentidadPresenteNoSaleNada() =>
        Assert.That(PropiedadDelOutbox.PuedeEnviarse(Encuestador, null, Oidc(EstadoDeSesion.Activa)), Is.False);

    [Test]
    public void ModoDispositivoEnviaLasSuyas() =>
        Assert.That(PropiedadDelOutbox.PuedeEnviarse("dispositivo-1", "dispositivo-1", SesionInformada.PorDispositivo), Is.True);

    [Test]
    public void ModoDispositivoNoEnviaFilasDeOtraInstalacionNiDeUnaCuenta()
    {
        Assert.That(PropiedadDelOutbox.PuedeEnviarse("dispositivo-viejo", "dispositivo-1", SesionInformada.PorDispositivo), Is.False);
        Assert.That(PropiedadDelOutbox.PuedeEnviarse(Encuestador, "dispositivo-1", SesionInformada.PorDispositivo), Is.False);
    }

    [Test]
    public void ElDuenoSeComparaExacto() =>
        // «oidc:abc» y «oidc:ABC» son dos personas distintas: el sub es opaco y sensible a mayúsculas (OIDC Core §2).
        Assert.That(PropiedadDelOutbox.PuedeEnviarse("oidc:abc", "oidc:ABC", Oidc(EstadoDeSesion.Activa)), Is.False);
}
