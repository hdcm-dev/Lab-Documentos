// Nuevo (ejemplo 09). Va DEBAJO de UseFunctionInvocation y encima de la capa única: ve cada llamada
// real al modelo del bucle de herramientas (la del pedido y la que sigue a la aprobación), para que el
// registro R-LLM tenga una línea por llamada y no una por petición.
using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

public sealed record LlamadaGrabada(IReadOnlyList<ChatMessage> Mensajes, ChatOptions? Opciones, ChatResponse Respuesta, TimeSpan Duracion);

public sealed class GrabadorDeLlamadas(IChatClient interno) : DelegatingChatClient(interno)
{
    public List<LlamadaGrabada> Llamadas { get; } = [];

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var enviados = messages.ToList();
        var reloj = Stopwatch.StartNew();
        var respuesta = await base.GetResponseAsync(enviados, options, cancellationToken);
        reloj.Stop();
        // Copias: FunctionInvokingChatClient reescribe después los contenidos de estos mensajes (cambia la
        // FunctionCallContent por el pedido de aprobación), y el registro tiene que ver lo que dijo el modelo.
        Llamadas.Add(new LlamadaGrabada([.. enviados.Select(Copiar)], options?.Clone(), new ChatResponse([.. respuesta.Messages.Select(Copiar)])
        {
            ModelId = respuesta.ModelId,
            ResponseId = respuesta.ResponseId,
            FinishReason = respuesta.FinishReason,
            Usage = respuesta.Usage,
            RawRepresentation = respuesta.RawRepresentation,
            AdditionalProperties = respuesta.AdditionalProperties
        }, reloj.Elapsed));
        return respuesta;
    }

    private static ChatMessage Copiar(ChatMessage m) =>
        new(m.Role, [.. m.Contents]) { AuthorName = m.AuthorName, MessageId = m.MessageId, RawRepresentation = m.RawRepresentation };
}
