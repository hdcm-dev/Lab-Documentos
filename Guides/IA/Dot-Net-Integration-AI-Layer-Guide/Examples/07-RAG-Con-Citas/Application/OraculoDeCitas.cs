// Nuevo (plan §E.2, fila 07; H-029). Oráculo «rag-con-citas», definido antes de correr. La misma
// regla, leída sólo del registro, está en tools/oraculos/rag-con-citas.jq.
//
//   · respondible con valor verificable (7): pasa si la cita es válida, cita un fragmento relevante
//     del oro, los valores del literal citado aparecen en la respuesta y no aparece otro valor;
//   · respondible sin valor verificable (5): pasa si la cita es válida (nada más se puede comprobar);
//   · no respondible (4): pasa si la respuesta es el «No sé» pedido, sin citas.
// «Cita y contradice» (sobre las 12 respondibles): cita válida y un valor que el literal citado no fija.
namespace MovilidadUrbana.Asistente.Application;

public sealed record EvaluacionDeCitas(string Resultado, bool CitaValida, bool CitaYContradice, bool NoSe, string? Motivo);

public static class OraculoDeCitas
{
    public const string Nombre = "rag-con-citas";

    public static EvaluacionDeCitas Evaluar(PreguntaDeOro pregunta, string salida, IReadOnlyList<Recuperacion> recuperados)
    {
        var validada = ValidadorDeCitas.Validar(salida, recuperados.Select(r => r.Fuente).ToHashSet(StringComparer.Ordinal));
        var citaValida = validada.Veredicto == Veredicto.Aceptada;
        var noSe = validada.Veredicto == Veredicto.NoSe;

        if (!pregunta.Respondible)
        {
            return new(noSe ? "pasa" : "falla", citaValida, false, noSe, noSe ? null : "no dijo «No sé»");
        }

        var valoresRespuesta = NumerosEnTexto.Valores(salida).ToHashSet();
        var citados = recuperados.Where(r => validada.Citas.Contains(r.Fuente)).ToList();
        var valoresCitados = citados.SelectMany(r => NumerosEnTexto.Valores(r.Texto)).ToHashSet();
        var contradice = citaValida && !valoresRespuesta.IsSubsetOf(valoresCitados);

        if (pregunta.SinValorVerificable)
        {
            return new(citaValida ? "pasa" : "falla", citaValida, contradice, noSe, citaValida ? null : $"cita {validada.Veredicto}");
        }

        var afirma = citaValida && !contradice && citados
            .Where(r => pregunta.Relevantes.Contains(r.Fuente))
            .Any(r => NumerosEnTexto.Valores(r.Texto).All(valoresRespuesta.Contains));
        string? motivo = afirma ? null
            : !citaValida ? $"cita {validada.Veredicto}"
            : contradice ? "cita y contradice"
            : "no afirma el valor del literal relevante citado";
        return new(afirma ? "pasa" : "falla", citaValida, contradice, noSe, motivo);
    }
}
