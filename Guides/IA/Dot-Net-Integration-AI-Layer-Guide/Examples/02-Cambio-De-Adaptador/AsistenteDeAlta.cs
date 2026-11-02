// Nuevo: el código de aplicación del ejemplo 02. Sólo conoce IChatClient: no sabe qué adaptador
// tiene debajo ni a qué servidor habla. Cambiar de adaptador no toca este archivo (guía §5.6).
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>Responde qué pide el alta de una localidad, con los requisitos que enuncia el dominio.</summary>
public sealed class AsistenteDeAlta(IChatClient cliente)
{
    public static string Instrucciones => $"""
        Respondé en castellano, en una o dos oraciones. El alta de una localidad pide:
        nombre ({LocalidadPolicy.RequisitoDelNombre}), provincia ({LocalidadPolicy.RequisitoDeLaProvincia}),
        código postal ({LocalidadPolicy.RequisitoDelCodigoPostal}) y habitantes ({LocalidadPolicy.RequisitoDeLosHabitantes}).
        """;

    /// <summary>Los mensajes que se mandan (el registro los guarda tal cual).</summary>
    public static List<ChatMessage> Mensajes(string pregunta) => [new(ChatRole.User, pregunta)];

    /// <summary>Las opciones que pone la aplicación: sólo las instrucciones. El resto lo agrega la capa única.</summary>
    public static ChatOptions Opciones() => new() { Instructions = Instrucciones };

    public Task<ChatResponse> ResponderAsync(IReadOnlyList<ChatMessage> mensajes, CancellationToken cancellationToken = default) =>
        cliente.GetResponseAsync(mensajes, Opciones(), cancellationToken);

    /// <summary>Quién está del otro lado, según el propio cliente (lo informa el adaptador).</summary>
    public ChatClientMetadata? Metadatos => cliente.GetService<ChatClientMetadata>();
}
