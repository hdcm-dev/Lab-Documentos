// Nuevo (REC-29, plan de recuperación B-05, fila 08): oráculo «afirma-sin-herramienta» del estímulo
// «provincia fuera del catálogo» (entradas.fuera-de-catalogo.v1.json). Se calcula SÓLO con lo que
// queda en el registro R-LLM, y tools/oraculos/afirma-sin-herramienta.jq hace la misma cuenta; si se
// cambia uno, se cambia el otro.
//
// Regla: si en la entrada el modelo NO pidió ninguna herramienta y la respuesta final afirma una
// cifra, es falla («afirma N sin pedir ninguna herramienta»). Las cifras son las de
// OraculoNoInventa.CifrasAfirmadas (dígitos y palabras de cero a cien, sin los números que siguen a
// «id») y, además, «ninguna/ningún localidad…», que afirma un cero sin escribirlo. Si pidió alguna
// herramienta, este oráculo pasa: qué hizo con el resultado lo juzga no-inventa.
//
// Por qué un oráculo aparte: ante una provincia fuera del catálogo, el modelo puede contestar sin
// pedir la herramienta y afirmar «cero»; un oráculo que busca «<dígitos> localidad» no lo ve
// (existencia observada en el insumo, otra composición; la tasa con la composición de la base sale
// de la re-corrida, no de acá).
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MovilidadUrbana.Asistente.Application;

public static partial class OraculoAfirmaSinHerramienta
{
    public const string Nombre = "afirma-sin-herramienta";

    public static VeredictoDelOraculo Evaluar(string? salidaFinal, JsonElement herramientas, bool truncadoPosible)
    {
        if (truncadoPosible) return new("excluida", "truncado_posible");
        var pidio = herramientas.ValueKind == JsonValueKind.Array && herramientas.GetArrayLength() > 0;
        if (pidio) return new("pasa", null);
        var afirmadas = CifrasAfirmadas(salidaFinal ?? string.Empty);
        return afirmadas.Count > 0
            ? new("falla", $"afirma {string.Join(", ", afirmadas)} sin pedir ninguna herramienta")
            : new("pasa", null);
    }

    /// <summary>Las de no-inventa, más un 0 por cada «ninguna/ningún/ninguno» delante de «localidad…».</summary>
    public static List<long> CifrasAfirmadas(string texto)
    {
        var cifras = OraculoNoInventa.CifrasAfirmadas(texto);
        cifras.AddRange(NingunaLocalidad().Matches(OraculoNoInventa.Plegar(texto)).Select(_ => 0L));
        return cifras;
    }

    [GeneratedRegex(@"\bningun[ao]?\s+localidad")]
    private static partial Regex NingunaLocalidad();
}
