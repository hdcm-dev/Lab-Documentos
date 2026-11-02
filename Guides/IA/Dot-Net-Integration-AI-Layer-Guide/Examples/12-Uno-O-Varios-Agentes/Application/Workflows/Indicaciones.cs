// Nuevo (ejemplo 12): el texto que reciben el redactor y el corrector. El MISMO mensaje de corrección
// va al redactor (variante «un-agente») o al corrector (variante «dos-agentes»): lo único que cambia
// entre las variantes es quién lo recibe, con qué instrucciones (la variable es cuántos agentes).
namespace MovilidadUrbana.Asistente.Application.Workflows;

public static class Indicaciones
{
    public const string DelRedactor =
        "Sos el redactor de informes de la encuesta de movilidad urbana. Escribí en castellano un resumen " +
        "breve (una a tres oraciones) que diga el porcentaje de respuestas de cada medio pedido, tal como " +
        "aparece en los datos, con el formato «N %». No inventes cifras ni agregues medios que no se pidieron. " +
        "Devolvé sólo el resumen.";

    public const string DelCorrector =
        "Sos el corrector de informes de la encuesta de movilidad urbana. Recibís un resumen, los datos y los " +
        "errores que encontró un verificador que hace la cuenta en código. Reescribí el resumen corrigiendo " +
        "exactamente esos errores, con el formato «N %», sin agregar otras cifras. Devolvé sólo el resumen.";

    public static string Redaccion(PedidoDeResumen p) =>
        $"{p.Solicitud.Pedido}\n\nDatos de {Resumenes.FuenteDeResultados.Nombre} (JSON):\n{p.Datos}";

    public static string Correccion(PedidoDeCorreccion c) =>
        $"{Redaccion(c.Pedido)}\n\nBorrador anterior:\n{c.BorradorAnterior}\n\n" +
        $"El verificador (una cuenta en código sobre los mismos datos) encontró:\n- {string.Join("\n- ", c.Errores)}\n\n" +
        "Reescribí el resumen corrigiendo eso.";
}
