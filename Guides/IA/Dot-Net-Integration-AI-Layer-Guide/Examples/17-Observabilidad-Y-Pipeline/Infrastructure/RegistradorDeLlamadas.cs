// Nuevo (ejemplo 17). Va justo encima del cliente de la capa única: todo lo que pasa por acá es una
// llamada al modelo (o al guion). Es la cuenta de costo real: un acierto de caché no llega.
using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure;

/// <summary>Una llamada que llegó al modelo: lo enviado, las opciones (las de arriba de la capa) y la respuesta.</summary>
public sealed record LlamadaAlModelo(IReadOnlyList<ChatMessage> Enviados, ChatOptions? Opciones, ChatResponse Respuesta, TimeSpan Reloj);

public sealed class RegistradorDeLlamadas(IChatClient interno) : DelegatingChatClient(interno)
{
    public IChatClient Interno { get; } = interno;

    public List<LlamadaAlModelo> Llamadas { get; } = [];

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var enviados = messages.ToList();
        var reloj = Stopwatch.StartNew();
        var respuesta = await base.GetResponseAsync(enviados, options, cancellationToken);
        Llamadas.Add(new LlamadaAlModelo(enviados, options?.Clone(), respuesta, reloj.Elapsed));
        return respuesta;
    }
}
