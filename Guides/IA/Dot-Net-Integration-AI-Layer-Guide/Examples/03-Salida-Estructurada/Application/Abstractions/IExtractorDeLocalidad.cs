// Nuevo (plan §F.7). Puerto de la aplicación: «de un texto libre, sacame una propuesta de
// localidad». La aplicación no sabe que detrás hay un modelo; sólo recibe la propuesta (o nada).
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Application.Abstractions;

public interface IExtractorDeLocalidad
{
    Task<ExtraccionDeLocalidad> ExtraerAsync(string texto, CancellationToken cancellationToken = default);
}

/// <param name="Propuesta">null si la respuesta no fue un JSON del tipo pedido (TryGetResult = false).</param>
/// <param name="Descartados">Campos que el modelo propuso con un valor que NO está en el texto: se tratan como ausentes.</param>
public sealed record ExtraccionDeLocalidad(LocalidadPropuesta? Propuesta, IReadOnlyList<string> Descartados);
