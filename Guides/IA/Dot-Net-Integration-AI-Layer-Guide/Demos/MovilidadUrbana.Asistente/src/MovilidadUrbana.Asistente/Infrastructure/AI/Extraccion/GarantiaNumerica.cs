// Nuevo. Delta: Agregado por la IA — garantía del código contra números inventados (plan §F.7, H2-032).
using System.Globalization;
using System.Text.RegularExpressions;

namespace MovilidadUrbana.Asistente.Infrastructure.AI.Extraccion;

/// <summary>
/// Un campo anulable no impide que el modelo invente un número plausible, que pasa la regla. Esto
/// sí: el adaptador acepta un valor numérico propuesto sólo si ese valor aparece en el texto de
/// entrada; si no, lo trata como ausente. Cuánto inventa el modelo se mide aparte (03, extensión).
/// Parseo con cultura invariante; «90.000» y «90000» son el mismo número; «12,5» es 12.5.
/// </summary>
public static partial class GarantiaNumerica
{
    public static IReadOnlyList<double> NumerosDe(string texto)
    {
        var numeros = new List<double>();
        foreach (Match m in Numero().Matches(texto))
        {
            var crudo = m.Value;
            // Separador de miles con punto (90.000) → sin separador; coma decimal → punto.
            if (Miles().IsMatch(crudo)) crudo = crudo.Replace(".", string.Empty, StringComparison.Ordinal);
            crudo = crudo.Replace(',', '.');
            if (double.TryParse(crudo, NumberStyles.Float, CultureInfo.InvariantCulture, out var valor)) numeros.Add(valor);
        }
        return numeros;
    }

    public static int? Entero(int? propuesto, string texto) =>
        propuesto is int v && NumerosDe(texto).Any(n => Math.Abs(n - v) < 1e-9) ? v : null;

    public static double? Real(double? propuesto, string texto) =>
        propuesto is double v && NumerosDe(texto).Any(n => Math.Abs(n - v) < 1e-9) ? v : null;

    [GeneratedRegex(@"\d+(?:[.,]\d+)*")]
    private static partial Regex Numero();

    [GeneratedRegex(@"^\d{1,3}(?:\.\d{3})+$")]
    private static partial Regex Miles();
}
