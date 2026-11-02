// Nuevo. Delta: Agregado por la IA — puerto de la ayuda con recuperación (PD-2; plan §F.6).
using MovilidadUrbana.Asistente.Application.Ayuda;

namespace MovilidadUrbana.Asistente.Application.Abstractions;

/// <summary>
/// Responde preguntas sobre los requisitos del alta y de la encuesta, citando el requisito del que
/// sale la respuesta. Lo usa <c>AyudaController</c> directamente, sin caso de uso en el medio: es la
/// excepción de lectura declarada (§A.4), porque no escribe ni lee datos de la sesión.
/// </summary>
public interface IAyuda
{
    Task<RespuestaDeAyuda> ResponderAsync(string pregunta, CancellationToken cancellationToken = default);
}
