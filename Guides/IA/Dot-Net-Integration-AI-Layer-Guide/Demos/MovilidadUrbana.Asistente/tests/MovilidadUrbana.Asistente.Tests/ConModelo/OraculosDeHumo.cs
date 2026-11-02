// Nuevo. Oráculos del humo con modelo (plan §E.5: «su M, su oráculo y su evidencia de conducta son los
// del ejemplo hermano»). Cada uno es la regla del hermano, adaptada a lo que la demo deja en el
// registro; la misma regla, leída sólo del registro, está en tools/oraculos/demo-pd*.jq (en la
// carpeta de la guía). Si cambia una, cambia la otra. Funciones puras: Cableado/OraculosDeHumoTests.cs
// las prueba sin servidor, con los casos que el oráculo tiene que separar.
//
//   · PD-2 ← 07 «rag-con-citas» (H-029), caso «respondible con valor»: cita válida, la fuente es el
//     relevante del oro, los valores del literal citado están en la respuesta y no aparece otro valor
//     («cita y contradice» falla). La demo responde en JSON {respuesta, fuente} con una sola fuente,
//     no con «[Fuente: id]» en el texto: por eso no se reusa el .jq de 07 tal cual.
//   · PD-3 ← 03 «salida-estructurada»: la decisión que las reglas toman sobre la salida cruda, con la
//     garantía del texto de 03 (código postal y habitantes), tiene que ser «faltan:habitantes».
//   · PD-6 ← 11 «agente-con-aprobacion», rama sin aprobación: el modelo pidió eliminar_localidad por
//     Resistencia y el pedido que armó el código (guardado en el «motivo» de la entrada) muestra
//     «Resistencia» (Chaco). 11 mide la rama aprobada («lo borrado = lo mostrado»); PD-6 rechaza, así
//     que se compara lo mostrado con lo pedido. Que la base no cambie es invariante de código: lo
//     afirma Humo.Pd6 siempre (AC-4) y no se recalcula del registro, porque eliminar_localidad
//     devuelve texto tanto si borra como si no.
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Tests.ConModelo;

public static partial class OraculosDeHumo
{
    /// <summary>Relevante de pd2-01: el mismo objetivo que «r03» del conjunto de oro de 06 (Examples/06…/oro).</summary>
    public const string RelevanteDePd2 = "LocalidadPolicy.RequisitoDelCodigoPostal";

    // ---------- PD-2 (regla de 07) ----------

    /// <summary>
    /// <paramref name="fuente" /> ya validada contra lo recuperado (<paramref name="citaValida" />);
    /// <paramref name="literal" /> es el texto del fragmento citado.
    /// </summary>
    public static bool Pd2(bool citaValida, string? fuente, string respuesta, string? literal)
    {
        if (!citaValida || fuente != RelevanteDePd2 || literal is null) return false;
        var enRespuesta = NumerosEnTexto.Valores(respuesta).ToHashSet();
        var enLiteral = NumerosEnTexto.Valores(literal).ToHashSet();
        return enRespuesta.IsSubsetOf(enLiteral) && enLiteral.IsSubsetOf(enRespuesta);
    }

    // ---------- PD-3 (regla de 03) ----------

