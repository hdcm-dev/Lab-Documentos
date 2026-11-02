// Nuevo: anota cada llamada al modelo de un nodo del workflow (redactor o corrector). Va ENCIMA de la
// capa única de opciones: así cada llamada deja su línea R-LLM con el nodo que la hizo, y el agente de
// MAF recibe el mismo cliente que el redactor (no construye el suyo).
using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

/// <summary>Una llamada al modelo: qué nodo, qué se mandó, con qué opciones y qué volvió.</summary>
public sealed record LlamadaGrabada(string Nodo, IReadOnlyList<ChatMessage> Mensajes, ChatOptions? Opciones, ChatResponse Respuesta, TimeSpan Reloj);

public sealed class GrabadorDeLlamadas(IChatClient interno, string nodo, List<LlamadaGrabada> destino) : DelegatingChatClient(interno)
{
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var enviados = messages.ToList();
        var reloj = Stopwatch.StartNew();
        var respuesta = await base.GetResponseAsync(enviados, options, cancellationToken);
        reloj.Stop();
        lock (destino)
        {
            destino.Add(new LlamadaGrabada(nodo, enviados, options?.Clone(), respuesta, reloj.Elapsed));
        }
        return respuesta;
    }
}
