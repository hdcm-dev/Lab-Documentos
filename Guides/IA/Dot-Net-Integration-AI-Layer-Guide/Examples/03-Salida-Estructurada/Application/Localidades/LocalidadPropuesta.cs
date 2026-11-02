// Nuevo (plan §E.2 fila 03, §F.7; ADR-08). Lo que el modelo PROPONE para un alta de localidad.
// Todo campo que el pedido puede no traer es anulable: así el modelo tiene un lugar para decir
// «no está» (Habitantes es int?, como LocalidadModel.Habitantes del caso base). Anulable no impide
// inventar: eso lo cuida GarantiaDelTexto (H2-032).
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente.Application.Localidades;

public sealed record LocalidadPropuesta
{
    [JsonPropertyName("nombre")] public string? Nombre { get; init; }

    [JsonPropertyName("provincia")] public string? Provincia { get; init; }

    [JsonPropertyName("codigoPostal")] public string? CodigoPostal { get; init; }

    [JsonPropertyName("habitantes")] public int? Habitantes { get; init; }
}
