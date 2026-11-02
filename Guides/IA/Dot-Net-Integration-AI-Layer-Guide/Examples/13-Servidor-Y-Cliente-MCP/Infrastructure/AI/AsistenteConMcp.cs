// Nuevo: el asistente del ejemplo 13. Es el mismo escalón 1 de §8.2 (el modelo pide, la app
// ejecuta, con tope), con una diferencia: la herramienta no es un método local sino una
// McpClientTool, y ejecutarla es un pedido MCP al servidor. La composición:
//   UseFunctionInvocation (bucle, tope) → grabador → capa única de opciones (_Comun) → adaptador.
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

public sealed class AsistenteConMcp
{
    /// <summary>Tope del bucle: una pregunta acotada no necesita más.</summary>
    public const int MaximoDeIteraciones = 3;

    public const string Instrucciones = """
        Sos el asistente del ABM de localidades de Movilidad Urbana. Respondé en castellano, en una o dos oraciones.
        Para saber qué localidades tiene cargadas la persona usá la herramienta listar_localidades.
        Usá sólo lo que devuelve la herramienta y nombrá cada localidad que devolvió. Si un dato no está, decí que no lo tenés; no lo inventes.
        """;

    private readonly IChatClient _bucle;

    /// <param name="capa">Cliente YA envuelto en la capa única (FabricaDeClientes).</param>
    public AsistenteConMcp(IChatClient capa)
    {
        Grabador = new GrabadorDeLlamadas(capa);
        _bucle = new ChatClientBuilder(Grabador)
            .UseFunctionInvocation(configure: f =>
            {
                f.MaximumIterationsPerRequest = MaximoDeIteraciones;
                f.MaximumConsecutiveErrorsPerRequest = 2;
            })
            .Build();
    }

    public GrabadorDeLlamadas Grabador { get; }

    /// <summary>Una pregunta, con las herramientas MCP que el cliente decidió montar.</summary>
    public async Task<ChatResponse> PreguntarAsync(string pregunta, IList<AITool> montadas, CancellationToken cancellationToken = default)
    {
        Grabador.Llamadas.Clear();
        List<ChatMessage> mensajes = [new(ChatRole.User, pregunta)];
        var opciones = new ChatOptions { Instructions = Instrucciones, Tools = montadas };
        return await _bucle.GetResponseAsync(mensajes, opciones, cancellationToken);
    }
}
