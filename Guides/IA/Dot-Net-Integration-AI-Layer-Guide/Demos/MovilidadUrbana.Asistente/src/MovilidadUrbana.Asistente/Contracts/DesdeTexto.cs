// Nuevo. Delta: Agregado por la IA — contratos de /desde-texto (PD-3, PD-4).
namespace MovilidadUrbana.Asistente.Contracts;

/// <summary>Un pedido en lenguaje natural.</summary>
public sealed record TextoRequest(string? Texto);

/// <summary>La propuesta de alta: lo leído y lo que falta. No se guardó nada; la persona confirma con POST /api/v1/localidades.</summary>
public sealed record PropuestaDeAltaDto(bool Interpretado, LocalidadRequest? Propuesta, IReadOnlyDictionary<string, string> Faltantes, bool ListaParaConfirmar);

/// <summary>La propuesta de encuesta (pasos 2 y 3) y sus errores. No se registró nada.</summary>
public sealed record PropuestaDeEncuestaDto(
    bool Interpretado, IReadOnlyList<string> Medios, string? Frecuencia, double? Distancia, int? Minutos, string? Motivo, int? Edad,
    IReadOnlyDictionary<string, string> Errores);
