// Nuevo. Delta: Agregado por la IA — adaptador del puerto IExtractorDeEncuesta (PD-4; plan §F.7).
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Infrastructure.AI.Extraccion;

/// <summary>
/// Como <see cref="ExtractorDeLocalidad" />, para los pasos 2 y 3 de la encuesta. El prompt nombra
/// las claves del catálogo, pero la normalización no confía en que el modelo las use: pasa todo por
/// <see cref="NormalizadorDeCatalogo" />, y lo que no se puede llevar a una clave queda como vino
/// para que la regla lo rechace.
/// </summary>
public sealed class ExtractorDeEncuesta(IChatClient chat, NormalizadorDeCatalogo normalizador) : IExtractorDeEncuesta
{
    public static string Instrucciones => $"""
        Extraé de un relato cómo viaja la persona. Devolvé sólo lo que el relato dice; lo que no dice, null.
        medios: lista de medios mencionados (claves posibles: {string.Join(", ", Catalogos.Medios.Select(m => m.Clave))}).
        frecuencia: {string.Join(", ", Catalogos.Frecuencias.Select(f => f.Clave))}.
        motivo: {string.Join(", ", Catalogos.Motivos.Select(m => m.Clave))}.
        distancia en km y minutos sólo si el relato los dice; edad sólo si la dice.
        Si un medio no está en la lista, devolvelo igual, con la palabra del relato.
        """;

    public async Task<EncuestaPropuesta?> ExtraerAsync(string texto, CancellationToken cancellationToken = default)
    {
        var respuesta = await chat.GetResponseAsync<Cruda>(
            [new ChatMessage(ChatRole.User, texto)],
            new ChatOptions { Instructions = Instrucciones },
            cancellationToken: cancellationToken);
        if (!respuesta.TryGetResult(out var cruda) || cruda is null) return null;

        var medios = (cruda.Medios ?? [])
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Select(normalizador.Medio)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        return new EncuestaPropuesta(
            medios,
            normalizador.Frecuencia(cruda.Frecuencia),
            GarantiaNumerica.Real(cruda.Distancia, texto),
            GarantiaNumerica.Entero(cruda.Minutos, texto),
            normalizador.Motivo(cruda.Motivo),
            GarantiaNumerica.Entero(cruda.Edad, texto));
    }

    public sealed record Cruda(
        [property: JsonPropertyName("medios")] IReadOnlyList<string>? Medios,
        [property: JsonPropertyName("frecuencia")] string? Frecuencia,
        [property: JsonPropertyName("distancia")] double? Distancia,
        [property: JsonPropertyName("minutos")] int? Minutos,
        [property: JsonPropertyName("motivo")] string? Motivo,
        [property: JsonPropertyName("edad")] int? Edad);
}
