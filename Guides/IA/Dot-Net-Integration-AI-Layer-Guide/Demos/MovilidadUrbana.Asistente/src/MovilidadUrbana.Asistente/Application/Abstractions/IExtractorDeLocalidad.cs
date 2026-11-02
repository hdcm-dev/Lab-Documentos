// Nuevo. Delta: Agregado por la IA — puerto de extracción de una localidad desde texto libre (plan §F.3, §F.7).
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Application.Abstractions;

/// <summary>
/// Puerto propio de Application: «de este texto, ¿qué localidad se quiere dar de alta?». El prompt,
/// el modelo y el proveedor son detalle del adaptador (ADR-01). Devuelve <c>null</c> cuando no hay
/// interpretación posible: el caso de uso cae al camino sin IA (el formulario).
/// </summary>
public interface IExtractorDeLocalidad
{
    Task<LocalidadPropuesta?> ExtraerAsync(string texto, CancellationToken cancellationToken = default);
}
