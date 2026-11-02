// Nuevo (plan §E.2, fila 05; H-024; N3b §3.4). Lo único que la aplicación controla de la caché del
// prefijo es el texto que manda: un prefijo idéntico byte a byte entre consultas, con la pregunta
// AL FINAL. Este archivo arma las tres formas del prefijo que usa el ejemplo.
namespace MovilidadUrbana.Asistente.Application;

public static class PrefijoEstable
{
    /// <summary>
    /// Instrucción + padrón. Empieza por «P»: la vuelta a frío cambia sólo ese carácter. El rótulo
    /// «escrito para el ejemplo» va también en el texto (regla de estímulo, plan §E.1 (ii)).
    /// </summary>
    public static string Construir(string padronEnTexto) =>
        "Padrón de localidades de Movilidad Urbana (escrito para el ejemplo: 70 registros ficticios, " +
        "«Localidad 01» a «Localidad 70»; no son datos reales).\n" +
        "Respondé en castellano, en una o dos oraciones, usando sólo este padrón. " +
        "Si el dato no está en el padrón, decí «no está en el padrón».\n\n" +
        padronEnTexto;

    /// <summary>
    /// Variante «sin-prefijo»: un nonce distinto AL PRINCIPIO. Desde el primer token el texto ya
    /// no coincide con nada cacheado, así que el servidor tiene que procesar todo el prefijo.
    /// </summary>
    public static string ConNonce(string prefijo, string nonce) => $"Consulta {nonce}\n{prefijo}";

    /// <summary>Vuelta a frío: el mismo prefijo con un solo carácter cambiado, el primero.</summary>
    public static string ConPrimerCaracterCambiado(string prefijo)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefijo);
        var primero = prefijo[0];
        var otro = char.IsUpper(primero) ? char.ToLowerInvariant(primero) : char.IsLower(primero) ? char.ToUpperInvariant(primero) : '#';
        return otro + prefijo[1..];
    }
}
