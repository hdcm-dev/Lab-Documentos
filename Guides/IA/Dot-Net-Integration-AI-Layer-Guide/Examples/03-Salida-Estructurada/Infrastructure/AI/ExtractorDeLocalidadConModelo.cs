// Nuevo (plan §E.2 fila 03, §F.7). Adaptador del puerto IExtractorDeLocalidad sobre IChatClient:
// GetResponseAsync<LocalidadPropuesta> (salida estructurada con esquema JSON) y TryGetResult. Las
// opciones del proveedor (razonamiento apagado, num_ctx, temperatura, semilla) NO se fijan acá:
// las pone la capa única que envuelve al cliente (FabricaDeClientes).
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Domain.Rules;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

/// <summary>Lo que hace falta para el registro R-LLM además de la extracción. <c>Cruda</c>: la propuesta antes de la garantía.</summary>
public sealed record DetalleDeExtraccion(
    ExtraccionDeLocalidad Extraccion, IReadOnlyList<ChatMessage> Enviados, ChatOptions Opciones, ChatResponse Respuesta, bool JsonValido, LocalidadPropuesta? Cruda);

public sealed class ExtractorDeLocalidadConModelo(IChatClient cliente) : IExtractorDeLocalidad
{
    public static readonly string Instrucciones = $"""
        Extraé de un pedido de alta de localidad estos campos y devolvé sólo el JSON pedido:
        nombre; provincia (una de: {string.Join(", ", Catalogos.Provincias)});
        codigoPostal ({LocalidadRules.DigitosDelCodigoPostal} dígitos, como texto); habitantes (número entero).
        Si un dato no está en el pedido, devolvé null en ese campo. No completes datos que el pedido no dice.
        """;

    public async Task<ExtraccionDeLocalidad> ExtraerAsync(string texto, CancellationToken cancellationToken = default) =>
        (await ExtraerConDetalleAsync(texto, cancellationToken)).Extraccion;

    public async Task<DetalleDeExtraccion> ExtraerConDetalleAsync(string texto, CancellationToken cancellationToken = default)
    {
        List<ChatMessage> mensajes = [new(ChatRole.User, texto)];
        var opciones = new ChatOptions { Instructions = Instrucciones };
        var respuesta = await cliente.GetResponseAsync<LocalidadPropuesta>(mensajes, opciones, cancellationToken: cancellationToken);

        // TryGetResult: si el texto no es un JSON del tipo pedido, false, sin excepción (N5 E3).
        if (!respuesta.TryGetResult(out var propuesta) || propuesta is null)
        {
            return new DetalleDeExtraccion(new ExtraccionDeLocalidad(null, []), mensajes, opciones, respuesta, false, null);
        }
        var (garantizada, descartados) = GarantiaDelTexto.Aplicar(propuesta, texto);
        return new DetalleDeExtraccion(new ExtraccionDeLocalidad(garantizada, descartados), mensajes, opciones, respuesta, true, propuesta);
    }
}
