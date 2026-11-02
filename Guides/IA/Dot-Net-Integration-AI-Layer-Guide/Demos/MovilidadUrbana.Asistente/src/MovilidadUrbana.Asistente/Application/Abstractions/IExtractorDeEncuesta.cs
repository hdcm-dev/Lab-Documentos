// Nuevo. Delta: Agregado por la IA — puerto de extracción de una respuesta de encuesta desde texto libre (plan §F.3, §F.7).
using MovilidadUrbana.Asistente.Application.Encuestas;

namespace MovilidadUrbana.Asistente.Application.Abstractions;

/// <summary>
/// Puerto propio de Application: «de este relato, ¿qué medios, frecuencia, distancia, minutos y
/// motivo se pueden leer?». La propuesta llega ya normalizada contra <c>Catalogos</c> por el
/// adaptador; las reglas las aplica el caso de uso. <c>null</c>: sin interpretación.
/// </summary>
public interface IExtractorDeEncuesta
{
    Task<EncuestaPropuesta?> ExtraerAsync(string texto, CancellationToken cancellationToken = default);
}
