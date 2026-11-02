// Nuevo. Delta: Agregado por la IA — regla 2 de la política de origen sobre cada escritura (plan §F.4; H3-008).
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure.AI.Chat;

/// <summary>
/// Envuelve una herramienta de escritura. Si el pedido está marcado (leyó texto que la app no
/// escribió) y esta llamada no tiene una aprobación armada por el código, no ejecuta: devuelve un
/// error al modelo. Con <see cref="ApprovalRequiredAIFunction" /> por fuera, la invocación llega
/// sólo después de aprobar, así que la guarda deja pasar; sin ella, rechaza.
/// </summary>
public sealed class GuardaDeOrigen(AIFunction interna, EstadoDelPedido estado) : DelegatingAIFunction(interna)
{
    public const string Rechazo = "Rechazado sin ejecutar: el pedido leyó texto que la app no escribió y esta escritura no tiene aprobación de la persona.";

    protected override ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var callId = FunctionInvokingChatClient.CurrentContext?.CallContent.CallId;
        if (estado.Marcado && estado.Aprobada(callId) is null)
        {
            return ValueTask.FromResult<object?>(Rechazo);
        }
        return base.InvokeCoreAsync(arguments, cancellationToken);
    }
}
