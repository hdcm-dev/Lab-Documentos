using System.Text.Json;

namespace FrameworkConcept.Core.Contracts.Encuestas;

/// <summary>
/// Lo que el outbox envía al receptor por cada respuesta (P5, P13).
///
/// — <see cref="ClaveIdempotencia"/>: única por respuesta; el receptor deduplica por ella, así reenviar no duplica.
/// — <see cref="EncuestaId"/> + <see cref="VersionDefinicion"/>: la definición con la que se tomó (P13.2). El
///   receptor acepta versiones publicadas y no retiradas, no sólo la vigente (P13.4).
/// — <see cref="VersionContenido"/>: para atribuir un error a versiones exactas (folio 023, ítem 4). Retirar una
///   versión de contenido NO rechaza las respuestas tomadas con ella (P13.10).
/// La concurrencia (versión + 412 con If-Match, D-04) va en los encabezados HTTP, no en este cuerpo.
/// </summary>
public sealed record EnvioDeRespuesta(
    string ClaveIdempotencia,
    string EncuestaId,
    int VersionDefinicion,
    VersionSemantica VersionContenido,
    DateTimeOffset RegistradaEn,
    IReadOnlyDictionary<string, JsonElement> Valores)
{
    public ReferenciaDeDefinicion Definicion => new(EncuestaId, VersionDefinicion);
}
