// Nuevo. Patrón tomado de N6-Evidencia/sonda-Program.cs (N6 §4.6), ampliado: el guion admite
// respuestas de texto, llamadas a herramientas y streaming por fragmentos, y anota cada llamada
// (mensajes y opciones) para que las pruebas de cableado comprueben qué salió hacia el «modelo».
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Comun.Pruebas;

/// <summary>Un paso del guion: lo que el «modelo» responde a la llamada n.</summary>
public sealed record PasoDelGuion(IReadOnlyList<AIContent> Contenidos, IReadOnlyList<string>? Fragmentos = null)
{
    public static PasoDelGuion Texto(string texto) => new([new TextContent(texto)]);

    /// <summary>Texto que en streaming llega partido en estos fragmentos (concatenados dan el texto).</summary>
    public static PasoDelGuion TextoEnFragmentos(params string[] fragmentos) =>
        new([new TextContent(string.Concat(fragmentos))], fragmentos);

    public static PasoDelGuion Herramienta(string callId, string nombre, IDictionary<string, object?>? argumentos = null) =>
        new([new FunctionCallContent(callId, nombre, argumentos)]);
}

/// <summary>Una llamada que recibió el cliente guionado, tal como llegó.</summary>
public sealed record LlamadaRecibida(IReadOnlyList<ChatMessage> Mensajes, ChatOptions? Opciones);

/// <summary>
/// Cliente falso del modo <c>--fake</c>: responde según el guion, en orden. Cuando el guion se
/// agota, responde el texto final (o «Listo.»). Informa uso de tokens con la estimación de la
/// guarda, para que el registro tenga los mismos campos que con un modelo.
/// </summary>
public sealed class GuionChatClient : IChatClient
{
    private readonly Queue<PasoDelGuion> _pasos;
    private readonly string _textoFinal;

    public GuionChatClient(IEnumerable<PasoDelGuion> pasos, string textoFinal = "Listo.")
    {
        _pasos = new Queue<PasoDelGuion>(pasos);
        _textoFinal = textoFinal;
    }

    public GuionChatClient(params string[] respuestas) : this(respuestas.Select(PasoDelGuion.Texto)) { }

    public List<LlamadaRecibida> Llamadas { get; } = [];

    /// <summary>Resultados de herramientas que el «modelo» vio, como «callId=resultado».</summary>
    public List<string> ResultadosVistos { get; } = [];

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var lista = messages.ToList();
        Llamadas.Add(new LlamadaRecibida(lista, options?.Clone()));
        if (lista.Count > 0)
        {
            foreach (var resultado in lista[^1].Contents.OfType<FunctionResultContent>())
            {
                ResultadosVistos.Add($"{resultado.CallId}={resultado.Result}");
            }
        }

        var paso = _pasos.Count > 0 ? _pasos.Dequeue() : PasoDelGuion.Texto(_textoFinal);
        var mensaje = new ChatMessage(ChatRole.Assistant, [.. paso.Contenidos]);
        var respuesta = new ChatResponse(mensaje)
        {
            ModelId = "guion",
            FinishReason = paso.Contenidos.Any(c => c is FunctionCallContent) ? ChatFinishReason.ToolCalls : ChatFinishReason.Stop,
            Usage = new UsageDetails
            {
                InputTokenCount = AI.GuardaDeTruncado.Estimar(lista, options),
                OutputTokenCount = (long)Math.Ceiling((mensaje.Text?.Length ?? 0) / AI.GuardaDeTruncado.CaracteresPorToken)
            },
            AdditionalProperties = paso.Fragmentos is null ? null : new AdditionalPropertiesDictionary { ["fragmentos"] = paso.Fragmentos }
        };
        return Task.FromResult(respuesta);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var respuesta = await GetResponseAsync(messages, options, cancellationToken);
        if (respuesta.AdditionalProperties?["fragmentos"] is IReadOnlyList<string> fragmentos)
        {
            foreach (var fragmento in fragmentos)
            {
                yield return new ChatResponseUpdate(ChatRole.Assistant, fragmento) { ModelId = respuesta.ModelId };
            }
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                FinishReason = respuesta.FinishReason,
                Contents = [new UsageContent(respuesta.Usage!)]
            };
            yield break;
        }

        foreach (var update in respuesta.ToChatResponseUpdates())
        {
            yield return update;
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType == typeof(ChatClientMetadata)
            ? new ChatClientMetadata("guion", null, "guion")
            : serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }
}
