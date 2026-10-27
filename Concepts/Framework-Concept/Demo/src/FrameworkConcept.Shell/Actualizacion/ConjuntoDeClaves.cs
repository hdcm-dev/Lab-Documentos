using System.Security.Cryptography;
using FrameworkConcept.Core.Contracts.Canal;

namespace FrameworkConcept.Shell.Actualizacion;

/// <summary>
/// P2 — El conjunto de claves embebido en el cascarón. En la demo hay una sola: la pública de demostración
/// (claves/publica-demo.pem, que genera «FrameworkConcept.Tools claves»), con su keyId. La de reserva fuera de línea
/// y publicar una revocación son diseño, no están implantados (P11).
/// Si la clave se regenera, este cascarón deja de aceptar el canal: hay que recompilarlo.
/// </summary>
public static class ConjuntoDeClaves
{
    public const string Archivo = "claves/publica-demo.pem";

    public static async Task<IReadOnlyList<ClaveDePublicacion>> CargarAsync(string keyId)
    {
        try
        {
            await using var flujo = await FileSystem.OpenAppPackageFileAsync(Archivo);
            using var lector = new StreamReader(flujo);
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(await lector.ReadToEndAsync());
            var spki = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo());
            Bitacora.Log($"claves: {keyId} cargada");
            return [new ClaveDePublicacion(keyId, RolDeClave.Activa, spki)];
        }
        catch (Exception ex) when (ex is FileNotFoundException or IOException or CryptographicException or ArgumentException)
        {
            // Sin clave embebida, ningún índice verifica: el actualizador rechaza todo (ClaveDesconocida) y la app
            // sigue con lo instalado. Se anota para que el diagnóstico lo muestre.
            Bitacora.Error("claves: sin clave pública embebida", ex);
            return [];
        }
    }
}
