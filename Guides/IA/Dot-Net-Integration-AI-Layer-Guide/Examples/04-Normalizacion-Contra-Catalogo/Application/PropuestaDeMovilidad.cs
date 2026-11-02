// Nuevo: lo que el modelo propone, en las palabras de la persona. Es una PROPUESTA: no son claves
// de Catalogos ni se guarda así; la normaliza el adaptador y la decide ValidarPaso (ADR-08, §5.3).
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente.Application.Normalizacion;

public sealed record PropuestaDeMovilidad
{
    /// <summary>
    /// Medios tal como los dijo la persona («bondi», «bici»…). Ojo: si el modelo escribe
    /// «"medios": null», System.Text.Json deja null aunque el tipo diga lo contrario (el «= []» sólo
    /// rige cuando la propiedad falta). El tipo no se cambia para no cambiar el esquema que se le pide
    /// al modelo; quien lo lee (<see cref="NormalizacionDeMovilidad" />) lo trata como lista vacía.
    /// </summary>
    [JsonPropertyName("medios")] public List<string> Medios { get; init; } = [];

    /// <summary>Frecuencia tal como la dijo la persona; null si no la dijo.</summary>
    [JsonPropertyName("frecuencia")] public string? Frecuencia { get; init; }
}
