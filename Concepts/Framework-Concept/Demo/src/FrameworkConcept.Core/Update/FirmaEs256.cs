using System.Security.Cryptography;
using FrameworkConcept.Core.Contracts.Canal;

namespace FrameworkConcept.Core.Update;

/// <summary>
/// P2 — Firmar y verificar el índice con ECDSA P-256 + SHA-256 («ES256»), con la criptografía de la plataforma:
/// sin paquetes NuGet. En Android, <c>ECDsa.Create()</c> usa la implementación del sistema (E-SEG-1, folio 016).
///
/// Las dos mitades viven juntas a propósito: la herramienta que firma y el cascarón que verifica tienen que usar
/// el mismo formato de firma (IEEE P1363, r‖s de 64 bytes, el que .NET usa por defecto) sobre los mismos bytes.
/// </summary>
public static class FirmaEs256
{
    /// <summary>Firma los bytes EXACTOS de indice.json. La clave privada no sale de quien publica.</summary>
    public static FirmaDelIndice Firmar(ReadOnlySpan<byte> indiceJson, ECDsa clavePrivada, string keyId) =>
        new(keyId, FirmaDelIndice.Es256, Convert.ToBase64String(clavePrivada.SignData(indiceJson, HashAlgorithmName.SHA256)));

    /// <summary>La clave pública en el formato que se embebe en el cascarón (SubjectPublicKeyInfo DER, base64).</summary>
    public static string ExportarPublica(ECDsa clave) => Convert.ToBase64String(clave.ExportSubjectPublicKeyInfo());
}

/// <summary>
/// El verificador del cascarón: conoce el conjunto de claves embebido (activa y reserva, con su keyId) y acepta sólo
/// claves conocidas y NO revocadas (P2). El orden de los rechazos importa para el diagnóstico: primero «¿qué clave?»,
/// después «¿la firma cierra?».
/// </summary>
public sealed class VerificadorDeFirmaEs256(IEnumerable<ClaveDePublicacion> claves) : IVerificadorDeFirma
{
    private readonly Dictionary<string, ClaveDePublicacion> _claves =
        claves.ToDictionary(c => c.KeyId, StringComparer.Ordinal);

    public ResultadoDeVerificacion Verificar(ReadOnlySpan<byte> indiceJson, FirmaDelIndice firma)
    {
        if (!string.Equals(firma.Algoritmo, FirmaDelIndice.Es256, StringComparison.Ordinal))
            return ResultadoDeVerificacion.Rechazo(MotivoDeRechazo.FirmaInvalida);

        if (!_claves.TryGetValue(firma.KeyId, out var clave))
            return ResultadoDeVerificacion.Rechazo(MotivoDeRechazo.ClaveDesconocida);

        if (clave.Revocada)
            return ResultadoDeVerificacion.Rechazo(MotivoDeRechazo.ClaveRevocada);

        byte[] bytesDeFirma;
        try
        {
            bytesDeFirma = Convert.FromBase64String(firma.Firma);
        }
        catch (FormatException)
        {
            return ResultadoDeVerificacion.Rechazo(MotivoDeRechazo.FirmaInvalida);
        }

        using var ecdsa = ECDsa.Create();
        try
        {
            ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(clave.ClavePublica), out _);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            // Una clave embebida que no se puede leer es un defecto del cascarón, no del canal: se rechaza igual.
            return ResultadoDeVerificacion.Rechazo(MotivoDeRechazo.ClaveDesconocida);
        }

        // Sólo P-256: una clave de otra curva en el conjunto no sirve aunque la firma cierre.
        if (ecdsa.KeySize != 256)
            return ResultadoDeVerificacion.Rechazo(MotivoDeRechazo.ClaveDesconocida);

        return ecdsa.VerifyData(indiceJson, bytesDeFirma, HashAlgorithmName.SHA256)
            ? ResultadoDeVerificacion.Ok
            : ResultadoDeVerificacion.Rechazo(MotivoDeRechazo.FirmaInvalida);
    }
}
