// Nuevo (ejemplo 11; mismo patrón que el grabador de 09). Va DEBAJO del bucle de herramientas del
// agente y encima de la capa única: ve cada llamada real al modelo, para que el registro R-LLM tenga
// una línea por llamada, y anota en qué petición (1 o 2) ocurrió.
using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

public sealed record LlamadaGrabada(string Peticion, IReadOnlyList<ChatMessage> Mensajes, ChatOptions? Opciones, ChatResponse Respuesta, TimeSpan Duracion);

public sealed class GrabadorDeLlamadas(IChatClient interno) : DelegatingChatClient(interno)
{
    public List<LlamadaGrabada> Llamadas { get; } = [];

    /// <summary>Rótulo de la petición en curso («peticion-1», «peticion-2»…); lo pone la raíz de composición.</summary>
    public string Peticion { get; set; } = "peticion-1";

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var enviados = messages.ToList();
        var reloj = Stopwatch.StartNew();
        var respuesta = await base.GetResponseAsync(enviados, options, cancellationToken);
        reloj.Stop();
        // Copias: FunctionInvokingChatClient reescribe después estos contenidos (cambia la
        // FunctionCallContent por el pedido de aprobación) y el registro tiene que ver lo que dijo el modelo.
        Llamadas.Add(new LlamadaGrabada(Peticion, [.. enviados.Select(Copiar)], options?.Clone(), new ChatResponse([.. respuesta.Messages.Select(Copiar)])
        {
            ModelId = respuesta.ModelId,
            ResponseId = respuesta.ResponseId,
            FinishReason = respuesta.FinishReason,
            // Copia, no referencia: FunctionInvokingChatClient suma el uso de cada vuelta del bucle EN el
            // UsageDetails de la primera respuesta, y la línea de la llamada 1 terminaría con la suma.
            Usage = respuesta.Usage is { } u ? new UsageDetails
            {
                InputTokenCount = u.InputTokenCount,
                OutputTokenCount = u.OutputTokenCount,
                TotalTokenCount = u.TotalTokenCount,
                AdditionalCounts = u.AdditionalCounts is null ? null : new(u.AdditionalCounts)
            } : null,
            RawRepresentation = respuesta.RawRepresentation,
            AdditionalProperties = respuesta.AdditionalProperties
        }, reloj.Elapsed));
        return respuesta;
    }

    private static ChatMessage Copiar(ChatMessage m) =>
        new(m.Role, [.. m.Contents]) { AuthorName = m.AuthorName, MessageId = m.MessageId, RawRepresentation = m.RawRepresentation };
}
