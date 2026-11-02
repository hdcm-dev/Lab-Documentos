// Nuevo. Delta: Agregado por la IA — contratos de /ayuda (PD-2).
namespace MovilidadUrbana.Asistente.Contracts;

public sealed record PreguntaRequest(string? Pregunta);

public sealed record FragmentoDto(string Id, string Texto, double Puntaje);

/// <summary>La respuesta con su fuente. <c>CitaValida = false</c>: el texto es el fragmento más cercano, literal.</summary>
public sealed record RespuestaDeAyudaDto(string Texto, string? Fuente, bool CitaValida, IReadOnlyList<FragmentoDto> Recuperados, string PlanDeEmbeddings);
