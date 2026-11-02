// Nuevo (plan §E.2, fila 07; guía §7.4). La GARANTÍA del ejemplo, en código: una respuesta se
// muestra sólo si toda cita apunta a un fragmento recuperado para esa pregunta, o si es el «No sé»
// pedido. Lo demás se rechaza. Por eso «siempre» vale: no depende de cómo responda el modelo.
using System.Text.RegularExpressions;

namespace MovilidadUrbana.Asistente.Application;

public enum Veredicto { Aceptada, NoSe, RechazadaCitaNoRecuperada, RechazadaSinCita }

/// <param name="Texto">Lo que devolvió el modelo, sin tocar.</param>
/// <param name="Citas">Ids citados, en orden, sin repetir.</param>
/// <param name="Mostrada">Lo que ve la persona: la respuesta, o el aviso de rechazo.</param>
public sealed record RespuestaConCitas(string Texto, IReadOnlyList<string> Citas, Veredicto Veredicto, string Mostrada)
{
    public bool CitaValida => Veredicto == Veredicto.Aceptada;
}

public static partial class ValidadorDeCitas
{
    public const string AvisoDeRechazo = "No pude verificar esta respuesta contra las reglas del sistema; no se muestra.";

    public static IReadOnlyList<string> Citas(string texto) =>
        Cita().Matches(texto).Select(m => m.Groups["id"].Value.Trim()).Distinct(StringComparer.Ordinal).ToList();

    /// <summary>«No sé» al principio, seguido de puntuación o del final (no «No se puede…»).</summary>
    public static bool EsNoSe(string texto) => NoSe().IsMatch(NumerosEnTexto.Normalizar(texto));

    public static RespuestaConCitas Validar(string texto, IReadOnlyCollection<string> recuperados)
    {
        var citas = Citas(texto);
        if (citas.Count == 0)
        {
            return EsNoSe(texto)
                ? new(texto, citas, Veredicto.NoSe, texto)
                : new(texto, citas, Veredicto.RechazadaSinCita, AvisoDeRechazo);
        }
        return citas.All(recuperados.Contains)
            ? new(texto, citas, Veredicto.Aceptada, texto)
            : new(texto, citas, Veredicto.RechazadaCitaNoRecuperada, AvisoDeRechazo);
    }

    [GeneratedRegex(@"\[\s*Fuente:\s*(?<id>[^\]]+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex Cita();

    [GeneratedRegex(@"^\s*no (lo )?se\s*([.!,;:]|$)")]
    private static partial Regex NoSe();
}
