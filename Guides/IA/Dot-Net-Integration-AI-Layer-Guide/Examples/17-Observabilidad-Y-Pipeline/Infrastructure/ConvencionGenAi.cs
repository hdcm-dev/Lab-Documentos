// Nuevo (ejemplo 17; plan §E.2: «los nombres de métricas emitidos se listan contra la convención
// vigente (desfase de N5 V9)»). Las dos listas son de N5 §3.2 (V9, reconfirmado en R5 el 2026-10-26):
// la convención GenAI está en estado Development y se mudó de repositorio, así que estas listas
// envejecen. Se vuelven a verificar cuando se actualice M.E.AI o la convención.

namespace MovilidadUrbana.Asistente.Infrastructure;

public static class ConvencionGenAi
{
    /// <summary>
    /// Métricas de cliente GenAI de <c>semantic-conventions</c> v1.41.0, la última versión del repo
    /// anterior que traía GenAI (N5 V9). Son las que emite M.E.AI 10.11.0 (N5 E6).
    /// </summary>
    public static readonly string[] Semconv141 =
    [
        "gen_ai.client.operation.duration",
        "gen_ai.client.operation.time_per_output_chunk",
        "gen_ai.client.operation.time_to_first_chunk",
        "gen_ai.client.token.usage"
    ];

    /// <summary>
    /// Métricas de cliente de inferencia del repo nuevo <c>semantic-conventions-genai</c> (main al
    /// 2026-10-26, N5 V9 y R5). <c>gen_ai.client.token.usage</c> ya no existe. N5 nombra además
    /// variantes de uso para caché y razonamiento (<c>cache_read</c>, <c>cache_write</c>,
    /// <c>reasoning</c>) sin el nombre completo verificado: no se listan.
    /// </summary>
    public static readonly string[] Vigente =
    [
        "gen_ai.client.inference.duration",
        "gen_ai.client.inference.time_per_output_chunk",
        "gen_ai.client.inference.time_to_first_chunk",
        "gen_ai.client.inference.usage.input_tokens",
        "gen_ai.client.inference.usage.output_tokens",
        // Sigue definida, para operaciones genéricas (N5 V9).
        "gen_ai.client.operation.duration"
    ];

    /// <summary>Una fila por nombre emitido: si está en v1.41.0 y si está en la convención vigente.</summary>
    public static IReadOnlyList<(string Nombre, bool EnV141, bool EnVigente)> Comparar(IEnumerable<string> emitidos) =>
        emitidos.Order(StringComparer.Ordinal)
            .Select(n => (n, Semconv141.Contains(n), Vigente.Contains(n)))
            .ToList();
}
