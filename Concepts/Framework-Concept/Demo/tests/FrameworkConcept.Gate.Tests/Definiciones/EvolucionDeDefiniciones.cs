using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Gate.Tests.Definiciones;

/// <summary>
/// P13.8 y P13.9 — Reglas de evolución entre dos versiones de una misma encuesta. JSON Schema no las puede
/// expresar (miran dos archivos a la vez), por eso viven acá.
///
///   - el encuestaId no cambia (un cambio destructivo es un encuestaId NUEVO, no una versión);
///   - la versión es estrictamente mayor (el roll-forward nunca la baja);
///   - toda pregunta publicada sigue existiendo, con el mismo id y el mismo tipo: su id es el nombre del
///     campo en las respuestas ya tomadas, que siguen llegando con la versión vieja (P13.2).
/// Agregar preguntas, corregir textos y aflojar restricciones, sí.
/// </summary>
public static class EvolucionDeDefiniciones
{
    /// <summary>Las violaciones al pasar de <paramref name="publicada"/> a <paramref name="nueva"/>; vacío si es una evolución válida.</summary>
    public static IReadOnlyList<string> Violaciones(DefinicionEncuesta publicada, DefinicionEncuesta nueva)
    {
        var violaciones = new List<string>();

        if (nueva.EncuestaId != publicada.EncuestaId)
            violaciones.Add($"cambió el encuestaId «{publicada.EncuestaId}» → «{nueva.EncuestaId}»");

        if (nueva.Version <= publicada.Version)
            violaciones.Add($"la versión no es estrictamente mayor ({publicada.Version} → {nueva.Version}) (P13.9)");

        var preguntasNuevas = nueva.Preguntas.ToDictionary(p => p.Id, StringComparer.Ordinal);
        foreach (var pregunta in publicada.Preguntas)
        {
            if (!preguntasNuevas.TryGetValue(pregunta.Id, out var ahora))
                violaciones.Add($"se borró la pregunta publicada «{pregunta.Id}» (P13.8)");
            else if (ahora.Tipo != pregunta.Tipo)
                violaciones.Add($"la pregunta publicada «{pregunta.Id}» cambió de tipo {pregunta.Tipo} → {ahora.Tipo} (P13.8)");
        }

        return violaciones;
    }
}
