// Nuevo (ejemplo 01, plan §E.2: «la respuesta contiene los nombres de campo de LocalidadPolicy /
// EncuestaPolicy que pide la pregunta; FinishReason = stop»). Lo recalcula tools/oraculos/campos-pedidos.jq
// leyendo sólo el registro: las dos implementaciones tienen que dar lo mismo.
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MovilidadUrbana.Asistente.Application;

public sealed record CampoEsperado(string Campo, IReadOnlyList<string> Formas);

public sealed record EntradaDeEjemplo(string Id, string Texto, string Politica, IReadOnlyList<CampoEsperado> Campos);

public static class OraculoDeCampos
{
    public const string Nombre = "campos-pedidos";

    public static IReadOnlyList<EntradaDeEjemplo> Cargar(string ruta)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(ruta));
        return doc.RootElement.GetProperty("entradas").EnumerateArray().Select(e => new EntradaDeEjemplo(
            e.GetProperty("id").GetString()!,
            e.GetProperty("texto").GetString()!,
            e.GetProperty("politica").GetString()!,
            e.GetProperty("campos").EnumerateArray().Select(c => new CampoEsperado(
                c.GetProperty("campo").GetString()!,
                c.GetProperty("formas").EnumerateArray().Select(f => f.GetString()!).ToList())).ToList())).ToList();
    }

    /// <summary>Minúsculas y sin tildes (la misma normalización que el .jq).</summary>
    public static string Normalizar(string texto)
    {
        var descompuesto = texto.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Una forma de una o dos letras («cp») tiene que ser palabra entera; las demás, subcadena.</summary>
    public static bool Contiene(string normalizado, string forma) =>
        forma.Length <= 2
            ? Regex.IsMatch(normalizado, $@"\b{Regex.Escape(forma)}\b")
            : normalizado.Contains(forma, StringComparison.Ordinal);

    public static IReadOnlyList<string> Faltantes(string? salida, EntradaDeEjemplo entrada)
    {
        var n = Normalizar(salida ?? string.Empty);
        return entrada.Campos.Where(c => !c.Formas.Any(f => Contiene(n, f))).Select(c => c.Campo).ToList();
    }
}
