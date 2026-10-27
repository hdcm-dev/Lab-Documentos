using System.Security.Cryptography;
using System.Text.Json;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;

namespace FrameworkConcept.Core.Update;

/// <summary>El índice ya verificado, o el motivo del rechazo.</summary>
public sealed record IndiceVerificado(IndiceDelCanal? Indice, MotivoDeRechazo Motivo, string Detalle)
{
    public bool EsValido => Indice is not null;
}

/// <summary>
/// P2 — La verificación del índice, en el orden del dictamen. Es una función: bytes de entrada, veredicto de salida,
/// sin red ni archivos, para que cada paso tenga su prueba.
///
///   1. firma y clave (conocida, no revocada)   — sobre los BYTES recibidos, antes de leer el JSON
///   2. el keyId de adentro coincide con el de la firma
///   3. formato conocido
///   4. expires                                 — un índice vencido no vale aunque la firma sea buena (C08, «freeze»)
///   5. canal y mayor del puente                — un índice de otro canal u otro mayor no es para este cascarón
///
/// La versión estrictamente mayor y el sha256 se verifican después, por entrada (ver <see cref="Actualizador"/>).
/// </summary>
public static class VerificacionDelIndice
{
    public static IndiceVerificado Verificar(
        byte[] indiceJson,
        byte[] firmaJson,
        IVerificadorDeFirma verificador,
        string canal,
        int puenteMayor,
        DateTimeOffset ahora)
    {
        FirmaDelIndice firma;
        try
        {
            firma = JsonDelContrato.Leer<FirmaDelIndice>(System.Text.Encoding.UTF8.GetString(firmaJson));
        }
        catch (JsonException ex)
        {
            return Rechazo(MotivoDeRechazo.FirmaInvalida, "indice.json.sig ilegible: " + ex.Message);
        }

        var resultado = verificador.Verificar(indiceJson, firma);
        if (!resultado.Valida)
            return Rechazo(resultado.Motivo, $"firma rechazada (keyId «{firma.KeyId}»): {resultado.Motivo}");

        // Sólo ahora se lee el JSON: nada de lo que dice un índice sin firma válida se usa.
        IndiceDelCanal indice;
        try
        {
            indice = JsonDelContrato.Leer<IndiceDelCanal>(System.Text.Encoding.UTF8.GetString(indiceJson));
        }
        catch (JsonException ex)
        {
            return Rechazo(MotivoDeRechazo.FormatoDesconocido, "indice.json ilegible: " + ex.Message);
        }

        if (!string.Equals(indice.KeyId, firma.KeyId, StringComparison.Ordinal))
            return Rechazo(MotivoDeRechazo.FirmaInvalida, $"el índice dice keyId «{indice.KeyId}» y la firma «{firma.KeyId}»");

        if (indice.Formato != IndiceDelCanal.FormatoActual)
            return Rechazo(MotivoDeRechazo.FormatoDesconocido, $"formato {indice.Formato}, este cascarón lee {IndiceDelCanal.FormatoActual}");

        if (ahora >= indice.Expires)
            return Rechazo(MotivoDeRechazo.IndiceVencido, $"índice vencido el {indice.Expires:O}");

        if (!string.Equals(indice.Canal, canal, StringComparison.Ordinal))
            return Rechazo(MotivoDeRechazo.OtroCanal, $"índice del canal «{indice.Canal}», este cascarón es de «{canal}»");

        if (indice.PuenteMayor != puenteMayor)
            return Rechazo(MotivoDeRechazo.OtroMayorDelPuente, $"índice del puente {indice.PuenteMayor}, este cascarón implementa {puenteMayor}");

        return new IndiceVerificado(indice, MotivoDeRechazo.Ninguno, "índice válido");
    }

    /// <summary>sha256 en hex minúscula, la forma en que lo escribe el índice.</summary>
    public static string Sha256Hex(ReadOnlySpan<byte> datos) => Convert.ToHexStringLower(SHA256.HashData(datos));

    private static IndiceVerificado Rechazo(MotivoDeRechazo motivo, string detalle) => new(null, motivo, detalle);
}
