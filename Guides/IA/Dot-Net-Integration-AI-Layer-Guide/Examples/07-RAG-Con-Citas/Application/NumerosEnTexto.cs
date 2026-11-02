// Nuevo (plan §E.2, fila 07: oráculo de la afirmación central, H-029). Saca los valores numéricos de
// un texto, en dígitos o en palabras («4», «cuatro»). La MISMA regla está en
// tools/oraculos/rag-con-citas.jq, para que el oráculo se recalcule desde el registro: si cambia
// una, cambia la otra (una prueba de cableado compara las tablas).
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MovilidadUrbana.Asistente.Application;

public static partial class NumerosEnTexto
{
    /// <summary>Frases de varias palabras, antes que las palabras sueltas.</summary>
    public static readonly IReadOnlyList<(string Frase, int Valor)> Frases = [("ciento diez", 110)];

    /// <summary>
    /// Palabras de número. «un» y «una» NO están: son artículos casi siempre («una localidad») y
    /// contarlas como 1 haría fallar respuestas correctas.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> Palabras = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["cero"] = 0, ["uno"] = 1, ["dos"] = 2, ["tres"] = 3, ["cuatro"] = 4, ["cinco"] = 5, ["seis"] = 6,
        ["siete"] = 7, ["ocho"] = 8, ["nueve"] = 9, ["diez"] = 10, ["once"] = 11, ["doce"] = 12,
        ["trece"] = 13, ["catorce"] = 14, ["quince"] = 15, ["dieciseis"] = 16, ["diecisiete"] = 17,
        ["dieciocho"] = 18, ["diecinueve"] = 19, ["veinte"] = 20, ["treinta"] = 30, ["cuarenta"] = 40,
        ["cincuenta"] = 50, ["sesenta"] = 60, ["setenta"] = 70, ["ochenta"] = 80, ["noventa"] = 90,
        ["cien"] = 100, ["doscientos"] = 200, ["trescientos"] = 300, ["cuatrocientos"] = 400,
        ["quinientos"] = 500, ["seiscientos"] = 600, ["setecientos"] = 700, ["ochocientos"] = 800,
        ["novecientos"] = 900, ["mil"] = 1000
    };

    /// <summary>Minúsculas, sin tildes ni diéresis y sin las citas «[Fuente: …]».</summary>
    public static string Normalizar(string texto)
    {
        var s = texto.ToLowerInvariant()
            .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u').Replace('ü', 'u');
        return Cita().Replace(s, " ");
    }

    /// <summary>Los valores del texto, sin repetir, en orden de aparición.</summary>
    public static IReadOnlyList<int> Valores(string texto)
    {
        var s = Normalizar(texto);
        foreach (var (frase, valor) in Frases) s = Regex.Replace(s, $@"\b{frase}\b", $" {valor} ");
        foreach (var (palabra, valor) in Palabras) s = Regex.Replace(s, $@"\b{palabra}\b", $" {valor} ");
        return Digitos().Matches(s).Select(m => int.Parse(m.Value, CultureInfo.InvariantCulture)).Distinct().ToList();
    }

    [GeneratedRegex(@"\[fuente:[^\]]*\]")]
    private static partial Regex Cita();

    [GeneratedRegex(@"[0-9]+")]
    private static partial Regex Digitos();
}