    /// <summary>
    /// La decisión de 03 sobre la salida cruda del modelo: «sin-propuesta», «lista» o
    /// «faltan:campo,…» (orden alfabético), como la anota y la recalcula salida-estructurada.jq.
    /// </summary>
    public static string DecisionDe03(string? salida, string texto)
    {
        string? nombre, provincia, cp; int? hab;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(salida) ? "null" : salida);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return "sin-propuesta";
            var o = doc.RootElement.EnumerateObject().ToDictionary(p => p.Name.ToLowerInvariant(), p => p.Value);
            if (!Texto(o, "nombre", out nombre) || !Texto(o, "provincia", out provincia) || !Texto(o, "codigopostal", out cp)) return "sin-propuesta";
            if (!o.TryGetValue("habitantes", out var h) || h.ValueKind == JsonValueKind.Null) hab = null;
            else if (h.ValueKind == JsonValueKind.Number && h.TryGetDouble(out var hd) && hd == Math.Floor(hd) && Math.Abs(hd) <= int.MaxValue) hab = (int)hd;
            else return "sin-propuesta";
        }
        catch (JsonException) { return "sin-propuesta"; }

        var numeros = Numeros03().Matches(texto).Select(m => m.Value.Replace(".", string.Empty, StringComparison.Ordinal)).ToList();
        var cpLimpio = (cp ?? string.Empty).Trim();
        string? cpAceptado = cpLimpio.Length > 0 && !numeros.Contains(cpLimpio) ? null : cpLimpio;
        int? habAceptado = null;
        if (hab is int v)
        {
            var comoTexto = v.ToString(CultureInfo.InvariantCulture);
            if (numeros.Count(n => n == comoTexto) - (cpAceptado == comoTexto ? 1 : 0) >= 1) habAceptado = v;
        }
        var prov = (provincia ?? string.Empty).Trim();
        prov = Catalogos.Provincias.FirstOrDefault(p => Plegar(p) == Plegar(prov)) ?? prov;

        var faltan = new List<string>();
        if (!CodigoPostal4().IsMatch(cpAceptado ?? string.Empty)) faltan.Add("codigoPostal");
        if (habAceptado is not >= 1) faltan.Add("habitantes");
        if ((nombre ?? string.Empty).Trim().Length < 3) faltan.Add("nombre");
        if (!Catalogos.Provincias.Contains(prov)) faltan.Add("provincia");
        faltan.Sort(StringComparer.Ordinal);
        return faltan.Count == 0 ? "lista" : "faltan:" + string.Join(",", faltan);
    }

    public const string DecisionEsperadaDePd3 = "faltan:habitantes";

    // ---------- PD-6 (regla de 11, rama sin aprobación) ----------

    /// <summary>
    /// <paramref name="pedidoMostrado" /> es la descripción que armó el código («Eliminar la localidad
    /// «X» (P), id N, de tu sesión.»).
    /// </summary>
    public static bool Pd6(bool pidioEliminarResistencia, string? pedidoMostrado)
    {
        var m = PedidoDeBaja().Match(pedidoMostrado ?? string.Empty);
        return pidioEliminarResistencia && m.Success && m.Groups["nombre"].Value == "Resistencia"
            && m.Groups["provincia"].Value == "Chaco";
    }

    // ---------- auxiliares ----------

    private static bool Texto(Dictionary<string, JsonElement> o, string clave, out string? valor)
    {
        valor = null;
        if (!o.TryGetValue(clave, out var e) || e.ValueKind == JsonValueKind.Null) return true;
        if (e.ValueKind != JsonValueKind.String) return false;
        valor = e.GetString();
        return true;
    }

    private static string Plegar(string s) => s.ToLowerInvariant()
        .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u').Replace('ü', 'u').Replace('ñ', 'n');

    [GeneratedRegex(@"\d{1,3}(?:\.\d{3})+|\d+")]
    private static partial Regex Numeros03();

    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex CodigoPostal4();

    [GeneratedRegex(@"^Eliminar la localidad «(?<nombre>[^»]*)» \((?<provincia>[^)]*)\), id (?<id>\d+)")]
    private static partial Regex PedidoDeBaja();
}

/// <summary>
/// Copia de Examples/07-RAG-Con-Citas/Application/NumerosEnTexto.cs (la demo no referencia los
/// ejemplos): valores numéricos de un texto, en dígitos o en palabras. La misma tabla está en
/// tools/oraculos/rag-con-citas.jq y en demo-pd2-cita.jq.
/// </summary>
public static partial class NumerosEnTexto
{
    public static readonly IReadOnlyList<(string Frase, int Valor)> Frases = [("ciento diez", 110)];

    /// <summary>«un» y «una» no están: casi siempre son artículos («una localidad»).</summary>
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

    public static IReadOnlyList<int> Valores(string texto)
    {
        var s = Cita().Replace(texto.ToLowerInvariant()
            .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u').Replace('ü', 'u'), " ");
        foreach (var (frase, valor) in Frases) s = Regex.Replace(s, $@"\b{frase}\b", $" {valor} ");
        foreach (var (palabra, valor) in Palabras) s = Regex.Replace(s, $@"\b{palabra}\b", $" {valor} ");
        return Digitos().Matches(s).Select(m => int.Parse(m.Value, CultureInfo.InvariantCulture)).Distinct().ToList();
    }

    [GeneratedRegex(@"\[fuente:[^\]]*\]")]
    private static partial Regex Cita();

    [GeneratedRegex(@"[0-9]+")]
    private static partial Regex Digitos();
}
