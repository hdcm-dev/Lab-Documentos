using System.Security.Cryptography;
using System.Text;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Update;

namespace FrameworkConcept.Update.Tests;

/// <summary>P2 — El verificador ES256 aislado: qué clave, qué curva, qué algoritmo, qué bytes.</summary>
[TestFixture]
public class FirmaTests
{
    private static readonly byte[] Indice = Encoding.UTF8.GetBytes("{\"formato\":1}");

    [Test]
    public void FirmaYVerificaConLaMismaClave()
    {
        using var clave = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var verificador = new VerificadorDeFirmaEs256([new ClaveDePublicacion("k", RolDeClave.Activa, FirmaEs256.ExportarPublica(clave))]);

        var firma = FirmaEs256.Firmar(Indice, clave, "k");

        Assert.That(Convert.FromBase64String(firma.Firma), Has.Length.EqualTo(64), "IEEE P1363: r‖s, 32 + 32 bytes");
        Assert.That(verificador.Verificar(Indice, firma), Is.EqualTo(ResultadoDeVerificacion.Ok));
    }

    [Test]
    [Description("Una clave del conjunto que no es P-256 no sirve aunque la firma cierre")]
    public void OtraCurva_Rechazada()
    {
        using var clave = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var verificador = new VerificadorDeFirmaEs256([new ClaveDePublicacion("k", RolDeClave.Activa, FirmaEs256.ExportarPublica(clave))]);

        var resultado = verificador.Verificar(Indice, FirmaEs256.Firmar(Indice, clave, "k"));

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.ClaveDesconocida));
    }

    [TestCase("RS256", TestName = "Algoritmo distinto de ES256")]
    [TestCase("none", TestName = "Algoritmo «none»")]
    public void AlgoritmoDistinto_Rechazado(string algoritmo)
    {
        using var clave = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var verificador = new VerificadorDeFirmaEs256([new ClaveDePublicacion("k", RolDeClave.Activa, FirmaEs256.ExportarPublica(clave))]);

        var firma = FirmaEs256.Firmar(Indice, clave, "k") with { Algoritmo = algoritmo };

        Assert.That(verificador.Verificar(Indice, firma).Motivo, Is.EqualTo(MotivoDeRechazo.FirmaInvalida));
    }

    [Test]
    public void FirmaQueNoEsBase64_Rechazada()
    {
        using var clave = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var verificador = new VerificadorDeFirmaEs256([new ClaveDePublicacion("k", RolDeClave.Activa, FirmaEs256.ExportarPublica(clave))]);

        Assert.That(verificador.Verificar(Indice, new FirmaDelIndice("k", FirmaDelIndice.Es256, "%%%")).Motivo,
            Is.EqualTo(MotivoDeRechazo.FirmaInvalida));
    }

    [Test]
    [Description("indice.json.sig ilegible: firma inválida, sin leer el índice")]
    public void ArchivoDeFirmaIlegible()
    {
        using var clave = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var verificador = new VerificadorDeFirmaEs256([new ClaveDePublicacion("k", RolDeClave.Activa, FirmaEs256.ExportarPublica(clave))]);

        var resultado = VerificacionDelIndice.Verificar(Indice, Encoding.UTF8.GetBytes("no es json"), verificador, Canales.Dev, 1, DateTimeOffset.UnixEpoch);

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.FirmaInvalida));
    }
}
