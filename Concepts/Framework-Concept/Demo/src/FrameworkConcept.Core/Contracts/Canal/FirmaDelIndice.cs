namespace FrameworkConcept.Core.Contracts.Canal;

/// <summary>
/// El archivo <c>indice.json.sig</c>, al lado del índice (P2).
/// Algoritmo: ECDSA P-256 con SHA-256 («ES256»), de la plataforma, sin dependencias. La firma se calcula
/// sobre los BYTES exactos de indice.json y se guarda en formato IEEE P1363 (r‖s, 64 bytes), en base64,
/// que es el formato por defecto de <c>ECDsa.SignData</c> en .NET.
/// </summary>
/// <param name="KeyId">Tiene que coincidir con el <c>keyId</c> de adentro del índice.</param>
public sealed record FirmaDelIndice(string KeyId, string Algoritmo, string Firma)
{
    public const string Es256 = "ES256";
}

/// <summary>Una clave pública del conjunto embebido en el cascarón (P2).</summary>
/// <param name="KeyId">Identificador estable.</param>
/// <param name="Rol">La activa firma; la de reserva vive fuera de línea.</param>
/// <param name="ClavePublica">SubjectPublicKeyInfo DER en base64 (lo que importa <c>ECDsa.ImportSubjectPublicKeyInfo</c>).</param>
/// <param name="Revocada">P2 — El cliente acepta sólo claves no revocadas. En la demo, el rechazo es un rojo
/// provocado de la suite; PUBLICAR una revocación se enseña como diseño, no está implantado.</param>
public sealed record ClaveDePublicacion(string KeyId, RolDeClave Rol, string ClavePublica, bool Revocada = false);

public enum RolDeClave
{
    Activa,
    Reserva
}

/// <summary>Los canales. El canal es un puntero que promueve el mismo artefacto, no una versión (P7).</summary>
public static class Canales
{
    public const string Dev = "dev";
    public const string Beta = "beta";
    public const string Prod = "prod";

    public static IReadOnlyList<string> Todos { get; } = [Dev, Beta, Prod];
}

/// <summary>P2 — Rutas fijas del servidor estático. Las lee el cascarón instalado: cambiarlas cuesta un release por tienda.</summary>
public static class RutasDelCanal
{
    public static string Carpeta(string canal, int puenteMayor) => $"{canal}/puente-{puenteMayor}";

    public static string Indice(string canal, int puenteMayor) => $"{Carpeta(canal, puenteMayor)}/indice.json";

    public static string FirmaDelIndice(string canal, int puenteMayor) => $"{Indice(canal, puenteMayor)}.sig";

    /// <summary>Una entrada del índice: su <c>ruta</c> es relativa a la carpeta del índice.</summary>
    public static string Archivo(string canal, int puenteMayor, string ruta) => $"{Carpeta(canal, puenteMayor)}/{ruta}";
}
