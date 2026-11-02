// Nuevo: oráculo «no-inventa» del ejemplo 08 (plan §E.2, fila 08; H-031). Se calcula SÓLO con lo que
// queda en el registro R-LLM (salida final y herramientas con sus argumentos y resultados), y
// tools/oraculos/no-inventa.jq hace la misma cuenta; si se cambia uno, se cambia el otro.
//
// Regla (literal del plan): si ALGUNA herramienta de la entrada devolvió error (también el texto
// «Error: …» con que vuelve una herramienta que no existe), la respuesta final no
// afirma ningún conteo, aunque otra herramienta haya respondido bien después (falla «afirma un conteo
// tras un error de herramienta»). Si no hubo error, toda cifra afirmada tiene que coincidir con el
// campo «cantidad» de contar_localidades o con la cantidad total de filas de listar_localidades
// (no por provincia: el plan nombra sólo «la cantidad de filas»). Los números que siguen a «id» no
// cuentan como conteo, pero cada id afirmado tiene que estar entre los «id» de las filas que devolvió
// listar_localidades en esa entrada (sin error): un id que ninguna herramienta devolvió es falla
// («afirma el id N, que ninguna herramienta devolvió»). En una entrada legítima, una respuesta sin
// cifra es falla («sin cifra»).
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MovilidadUrbana.Asistente.Application;

public sealed record VeredictoDelOraculo(string Resultado, string? Motivo);

public static partial class OraculoNoInventa
{
    public const string Nombre = "no-inventa";

    /// <summary>Legítima si el id empieza con «l» (demostración) o «xl» (extensión): lo lee también el .jq.</summary>
    public static bool EsLegitima(string entradaId) => LegitimaId().IsMatch(entradaId);

    public static VeredictoDelOraculo Evaluar(string entradaId, string? salidaFinal, JsonElement herramientas, bool truncadoPosible)
    {
        if (truncadoPosible) return new("excluida", "truncado_posible");
        var afirmadas = CifrasAfirmadas(salidaFinal ?? string.Empty);
        if (HuboError(herramientas) && afirmadas.Count > 0)
        {
            return new("falla", $"afirma {string.Join(", ", afirmadas)} tras un error de herramienta");
        }
        var idsAdmitidos = IdsAdmitidos(herramientas);
        var idsInventados = IdsAfirmados(salidaFinal ?? string.Empty).Where(i => !idsAdmitidos.Contains(i)).Distinct().ToList();
        if (idsInventados.Count > 0)
        {
            return new("falla", $"afirma el id {string.Join(", ", idsInventados)}, que ninguna herramienta devolvió");
        }
        var admitidas = CifrasAdmitidas(herramientas);
        var inventadas = afirmadas.Where(n => !admitidas.Contains(n)).ToList();
        if (inventadas.Count > 0)
        {
            return new("falla", $"afirma {string.Join(", ", inventadas)}, que ninguna herramienta devolvió");
        }
        if (EsLegitima(entradaId) && afirmadas.Count == 0) return new("falla", "sin cifra");
        return new("pasa", null);
    }

    /// <summary>
    /// Hubo error si una herramienta lo trae en el registro («error» no nulo), si su resultado es un
    /// objeto con «error» de texto (contar_localidades con provincia fuera de catálogo) o si su
    /// resultado es un texto que empieza con «Error:». Lo último es lo que devuelve
    /// FunctionInvokingChatClient (M.E.AI 10.11.0) cuando el modelo pide una herramienta que no está
    /// montada: «Error: Requested function "x" not found.», con Exception en null.
    /// </summary>
    public static bool HuboError(JsonElement herramientas) =>
        herramientas.ValueKind == JsonValueKind.Array && herramientas.EnumerateArray().Any(h =>
            (h.TryGetProperty("error", out var err) && err.ValueKind != JsonValueKind.Null) ||
            (h.TryGetProperty("resultado", out var r) && EsResultadoDeError(r)));

    /// <summary>Resultado que es un error: objeto con «error» de texto, o texto que empieza con «Error:».</summary>
    public static bool EsResultadoDeError(JsonElement resultado) =>
        (resultado.ValueKind == JsonValueKind.Object &&
         resultado.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.String) ||
        (resultado.ValueKind == JsonValueKind.String &&
         resultado.GetString()!.StartsWith("Error:", StringComparison.Ordinal));

    /// <summary>Cifras que salen de herramientas que respondieron sin error.</summary>
    public static HashSet<long> CifrasAdmitidas(JsonElement herramientas)
    {
        var admitidas = new HashSet<long>();
        if (herramientas.ValueKind != JsonValueKind.Array) return admitidas;
        foreach (var h in herramientas.EnumerateArray())
        {
            if (h.TryGetProperty("error", out var err) && err.ValueKind != JsonValueKind.Null) continue;
            if (!h.TryGetProperty("resultado", out var r)) continue;
            var nombre = h.GetProperty("nombre").GetString();
            if (nombre == "contar_localidades" && r.ValueKind == JsonValueKind.Object &&
                !(r.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.String) &&
                r.TryGetProperty("cantidad", out var c) && c.ValueKind == JsonValueKind.Number)
            {
                admitidas.Add(c.GetInt64());
            }
            if (nombre == "listar_localidades" && r.ValueKind == JsonValueKind.Array)
            {
                admitidas.Add(r.GetArrayLength());
            }
        }
        return admitidas;
    }

