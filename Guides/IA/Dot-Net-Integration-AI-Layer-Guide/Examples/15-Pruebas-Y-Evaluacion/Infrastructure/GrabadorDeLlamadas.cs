// Nuevo (plan §E.1, registro R-LLM). Los evaluadores de M.E.AI.Evaluation llaman al juez por dentro:
// para registrar cada llamada (mensajes, opciones, respuesta y duración) se pone este cliente entre
// el reporte y la capa única de opciones. Lo que el reporte sirve desde su caché no pasa por acá: no
// es una llamada al modelo y no se registra como tal.
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure;

public sealed record LlamadaGrabada(IReadOnlyList<ChatMessage> Mensajes, ChatOptions? Opciones, ChatResponse Respuesta, TimeSpan Duracion);

public sealed class GrabadorDeLlamadas(IChatClient interno) : DelegatingChatClient(interno)
{
    private readonly ConcurrentQueue<LlamadaGrabada> _grabadas = new();

    public int Total { get; private set; }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var lista = messages.ToList();
        var reloj = Stopwatch.StartNew();
        var respuesta = await base.GetResponseAsync(lista, options, cancellationToken);
        _grabadas.Enqueue(new(lista, options?.Clone(), respuesta, reloj.Elapsed));
        Total++;
        return respuesta;
    }

    /// <summary>Devuelve y olvida las llamadas grabadas desde la última vez.</summary>
    public IReadOnlyList<LlamadaGrabada> Tomar()
    {
        var salida = new List<LlamadaGrabada>();
        while (_grabadas.TryDequeue(out var l)) salida.Add(l);
        return salida;
    }
}
