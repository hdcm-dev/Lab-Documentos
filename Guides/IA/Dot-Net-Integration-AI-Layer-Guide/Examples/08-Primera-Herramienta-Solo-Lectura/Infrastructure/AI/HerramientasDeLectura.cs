// Nuevo: las dos herramientas de SÓLO LECTURA del ejemplo 08 (§8.2–§8.3; H-011, PR-20). Envuelven el
// caso de uso (LocalidadService), nunca el repositorio, y se arman POR SESIÓN: la instancia ya trae
// el servicio de esa sesión, así que ninguna firma recibe un id de sesión que el modelo pueda elegir.
using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

/// <summary>Lo que ve el modelo de cada localidad: id, nombre y provincia. Ni CP ni habitantes.</summary>
public sealed record LocalidadDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("provincia")] string Provincia);

/// <summary>Resultado de contar: la cantidad, o el error que explica por qué no se contó.</summary>
public sealed record ConteoDto(
    [property: JsonPropertyName("provincia")] string Provincia,
    [property: JsonPropertyName("cantidad")] int? Cantidad,
    [property: JsonPropertyName("error")] string? Error);

public sealed class HerramientasDeLectura(LocalidadService localidades)
{
    public const string Listar = "listar_localidades";
    public const string Contar = "contar_localidades";

    [Description("Lista las localidades cargadas por la persona: id, nombre y provincia de cada una. No devuelve código postal ni habitantes.")]
    public async Task<IReadOnlyList<LocalidadDto>> ListarLocalidadesAsync(CancellationToken cancellationToken = default)
    {
        var todas = await localidades.GetAllAsync(cancellationToken);
        return [.. todas.Select(l => new LocalidadDto(l.Id, l.Nombre, l.Provincia))];
    }

    [Description("Cuenta cuántas localidades tiene cargadas la persona en una provincia del catálogo.")]
    public async Task<ConteoDto> ContarLocalidadesAsync(
        [Description("Nombre exacto de la provincia, uno de: Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe.")] string provincia,
        CancellationToken cancellationToken = default)
    {
        // Misma regla que LocalidadRules.ProvinciaValida: clave exacta del catálogo. Un texto fuera de
        // catálogo es un error con su motivo, nunca un «0» (que el modelo leería como dato).
        if (!Catalogos.Provincias.Contains(provincia))
        {
            return new ConteoDto(provincia, null,
                $"La provincia «{provincia}» no está en el catálogo ({string.Join(", ", Catalogos.Provincias)}).");
        }
        var todas = await localidades.GetAllAsync(cancellationToken);
        return new ConteoDto(provincia, todas.Count(l => l.Provincia == provincia), null);
    }

    /// <summary>Las herramientas montadas: sólo estas dos, las dos de lectura.</summary>
    public IList<AITool> Crear() =>
    [
        AIFunctionFactory.Create(ListarLocalidadesAsync, Listar),
        AIFunctionFactory.Create(ContarLocalidadesAsync, Contar)
    ];
}
