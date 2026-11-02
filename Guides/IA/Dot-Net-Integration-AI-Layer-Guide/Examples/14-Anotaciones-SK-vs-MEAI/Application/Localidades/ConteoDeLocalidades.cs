// Nuevo (ejemplo 14). El cuerpo de contar_localidades, UNA vez: las dos anotaciones (SK y M.E.AI)
// delegan acá en una línea, así que lo único que cambia entre HerramientaMeai.cs y PluginSk.cs es cómo
// se anota el método. Misma regla que la herramienta de 08 (LocalidadRules.ProvinciaValida: clave
// exacta del catálogo; fuera de catálogo es un error con motivo, nunca un «0»).
using System.Text.Json.Serialization;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Application.Localidades;

/// <summary>Resultado de contar: la cantidad, o el error que explica por qué no se contó.</summary>
public sealed record ConteoDto(
    [property: JsonPropertyName("provincia")] string Provincia,
    [property: JsonPropertyName("cantidad")] int? Cantidad,
    [property: JsonPropertyName("error")] string? Error);

public static class ConteoDeLocalidades
{
    public static async Task<ConteoDto> ContarAsync(LocalidadService localidades, string provincia, CancellationToken cancellationToken = default)
    {
        if (!Catalogos.Provincias.Contains(provincia))
        {
            return new ConteoDto(provincia, null,
                $"La provincia «{provincia}» no está en el catálogo ({string.Join(", ", Catalogos.Provincias)}).");
        }
        var todas = await localidades.GetAllAsync(cancellationToken);
        return new ConteoDto(provincia, todas.Count(l => l.Provincia == provincia), null);
    }
}
