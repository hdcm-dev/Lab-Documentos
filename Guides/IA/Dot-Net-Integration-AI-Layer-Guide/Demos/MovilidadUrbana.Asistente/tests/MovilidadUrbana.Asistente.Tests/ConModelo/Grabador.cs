// Nuevo. Graba cada llamada que el host hace al cliente base (encima de la capa de opciones), para
// escribir el registro R-LLM con lo que se mandó y lo que volvió.
using System.Diagnostics;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun.AI;

namespace MovilidadUrbana.Asistente.Tests.ConModelo;

public sealed record LlamadaGrabada(IReadOnlyList<ChatMessage> Mensajes, ChatOptions Efectivas, ChatResponse Respuesta, TimeSpan Reloj);

public sealed class Grabador(IChatClient interno, ConfiguracionDelModelo config) : DelegatingChatClient(interno)
{
    public List<LlamadaGrabada> Llamadas { get; } = [];

    public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var lista = messages.ToList();
        // Las opciones efectivas son las que deja la capa única debajo: se reconstruyen con la misma función.
        var efectivas = options?.Clone() ?? new ChatOptions();
        OpcionesDelProveedor.Aplicar(efectivas, config, Adaptador.OllamaSharp);
        var reloj = Stopwatch.StartNew();
        var respuesta = await base.GetResponseAsync(lista, options, cancellationToken);
        // Instantánea: FunctionInvokingChatClient reescribe después los mensajes de esta misma respuesta
        // (cambia la llamada a una herramienta aprobable por el pedido de aprobación y, al decidir, suma
        // llamada y resultado a la respuesta siguiente). Sin copia, el registro atribuiría la herramienta
        // a otra llamada.
        var copia = new ChatResponse(respuesta.Messages.Select(m => new ChatMessage(m.Role, [.. m.Contents]) { AuthorName = m.AuthorName, MessageId = m.MessageId }).ToList())
        {
            ResponseId = respuesta.ResponseId, ConversationId = respuesta.ConversationId, ModelId = respuesta.ModelId,
            CreatedAt = respuesta.CreatedAt, FinishReason = respuesta.FinishReason, Usage = respuesta.Usage,
            RawRepresentation = respuesta.RawRepresentation, AdditionalProperties = respuesta.AdditionalProperties
        };
        Llamadas.Add(new LlamadaGrabada(lista.ToList(), efectivas, copia, reloj.Elapsed));
        return respuesta;
    }
}
