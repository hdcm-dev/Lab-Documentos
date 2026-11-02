// Nuevo (ejemplo 16; plan §F.4, «Política de origen»; H-012, H2-012, H3-008). Es NO CONFIABLE todo
// texto que la app no escribió; acá, el Nombre que devuelve listar_localidades, que cargó una persona
// por el ABM. Dos reglas, las dos en código y no en el prompt:
//   (1) Origen: si el pedido trae texto no confiable al armarse, el cliente se arma SIN herramientas
//       de escritura. Garantía mecánica: no hay pedido de escritura posible.
//   (2) Marca: si una herramienta devuelve texto no confiable a mitad del bucle, el pedido queda
//       marcado. Desde ahí una escritura sólo se ejecuta con aprobación humana cuyo pedido arma el
//       código (y lo dice); una escritura no aprobable rechaza sin ejecutar hasta que el pedido termina.
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Application.Origen;

/// <summary>Las variantes del ejemplo. «origen» no se corre contra el modelo: es una garantía de código.</summary>
public static class Variantes
{
    public const string Origen = "origen";
    public const string Aprobacion = "aprobacion";
    public const string AprobacionYMarcado = "aprobacion+marcado";

    public static bool Valida(string v) => v is Origen or Aprobacion or AprobacionYMarcado;
}

/// <summary>Estado de un pedido (una conversación): si ya leyó texto que la app no escribió.</summary>
public sealed class EstadoDelPedido
{
    public bool Marcado { get; private set; }

    /// <summary>Qué herramienta trajo el texto no confiable (para el registro y el pedido de aprobación).</summary>
    public string? MarcadoPor { get; private set; }

    public void Marcar(string herramienta)
    {
        if (Marcado) return;
        Marcado = true;
        MarcadoPor = herramienta;
    }
}

/// <summary>Quién aprobó qué: lo implementa el servidor de aprobaciones.</summary>
public interface IAprobaciones
{
    bool Aprobada(string callId);
}

/// <summary>
/// Regla 2 en la frontera de la herramienta: una escritura sobre un pedido marcado se ejecuta sólo si
/// su llamada tiene aprobación humana registrada. Envuelve la función de escritura; si además va
/// dentro de <see cref="ApprovalRequiredAIFunction" />, FunctionInvokingChatClient no la invoca sin
/// respuesta de aprobación, y esta capa no lo da por hecho (defensa en capas).
/// </summary>
public sealed class EscrituraVigilada(AIFunction interna, EstadoDelPedido estado, IAprobaciones aprobaciones) : DelegatingAIFunction(interna)
{
    public const string MotivoDeRechazo =
        "Rechazado por la política de origen: este pedido leyó texto que la app no escribió y la escritura no tiene aprobación de la persona.";

    /// <summary>Llamadas que esta capa rechazó sin ejecutar (CallId).</summary>
    public List<string> Rechazadas { get; } = [];

    protected override ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var callId = FunctionInvokingChatClient.CurrentContext?.CallContent.CallId;
        if (estado.Marcado && (callId is null || !aprobaciones.Aprobada(callId)))
        {
            Rechazadas.Add(callId ?? "(fuera del bucle)");
            return ValueTask.FromResult<object?>(MotivoDeRechazo);
        }
        return base.InvokeCoreAsync(arguments, cancellationToken);
    }
}
