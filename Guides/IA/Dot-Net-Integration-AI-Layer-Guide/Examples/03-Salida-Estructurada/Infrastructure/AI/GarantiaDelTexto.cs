// Nuevo (plan §E.2 fila 03; H2-032). La garantía del código contra la invención: un valor numérico
// propuesto (habitantes, código postal) se acepta sólo si aparece en el texto de entrada. Si no
// aparece, se trata como ausente (null) y la aplicación lo pide; la persona puede confirmarlo.
// El campo anulable le da al modelo un lugar para no inventar; esto es lo que impide que lo
// inventado llegue a las reglas, porque un número plausible pasaría HabitantesValidos.
// Cubre los dos campos numéricos de LocalidadPropuesta (codigoPostal y habitantes): «CP 34» con un
// «3400» propuesto descarta el código postal (REC-16). Nombre y provincia son texto y no pasan por
// acá: la provincia la decide el catálogo y el nombre, LocalidadRules (y la persona, que confirma).
using System.Globalization;
using System.Text.RegularExpressions;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

public static partial class GarantiaDelTexto
{
    /// <summary>
    /// Números del texto: «90000», «90.000» (punto de miles) y «3432». Cultura invariante (T6): el
    /// punto entre grupos de tres dígitos es separador de miles. El oráculo usa la misma expresión.
    /// </summary>
    public static IReadOnlyList<string> Numeros(string texto) =>
        Numero().Matches(texto).Select(m => m.Value.Replace(".", string.Empty, StringComparison.Ordinal)).ToList();

    /// <summary>Devuelve la propuesta con los valores que no están en el texto puestos en null, y cuáles fueron.</summary>
    public static (LocalidadPropuesta Propuesta, IReadOnlyList<string> Descartados) Aplicar(LocalidadPropuesta propuesta, string texto)
    {
        var numeros = Numeros(texto);
        var descartados = new List<string>();

        var cp = propuesta.CodigoPostal?.Trim();
        if (!string.IsNullOrEmpty(cp) && !numeros.Contains(cp, StringComparer.Ordinal))
        {
            descartados.Add("codigoPostal");
            cp = null;
        }

        var habitantes = propuesta.Habitantes;
        if (habitantes is int h)
        {
            var comoTexto = h.ToString(CultureInfo.InvariantCulture);
            // El número tiene que estar en el texto y no ser el mismo token que ya se usó como código postal.
            var usos = numeros.Count(n => n == comoTexto) - (cp == comoTexto ? 1 : 0);
            if (usos < 1)
            {
                descartados.Add("habitantes");
                habitantes = null;
            }
        }

        return (propuesta with { CodigoPostal = cp, Habitantes = habitantes }, descartados);
    }

    [GeneratedRegex(@"\d{1,3}(?:\.\d{3})+|\d+")]
    private static partial Regex Numero();
}
