using System.Security.Cryptography;
using System.Text;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Update;
using FrameworkConcept.Gate.Tests.Infraestructura;

namespace FrameworkConcept.Gate.Tests.Semilla;

/// <summary>
/// P2 — La semilla que el APK embebe (semilla/) tiene que verificar con la clave pública que el mismo APK embebe
/// (claves/publica-demo.pem), igual que lo hará el cascarón en el primer arranque. Si alguien regenera el par de
/// claves y no rehace la semilla, el cascarón la descarta («semilla: firma rechazada») y arranca sin contenido:
/// esta prueba lo frena antes de compilar el APK.
/// </summary>
[TestFixture]
public class SemillaTests
{
    private const string Carpeta = "semilla";
    private const string ClavePublica = "claves/publica-demo.pem";

    [Test]
    [Description("P2 — semilla/indice.json.sig verifica con claves/publica-demo.pem; size y sha256 de cada archivo coinciden")]
    public void LaSemillaVerificaConLaClaveVersionada()
    {
        var indicePath = Repositorio.Ruta(Carpeta + "/indice.json");
        if (!File.Exists(indicePath))
            Assert.Ignore("semilla/ está vacía: el APK sale sin semilla (no hay nada que verificar).");

        var indiceJson = File.ReadAllBytes(indicePath);
        var firma = JsonDelContrato.Leer<FirmaDelIndice>(File.ReadAllText(Repositorio.Ruta(Carpeta + "/indice.json.sig")));
        var indice = JsonDelContrato.Leer<IndiceDelCanal>(Encoding.UTF8.GetString(indiceJson));

        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(File.ReadAllText(Repositorio.Ruta(ClavePublica)));
        var clave = new ClaveDePublicacion(firma.KeyId, RolDeClave.Activa, FirmaEs256.ExportarPublica(ecdsa));

        // El mismo verificador que usa el cascarón (Core/Update/FirmaEs256.cs).
        var resultado = new VerificadorDeFirmaEs256([clave]).Verificar(indiceJson, firma);
        Assert.That(resultado.Valida, Is.True,
            $"semilla/indice.json no verifica con {ClavePublica} ({resultado.Motivo}). ¿Se regeneró el par de claves? " +
            "Rehacé la semilla: guion.sh publicar v1, definicion v1, firmar, semilla.");
        Assert.That(indice.KeyId, Is.EqualTo(firma.KeyId), "keyId del índice y de la firma");

        var rutas = indice.Entradas.Contenido.Select(e => (e.Ruta, e.Size, e.Sha256))
            .Concat(indice.Entradas.Definiciones.Select(d => (d.Ruta, d.Size, d.Sha256)));
        foreach (var (ruta, size, sha256) in rutas)
        {
            var archivo = Repositorio.Ruta(Carpeta + "/" + ruta);
            Assert.That(File.Exists(archivo), Is.True, $"el índice de la semilla nombra {ruta}, que no está en semilla/");
            var bytes = File.ReadAllBytes(archivo);
            Assert.That(bytes.LongLength, Is.EqualTo(size), $"size de {ruta}");
            Assert.That(Convert.ToHexStringLower(SHA256.HashData(bytes)), Is.EqualTo(sha256.ToLowerInvariant()), $"sha256 de {ruta}");
        }
    }
}
