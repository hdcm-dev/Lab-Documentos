// Nuevo. Delta: Agregado por la IA — propuesta de encuesta con campos anulables (ADR-08; plan §F.7, H-004).
namespace MovilidadUrbana.Asistente.Application.Encuestas;

/// <summary>
/// Pasos 2 y 3 de la encuesta leídos de un relato. Edad, distancia y minutos son anulables: si el
/// relato no los trae, llegan como <c>null</c>. Los medios, la frecuencia y el motivo llegan como
/// claves de <c>Catalogos</c> cuando el adaptador pudo normalizarlos, o tal como vinieron si no:
/// en ese caso la regla endurecida los rechaza (T2).
/// </summary>
public sealed record EncuestaPropuesta(
    IReadOnlyList<string> Medios,
    string? Frecuencia,
    double? Distancia,
    int? Minutos,
    string? Motivo,
    int? Edad);
