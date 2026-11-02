// Nuevo: guion del modo falso de 15 (escrito para el ejemplo). Un «juez» guionado que responde con el
// formato que piden los evaluadores de Quality (<S0>…</S0><S1>…</S1><S2>puntaje</S2>), en el orden en
// que la corrida los llama: por caso, Relevance y después Groundedness. Trae a propósito:
//   · un desacuerdo con el oráculo determinista (s07-r07: Groundedness 2 sobre una salida que pasó en 07);
//   · una salida no interpretable (c10, Groundedness: «Score: 2», sin etiquetas).
// Con eso, el cableado sabe exactamente qué tiene que salir: 15/16, 9/10 y 51/52 interpretables.
// Después, Equivalence (REC-18, descriptiva) sobre las salidas de 07 respondibles, en el orden del oro:
// 5 si en 07 pasó, 2 si no, como entero solo (el formato de Equivalence, sin etiquetas). Todas
// interpretables: sólo prueban el cableado de la segunda pasada.
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun.Pruebas;

namespace MovilidadUrbana.Asistente;

public static class GuionDelJuez
{
    public const int AcuerdoConOraculoEsperado = 15;
    public const int AcuerdoConAnotadorEsperado = 9;
    public const int InterpretablesEsperadas = 51;

    public static string Salida(int puntaje, string explicacion) =>
        $"<S0>Let's think step by step: guion del modo falso.</S0><S1>{explicacion}</S1><S2>{puntaje}</S2>";

    /// <summary>(Relevance, Groundedness) por caso; null = salida sin etiquetas.</summary>
    public static (int? Relevancia, int? Fundamento) Puntajes(CasoDeEvaluacion caso) => caso.EntradaId switch
    {
        "s07-r07" => (5, 2),
        "c10" => (4, null),
        _ when caso.Grupo == GrupoDeEntrada.Calibracion =>
            (caso.Anotacion!.Relevante ? 5 : 2, caso.Anotacion.Fundamentada ? 5 : 2),
        _ when caso.Resultado07 == "pasa" => (5, 5),
        _ => (2, 4)
    };

    /// <summary>Equivalence guionada: 5 si la salida pasó en 07, 2 si no.</summary>
    public static int PuntajeDeEquivalencia(CasoDeEvaluacion caso) => caso.Resultado07 == "pasa" ? 5 : 2;

    public static GuionChatClient Crear(IEnumerable<CasoDeEvaluacion> casos)
    {
        var pasos = new List<PasoDelGuion>();
        foreach (var caso in casos)
        {
            var (r, g) = Puntajes(caso);
            pasos.Add(PasoDelGuion.Texto(r is int pr ? Salida(pr, "relevancia guionada") : "Score: 2"));
            pasos.Add(PasoDelGuion.Texto(g is int pg ? Salida(pg, "fundamento guionado") : "Score: 2"));
        }
        foreach (var caso in CorridaDeEquivalencia.Elegibles(casos))
            pasos.Add(PasoDelGuion.Texto(PuntajeDeEquivalencia(caso).ToString(System.Globalization.CultureInfo.InvariantCulture)));
        return new GuionChatClient(pasos, "Score: 2");
    }
}
