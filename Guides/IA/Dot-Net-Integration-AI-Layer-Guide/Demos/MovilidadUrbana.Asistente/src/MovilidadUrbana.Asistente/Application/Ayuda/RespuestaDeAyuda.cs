// Nuevo. Delta: Agregado por la IA — respuesta de la ayuda con su cita (PD-2; plan §F.6).
namespace MovilidadUrbana.Asistente.Application.Ayuda;

/// <summary>Un fragmento del corpus que la búsqueda devolvió, con su puntaje de similitud.</summary>
/// <param name="Id">Id calificado por clase, p. ej. <c>LocalidadPolicy.RequisitoDelCodigoPostal</c>.</param>
public sealed record FragmentoRecuperado(string Id, string Texto, double Puntaje);

/// <summary>
/// La respuesta de la ayuda. <see cref="CitaValida" /> es <c>true</c> sólo si la fuente que propuso
/// el modelo está entre los fragmentos recuperados; si no, <see cref="Texto" /> es el fragmento más
/// cercano, literal, y la UI lo dice. <see cref="PlanDeEmbeddings" /> va a la UI y al registro: con
/// EMB-C la recuperación «no se ejecutó contra un modelo de embeddings» (H2-017).
/// </summary>
public sealed record RespuestaDeAyuda(
    string Texto,
    string? Fuente,
    bool CitaValida,
    IReadOnlyList<FragmentoRecuperado> Recuperados,
    string PlanDeEmbeddings);
