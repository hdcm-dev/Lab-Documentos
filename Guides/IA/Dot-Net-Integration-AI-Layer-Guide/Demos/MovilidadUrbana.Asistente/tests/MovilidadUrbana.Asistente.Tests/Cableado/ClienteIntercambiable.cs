// Nuevo. Cliente guionado al que cada prueba le cambia el guion sin rearmar el host.
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

public sealed class ClienteIntercambiable : IChatClient
{
    public GuionChatClient Actual { get; private set; } = new();

    public GuionChatClient Poner(params PasoDelGuion[] pasos) => Actual = new GuionChatClient(pasos, "Listo.");

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        Actual.GetResponseAsync(messages, options, cancellationToken);

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        Actual.GetStreamingResponseAsync(messages, options, cancellationToken);

    public object? GetService(Type serviceType, object? serviceKey = null) => Actual.GetService(serviceType, serviceKey);

    public void Dispose() { }
}
