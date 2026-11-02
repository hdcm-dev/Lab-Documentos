// Nuevo (ejemplo 01, variante negativa del plan §E.2: «sin la capa de opciones y con presupuesto
// chico, Text vacío»; reproduce Sondeo §2). Es un cliente FALSO que imita la conducta medida de
// qwen3.5:4b en el laboratorio: si nadie apaga el razonamiento, el modelo gasta el presupuesto de
// salida pensando y devuelve contenido vacío con finish = length. No llama a ningún servidor.
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure;

public sealed class ModeloQueRazonaPorDefecto(string respuestaSinRazonar) : IChatClient
{
    /// <summary>Tokens que «gasta» pensando antes de escribir la respuesta (Sondeo §2: más de 200).</summary>
    public const int TokensDeRazonamiento = 400;

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var razonaApagado = options?.Reasoning?.Effort == ReasoningEffort.None;
        var tope = options?.MaxOutputTokens ?? int.MaxValue;
        if (!razonaApagado && tope <= TokensDeRazonamiento)
        {
            var pensado = new ChatMessage(ChatRole.Assistant, [new TextReasoningContent("Pienso qué campos pide el formulario…")]);
            return Task.FromResult(new ChatResponse(pensado)
            {
                FinishReason = ChatFinishReason.Length,
                ModelId = "simulado",
                Usage = new UsageDetails { OutputTokenCount = tope }
            });
        }
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, respuestaSinRazonar))
        {
            FinishReason = ChatFinishReason.Stop,
            ModelId = "simulado"
        });
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var respuesta = await GetResponseAsync(messages, options, cancellationToken);
        foreach (var update in respuesta.ToChatResponseUpdates()) yield return update;
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }
}
