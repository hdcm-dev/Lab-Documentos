// Nuevo (plan §E.1: «una prueba de cableado con un cliente espía comprueba que el 100 % de las
// llamadas … salen con ReasoningEffort.None»). Se pone DEBAJO de la capa de opciones, pegado al
// adaptador, para ver lo que de verdad saldría hacia el servidor.
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Comun.Pruebas;

public sealed class ClienteEspia(IChatClient interno) : DelegatingChatClient(interno)
{
    public List<ChatOptions?> OpcionesVistas { get; } = [];

    public override Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        OpcionesVistas.Add(options?.Clone());
        return base.GetResponseAsync(messages, options, cancellationToken);
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        OpcionesVistas.Add(options?.Clone());
        await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            yield return update;
        }
    }
}
