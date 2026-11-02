// Nuevo: anota cada llamada al modelo que hace el bucle de herramientas. Va DEBAJO de
// UseFunctionInvocation y ENCIMA de la capa única de opciones: así una pregunta de la persona
// deja una línea R-LLM por llamada al modelo, no una por pregunta.
using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

/// <summary>Una llamada al modelo dentro del bucle: lo que se mandó, con qué opciones y qué volvió.</summary>
public sealed record LlamadaGrabada(IReadOnlyList<ChatMessage> Mensajes, ChatOptions? Opciones, ChatResponse Respuesta, TimeSpan Reloj);

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
        // Copia: UseFunctionInvocation reusa la última respuesta y le agrega los mensajes de todo el
        // bucle; la línea de esta llamada tiene que quedar con lo que volvió en ESTA llamada.
        var copia = new ChatResponse([.. respuesta.Messages])
        {
            ModelId = respuesta.ModelId,
            FinishReason = respuesta.FinishReason,
            Usage = respuesta.Usage,
            RawRepresentation = respuesta.RawRepresentation,
            AdditionalProperties = respuesta.AdditionalProperties
        };
        Llamadas.Add(new LlamadaGrabada(enviados, options?.Clone(), copia, reloj.Elapsed));
        return respuesta;
    }
}
