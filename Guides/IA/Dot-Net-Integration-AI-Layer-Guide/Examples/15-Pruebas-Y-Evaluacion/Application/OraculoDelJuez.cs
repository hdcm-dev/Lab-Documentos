// Nuevo (plan §E.2, fila 15; guía §10.6). Oráculo «acuerdo-del-juez», definido antes de juzgar. La
// MISMA regla, leída sólo del registro R-LLM, está en tools/oraculos/acuerdo-del-juez.jq.
//
//   · Lectura del juez: el puntaje es el entero entre <S2> y </S2> de la salida (el formato que piden
//     RelevanceEvaluator y GroundednessEvaluator de M.E.AI.Evaluation.Quality 10.11.0). Sin ese
//     entero, la salida «no es interpretable».
//   · «alta» = puntaje ≥ umbral (4, fijado en calibracion.v2.json antes de juzgar).
//   · calibración: pasa si las dos lecturas son interpretables y cada una coincide con la anotación
//     (relevante ↔ Relevance alta; fundamentada ↔ Groundedness alta);
//   · salidas de 07: pasa si las dos son interpretables y «las dos altas» coincide con lo que dijo el
//     oráculo determinista de 07 (pasa ↔ aceptable para el juez).
// Una salida no interpretable es una falla del juez, no una exclusión. Se excluye por truncado,
// plazo, error de llamada o si no hubo las dos llamadas.
using System.Globalization;
using System.Text.RegularExpressions;

namespace MovilidadUrbana.Asistente.Application;

public static partial class LecturaDelJuez
{
    /// <summary>El puntaje entre &lt;S2&gt; y &lt;/S2&gt;, o null si no hay un entero ahí.</summary>
    public static int? Puntaje(string? salida)
    {
        if (salida is null) return null;
        var m = S2().Match(salida);
        return m.Success && int.TryParse(m.Groups["v"].Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) ? p : null;
    }

    [GeneratedRegex(@"<S2>(?<v>[\s\S]*?)</S2>")]
    private static partial Regex S2();
}

/// <summary>Una lectura del juez sobre una entrada: qué evaluador, qué puntaje y qué se esperaba.</summary>
public sealed record LecturaDeEvaluador(string Evaluador, int? Puntaje, string Esperado)
{
    public bool Interpretable => Puntaje is not null;
}

public sealed record VeredictoDelJuez(string Resultado, string? Motivo, bool? Aceptable);

public static class OraculoDelJuez
{
    public const string Nombre = "acuerdo-del-juez";

    public static VeredictoDelJuez Evaluar(string grupo, IReadOnlyList<LecturaDeEvaluador> lecturas, int umbral)
    {
        if (lecturas.Count != 2) return new(Registro.Excluida, $"se esperaban 2 llamadas al juez y hubo {lecturas.Count}", null);
        if (lecturas.Any(l => !l.Interpretable))
        {
            var cuales = string.Join(", ", lecturas.Where(l => !l.Interpretable).Select(l => l.Evaluador));
            return new(Registro.Falla, $"salida del juez no interpretable ({cuales})", null);
        }
        bool Alta(LecturaDeEvaluador l) => l.Puntaje >= umbral;
        var aceptable = lecturas.All(Alta);
        if (grupo == GrupoDeEntrada.Calibracion)
        {
            var desacuerdos = lecturas.Where(l => Alta(l) != (l.Esperado == "alta")).Select(l => $"{l.Evaluador} {l.Puntaje} (anotado {l.Esperado})").ToList();
            return desacuerdos.Count == 0
                ? new(Registro.Pasa, null, aceptable)
                : new(Registro.Falla, $"desacuerdo con el anotador: {string.Join("; ", desacuerdos)}", aceptable);
        }
        var esperado = lecturas[0].Esperado == "pasa";
        return aceptable == esperado
            ? new(Registro.Pasa, null, aceptable)
            : new(Registro.Falla, $"juez {(aceptable ? "aceptable" : "no aceptable")}, oráculo determinista {lecturas[0].Esperado}", aceptable);
    }

    private static class Registro
    {
        public const string Pasa = "pasa";
        public const string Falla = "falla";
        public const string Excluida = "excluida";
    }
}
