// Nuevo: las dos líneas base sin modelo (plan §E.2; guía §7.5). Una recuperación con embeddings sólo
// dice algo si le gana a esto, y sólo se afirma con la regla pareada de §10.4 (ComparacionPareada).
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>Línea base léxica: puntaje = cantidad de palabras compartidas entre la pregunta y el texto de plantilla.v1.</summary>
public static partial class ReferenciaLexica
{
    // Palabras sin contenido que no cuentan (declaradas; las de menos de 3 letras ya quedan afuera).
    public static readonly IReadOnlySet<string> Vacias = new HashSet<string>(StringComparer.Ordinal)
    {
        "que", "una", "uno", "los", "las", "del", "por", "con", "para", "hay", "como", "tiene", "puede",
        "puedo", "cual", "cuanto", "cuantos", "cuantas", "sobre", "esos", "esas", "entre", "mas", "despues"
    };

    public static IReadOnlySet<string> Palabras(string texto)
    {
        var sinTildes = new StringBuilder();
        foreach (var c in texto.ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sinTildes.Append(c);
        }
        return NoPalabra().Split(sinTildes.ToString())
            .Where(p => p.Length > 2 && !Vacias.Contains(p))
            .ToHashSet(StringComparer.Ordinal);
    }

    public static IReadOnlyList<Posicion> Ordenar(IReadOnlyList<Fragmento> corpus, string pregunta)
    {
        var deLaPregunta = Palabras(pregunta);
        var porId = corpus.ToDictionary(f => f.Id, f => Palabras(f.Texto));
        return Busqueda.Ordenar(corpus.Select(f => f.Id).ToList(), id => porId[id].Count(deLaPregunta.Contains));
    }

    [GeneratedRegex(@"\W+")]
    private static partial Regex NoPalabra();
}

/// <summary>
/// Línea base al azar. Para comparar en pareado hace falta un ordenamiento por pregunta: se usa una
/// permutación con semilla fija (Random(semilla) es estable entre corridas). Además se informa la
/// esperanza analítica (con 12 fragmentos y un relevante: hit@1 = 1/12, hit@3 = 3/12, MRR ≈ 0,259).
/// </summary>
public static class ReferenciaAlAzar
{
    public const int Semilla = 42;

    public static IReadOnlyList<Posicion> Ordenar(IReadOnlyList<Fragmento> corpus, int indiceDePregunta)
    {
        var ids = corpus.Select(f => f.Id).ToArray();
        new Random(Semilla + indiceDePregunta).Shuffle(ids);
        return ids.Select((id, i) => new Posicion(id, ids.Length - i)).ToList();
    }

    /// <summary>P(algún relevante en los k primeros) con n fragmentos y r relevantes: 1 − C(n−r, k) / C(n, k).</summary>
    public static double HitEsperado(int n, int r, int k) => 1 - Combinaciones(n - r, k) / Combinaciones(n, k);

    /// <summary>E[1 / rango del primer relevante] = Σ_j (1/j) · P(primer relevante en j).</summary>
    public static double RrEsperado(int n, int r)
    {
        double suma = 0;
        for (var j = 1; j <= n - r + 1; j++)
        {
            // Los j−1 primeros no relevantes y el j-ésimo relevante.
            var p = Combinaciones(n - j, r - 1) / Combinaciones(n, r);
            suma += p / j;
        }
        return suma;
    }

    public static double Combinaciones(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        double c = 1;
        for (var i = 1; i <= k; i++) c = c * (n - k + i) / i;
        return c;
    }
}

/// <summary>
/// Regla única de comparación (guía §10.4; plan §E.1, H2-029): mismas preguntas por las dos
/// variantes; b = acierta sólo A, c = acierta sólo B; se afirma una diferencia sólo si la binomial
/// exacta bilateral sobre b + c da p &lt; 0,05 (hacen falta al menos 6 discordantes en el mismo sentido).
/// </summary>
public sealed record ComparacionPareada(string A, string B, string Medida, int SoloA, int SoloB, double P)
{
    public bool SeAfirmaDiferencia => SoloA + SoloB > 0 && P < 0.05;

    public static ComparacionPareada Comparar(string a, string b, string medida, IReadOnlyList<bool> aciertosA, IReadOnlyList<bool> aciertosB)
    {
        if (aciertosA.Count != aciertosB.Count) throw new ArgumentException("El diseño pareado pide las mismas preguntas por las dos variantes.");
        var soloA = aciertosA.Zip(aciertosB).Count(x => x.First && !x.Second);
        var soloB = aciertosA.Zip(aciertosB).Count(x => !x.First && x.Second);
        return new(a, b, medida, soloA, soloB, BinomialBilateral(soloA, soloB));
    }

    public static double BinomialBilateral(int b, int c)
    {
        var n = b + c;
        if (n == 0) return 1;
        var cola = 0.0;
        for (var i = 0; i <= Math.Min(b, c); i++) cola += ReferenciaAlAzar.Combinaciones(n, i);
        return Math.Min(1, 2 * cola / Math.Pow(2, n));
    }
}
