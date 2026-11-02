// Nuevo. Delta: Agregado por la IA — adaptador del puerto IExtractorDeLocalidad (PD-3; plan §F.7).
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Infrastructure.AI.Extraccion;

/// <summary>
/// Salida estructurada con <c>GetResponseAsync&lt;T&gt;</c> sobre el cliente base, que ya trae las
/// opciones del proveedor de la capa única (razonamiento apagado, <c>num_ctx</c>, temperatura 0).
/// Consume con <c>TryGetResult</c>: si no hay resultado, devuelve <c>null</c> y el caso de uso cae
/// al formulario. Normaliza la provincia y aplica la garantía numérica antes de entregar.
/// </summary>
public sealed class ExtractorDeLocalidad(IChatClient chat, NormalizadorDeCatalogo normalizador) : IExtractorDeLocalidad
{
    public const string Instrucciones = """
        Extraé los datos de una localidad del pedido de la persona. Devolvé sólo lo que el pedido dice.
        Si un dato no está en el pedido, devolvelo como null: no lo estimes ni lo completes.
        provincia: el nombre de la provincia tal como aparece. codigoPostal: los dígitos tal como aparecen.
        habitantes: un número entero sólo si el pedido lo dice.
        """;

    public async Task<LocalidadPropuesta?> ExtraerAsync(string texto, CancellationToken cancellationToken = default)
    {
        var respuesta = await chat.GetResponseAsync<Cruda>(
            [new ChatMessage(ChatRole.User, texto)],
            new ChatOptions { Instructions = Instrucciones },
            cancellationToken: cancellationToken);
        if (!respuesta.TryGetResult(out var cruda) || cruda is null) return null;

        return new LocalidadPropuesta(
            string.IsNullOrWhiteSpace(cruda.Nombre) ? null : cruda.Nombre.Trim(),
            normalizador.Provincia(cruda.Provincia),
            string.IsNullOrWhiteSpace(cruda.CodigoPostal) ? null : cruda.CodigoPostal.Trim(),
            GarantiaNumerica.Entero(cruda.Habitantes, texto));
    }

    /// <summary>Lo que el modelo devuelve, antes de normalizar. Los nombres JSON son el contrato con el prompt.</summary>
    public sealed record Cruda(
        [property: JsonPropertyName("nombre")] string? Nombre,
        [property: JsonPropertyName("provincia")] string? Provincia,
        [property: JsonPropertyName("codigoPostal")] string? CodigoPostal,
        [property: JsonPropertyName("habitantes")] int? Habitantes);
}