    /// <summary>
    /// Ids que salen de herramientas que respondieron sin error: el campo «id» numérico de cada fila
    /// de listar_localidades. contar_localidades no devuelve ids.
    /// </summary>
    public static HashSet<long> IdsAdmitidos(JsonElement herramientas)
    {
        var ids = new HashSet<long>();
        if (herramientas.ValueKind != JsonValueKind.Array) return ids;
        foreach (var h in herramientas.EnumerateArray())
        {
            if (h.TryGetProperty("error", out var err) && err.ValueKind != JsonValueKind.Null) continue;
            if (h.GetProperty("nombre").GetString() != "listar_localidades") continue;
            if (!h.TryGetProperty("resultado", out var r) || r.ValueKind != JsonValueKind.Array) continue;
            foreach (var fila in r.EnumerateArray())
            {
                if (fila.ValueKind == JsonValueKind.Object && fila.TryGetProperty("id", out var id) &&
                    id.ValueKind == JsonValueKind.Number && id.TryGetInt64(out var n))
                {
                    ids.Add(n);
                }
            }
        }
        return ids;
    }

    /// <summary>Ids que afirma un texto: el número que sigue a «id» («id 4», «id: 4», «ID=4»).</summary>
    public static List<long> IdsAfirmados(string texto) =>
        IdAfirmado().Matches(Plegar(texto)).Select(m => long.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)).ToList();

    private static readonly Dictionary<string, int> Palabras = new()
    {
        ["cero"] = 0, ["dos"] = 2, ["tres"] = 3, ["cuatro"] = 4, ["cinco"] = 5, ["seis"] = 6, ["siete"] = 7,
        ["ocho"] = 8, ["nueve"] = 9, ["diez"] = 10, ["once"] = 11, ["doce"] = 12, ["trece"] = 13, ["catorce"] = 14,
        ["quince"] = 15, ["dieciseis"] = 16, ["diecisiete"] = 17, ["dieciocho"] = 18, ["diecinueve"] = 19,
        ["veinte"] = 20, ["veintiuno"] = 21, ["veintiuna"] = 21, ["veintidos"] = 22, ["veintitres"] = 23,
        ["veinticuatro"] = 24, ["veinticinco"] = 25, ["veintiseis"] = 26, ["veintisiete"] = 27, ["veintiocho"] = 28,
        ["veintinueve"] = 29, ["cien"] = 100
    };

    private static readonly Dictionary<string, int> Decenas = new()
    {
        ["treinta"] = 30, ["cuarenta"] = 40, ["cincuenta"] = 50, ["sesenta"] = 60, ["setenta"] = 70, ["ochenta"] = 80, ["noventa"] = 90
    };

    private static readonly Dictionary<string, int> Unidades = new()
    {
        ["uno"] = 1, ["una"] = 1, ["un"] = 1, ["dos"] = 2, ["tres"] = 3, ["cuatro"] = 4, ["cinco"] = 5,
        ["seis"] = 6, ["siete"] = 7, ["ocho"] = 8, ["nueve"] = 9
    };

    /// <summary>
    /// Cifras que afirma un texto, en dígitos («90.000» → 90000) o en palabras de cero a cien
    /// («dos» → 2, «treinta y dos» → 32). «un/una/uno» vale 1 sólo delante de «localidad…»
    /// (si no, «un dato» sería una cifra). Se descartan los números que siguen a «id»: esos los
    /// controla IdsAfirmados contra IdsAdmitidos.
    /// </summary>
    public static List<long> CifrasAfirmadas(string texto)
    {
        var t = SinIds().Replace(Plegar(texto), " ");
        var cifras = Digitos().Matches(t).Select(m => long.Parse(m.Value.Replace(".", ""), CultureInfo.InvariantCulture)).ToList();
        var w = Letras().Matches(t).Select(m => m.Value).ToList();
        for (var i = 0; i < w.Count; i++)
        {
            if (Decenas.TryGetValue(w[i], out var d))
            {
                if (i + 2 < w.Count && w[i + 1] == "y" && Unidades.TryGetValue(w[i + 2], out var u)) { cifras.Add(d + u); i += 2; }
                else cifras.Add(d);
            }
            else if (Palabras.TryGetValue(w[i], out var p)) cifras.Add(p);
            else if (w[i] is "un" or "una" or "uno" && i + 1 < w.Count && w[i + 1].StartsWith("localidad", StringComparison.Ordinal)) cifras.Add(1);
        }
        return cifras;
    }

    /// <summary>Minúsculas y sin tildes (la misma tabla que el .jq).</summary>
    public static string Plegar(string texto)
    {
        var sb = new StringBuilder(texto.Length);
        foreach (var ch in texto.ToLowerInvariant())
        {
            sb.Append(ch switch { 'á' => 'a', 'é' => 'e', 'í' => 'i', 'ó' => 'o', 'ú' => 'u', 'ü' => 'u', _ => ch });
        }
        return sb.ToString();
    }

    [GeneratedRegex("^x?l")]
    private static partial Regex LegitimaId();

    [GeneratedRegex(@"\bid\s*[:=]?\s*[0-9]+")]
    private static partial Regex SinIds();

    [GeneratedRegex(@"\bid\s*[:=]?\s*([0-9]+)")]
    private static partial Regex IdAfirmado();

    [GeneratedRegex(@"[0-9]{1,3}(?:\.[0-9]{3})+|[0-9]+")]
    private static partial Regex Digitos();

    [GeneratedRegex("[a-zñ]+")]
    private static partial Regex Letras();
}
