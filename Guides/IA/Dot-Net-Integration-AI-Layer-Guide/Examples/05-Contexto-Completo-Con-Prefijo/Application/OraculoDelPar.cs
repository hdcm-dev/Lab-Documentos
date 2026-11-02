// Nuevo (plan §E.2 y §10.4). Oráculo «par-prefijo», el mismo que tools/oraculos/par-prefijo.jq:
// decide sobre DURACIONES DEL SERVIDOR (prompt_eval_duration_ms), nunca de reloj (H3-021). La
// calidad de la respuesta no se mide (X-008): el oráculo no lee la salida.
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Application;

public sealed record VeredictoDelPar(string Resultado, string? Motivo, double? Razon);

public static class OraculoDelPar
{
    public const string Nombre = "par-prefijo";

    /// <summary>Un par es «a favor» si con-prefijo tarda menos de la mitad que sin-prefijo (§E.2).</summary>
    public const double UmbralAFavor = 0.5;

    public static VeredictoDelPar Evaluar(IReadOnlyCollection<RegistroDeLlamada> llamadas)
    {
        if (llamadas.Count == 0) return new(ResultadoDeEntrada.Excluida, "plazo", null);
        if (llamadas.Any(l => l.TruncadoPosible == true)) return new(ResultadoDeEntrada.Excluida, "truncado_posible", null);
        var sin = llamadas.FirstOrDefault(l => l.Paso == Pasos.SinPrefijo);
        var con = llamadas.FirstOrDefault(l => l.Paso == Pasos.ConPrefijo);
        if (sin is null || con is null) return new(ResultadoDeEntrada.Excluida, "plazo", null);
        if (sin.PromptEvalDurationMs is not > 0 || con.PromptEvalDurationMs is null)
        {
            return new(ResultadoDeEntrada.Excluida, "sin duraciones del servidor", null);
        }
        var razon = con.PromptEvalDurationMs.Value / sin.PromptEvalDurationMs.Value;
        var aFavor = razon < UmbralAFavor;
        // La vuelta a frío NO tiene que aprovechar la caché: si también saliera rápida, lo que se midió
        // no es el prefijo (p. ej. el servidor cachea otra cosa) y el par falla.
        var vuelta = llamadas.FirstOrDefault(l => l.Paso == Pasos.VueltaAFrio);
        if (vuelta is not null)
        {
            if (vuelta.PromptEvalDurationMs is null) return new(ResultadoDeEntrada.Excluida, "sin duraciones del servidor", razon);
            if (vuelta.PromptEvalDurationMs.Value < UmbralAFavor * sin.PromptEvalDurationMs.Value)
            {
                return new(ResultadoDeEntrada.Falla, "la vuelta a frío salió cacheada", razon);
            }
        }
        return aFavor ? new(ResultadoDeEntrada.Pasa, null, razon) : new(ResultadoDeEntrada.Falla, "con-prefijo no bajó de la mitad", razon);
    }
}
