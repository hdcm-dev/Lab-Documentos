// Nuevo. Delta: Agregado por la IA — modo guion del host (ASISTENTE_MODO=guion), ESCRITO PARA EL EJEMPLO.
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

/// <summary>
/// Un <see cref="IChatClient" /> sin modelo, para levantar el host y recorrer la página sin servidor
/// de modelos (captura de la evidencia, lectores sin Ollama). Reconoce tres pedidos por patrón y
/// pide la herramienta que corresponde; todo lo demás lo responde con un aviso. No es una prueba de
/// conducta: muestra el cableado (herramientas = casos de uso, aprobación armada por el código).
/// Pasa por la misma capa de opciones que el cliente real (<c>FabricaDeClientes.Envolver</c>).
/// </summary>
public sealed partial class ClienteDeDemostracion : IChatClient
{
    public const string Aviso = "Modo guion (sin modelo): reconozco «¿Cuántas localidades tengo en <provincia>?», «¿Qué medio usa más la gente?» y «Borrá <localidad>».";

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var lista = messages.ToList();
        var ultimo = lista.LastOrDefault();
        var contenido = Responder(ultimo, options);
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, [contenido])) { ModelId = "guion" });
    }

    private static AIContent Responder(ChatMessage? ultimo, ChatOptions? options)
    {
        // Salida estructurada (extractores, ayuda): sin modelo no hay interpretación; el caso de uso cae al camino sin IA.
        if (options?.ResponseFormat is ChatResponseFormatJson) return new TextContent("null");

        if (ultimo?.Contents.OfType<FunctionResultContent>().FirstOrDefault() is { } resultado)
        {
            var texto = resultado.Result switch
            {
                string s => s,
                null => "sin resultado",
                var otro => JsonSerializer.Serialize(otro, AIJsonUtilities.DefaultOptions)
            };
            return new TextContent($"Resultado de la herramienta: {texto}");
        }

        var pedido = ultimo?.Text ?? string.Empty;
        if (Borrar().Match(pedido) is { Success: true } b)
        {
            return new FunctionCallContent($"guion-{Guid.NewGuid():n}", Chat.HerramientasDelAsistente.Eliminar,
                new Dictionary<string, object?> { ["nombre"] = b.Groups["nombre"].Value.Trim() });
        }
        if (Contar().Match(pedido) is { Success: true } c)
        {
            return new FunctionCallContent($"guion-{Guid.NewGuid():n}", Chat.HerramientasDelAsistente.Contar,
                new Dictionary<string, object?> { ["provincia"] = c.Groups["provincia"].Value.Trim() });
        }
        if (Medio().IsMatch(pedido))
        {
            return new FunctionCallContent($"guion-{Guid.NewGuid():n}", Chat.HerramientasDelAsistente.Resumir, new Dictionary<string, object?>());
        }
        return new TextContent(Aviso);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var update in (await GetResponseAsync(messages, options, cancellationToken)).ToChatResponseUpdates()) yield return update;
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }

    [GeneratedRegex(@"^\s*borr[aá]\s+(la localidad\s+)?(?<nombre>[^?.!]+)", RegexOptions.IgnoreCase)]
    private static partial Regex Borrar();

    [GeneratedRegex(@"cu[aá]ntas localidades.*\ben\s+(?<provincia>[^?]+)", RegexOptions.IgnoreCase)]
    private static partial Regex Contar();

    [GeneratedRegex(@"\bmedio\b", RegexOptions.IgnoreCase)]
    private static partial Regex Medio();
}
