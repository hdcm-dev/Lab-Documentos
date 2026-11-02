// Nuevo: el paso 2 de la encuesta armado desde la propuesta, con los errores de ValidarPaso.
using MovilidadUrbana.Asistente.Application.Encuestas;

namespace MovilidadUrbana.Asistente.Application.Normalizacion;

/// <param name="Modelo">El paso 2 de la encuesta con las claves normalizadas (o el texto que no se pudo normalizar).</param>
/// <param name="Errores">Lo que dice <see cref="EncuestaService.ValidarPaso" />; vacío = el paso es válido.</param>
/// <param name="PropuestaValida">false si la respuesta del modelo no era el JSON pedido (TryGetResult = false).</param>
public sealed record ResultadoDeNormalizacion(EncuestaModel Modelo, IReadOnlyDictionary<string, string> Errores, bool PropuestaValida)
{
    public bool Valido => PropuestaValida && Errores.Count == 0;
}
