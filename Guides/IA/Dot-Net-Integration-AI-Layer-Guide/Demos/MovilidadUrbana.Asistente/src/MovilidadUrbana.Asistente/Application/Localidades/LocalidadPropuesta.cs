// Nuevo. Delta: Agregado por la IA — propuesta de alta con campos anulables (ADR-08; plan §F.7, H-004).
namespace MovilidadUrbana.Asistente.Application.Localidades;

/// <summary>
/// Lo que el adaptador leyó del texto. Todo campo que el pedido puede no traer es anulable, como
/// <see cref="LocalidadModel.Habitantes" />: un dato ausente llega como <c>null</c> y la regla lo
/// rechaza, en vez de llegar como un número inventado.
/// </summary>
public sealed record LocalidadPropuesta(string? Nombre, string? Provincia, string? CodigoPostal, int? Habitantes)
{
    public LocalidadModel ComoModelo() => new()
    {
        Nombre = Nombre ?? string.Empty,
        Provincia = Provincia ?? string.Empty,
        CodigoPostal = CodigoPostal ?? string.Empty,
        Habitantes = Habitantes
    };
}
