// Nuevo (ejemplo 14, REC-30). Un IChatClient con guion fijo para las trampas de la migración, sin
// servidor: en el primer turno pide la herramienta del guion con sus argumentos; cuando recibe el
// resultado, lo repite como texto. Muestra el cableado de cada marco; no dice nada de cómo se
// comporta un modelo. Pide el nombre tal como se lo ofrecieron (con el prefijo del plugin si lo
// hay), como haría un modelo que copia la declaración.
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Pruebas;

public sealed class ClienteGuionado(string herramienta, IDictionary<string, object?> argumentos) : IChatClient
{
    public int Llamadas { get; private set; }

    /// <summary>Nombres de las herramientas que ofreció cada llamada al «modelo».</summary>
    public List<IReadOnlyList<string>> HerramientasOfrecidas { get; } = [];

    /// <summary>Nombre que pidió el «modelo» en su llamada a la herramienta.</summary>
    public string? NombrePedido { get; private set; }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Llamadas++;
        var ofrecidas = (options?.Tools ?? []).OfType<AIFunctionDeclaration>().Select(t => t.Name).ToList();
        HerramientasOfrecidas.Add(ofrecidas);

        var resultado = messages.SelectMany(m => m.Contents).OfType<FunctionResultContent>().LastOrDefault();
        AIContent contenido;
        if (resultado is null)
        {
            NombrePedido = ofrecidas.FirstOrDefault(n => n == herramienta || n.EndsWith("_" + herramienta, StringComparison.Ordinal)) ?? herramienta;
            contenido = new FunctionCallContent($"guion-{Llamadas}", NombrePedido, new Dictionary<string, object?>(argumentos));
        }
        else
        {
            contenido = new TextContent($"(sin modelo) {Texto(resultado.Result)}");
        }
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, [contenido])));
    }

    private static string Texto(object? valor) => valor switch
    {
        JsonElement e => e.GetRawText(),
        string s => s,
        _ => JsonSerializer.Serialize(valor)
    };

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Las trampas de la migración no usan streaming.");

    // SK 1.81.0 pide ChatClientMetadata al envolver el IChatClient para una función de prompt
    // (ChatClientAIService); sin ella lanza ArgumentNullException «metadata» antes de llamar.
    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceType == typeof(ChatClientMetadata) ? new ChatClientMetadata("guion")
        : serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }
}
