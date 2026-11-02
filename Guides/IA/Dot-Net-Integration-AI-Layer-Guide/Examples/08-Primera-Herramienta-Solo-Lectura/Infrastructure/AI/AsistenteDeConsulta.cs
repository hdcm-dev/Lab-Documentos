// Nuevo: el escalón 1 de la guía (§8.2): el modelo PIDE una herramienta y la app la EJECUTA, en un
// bucle con tope. La composición es la que enseña §8.2:
//   UseFunctionInvocation (bucle, tope) → grabador → capa única de opciones (_Comun) → adaptador.
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

public sealed class AsistenteDeConsulta
{
    /// <summary>Tope del bucle (MaximumIterationsPerRequest): una pregunta acotada no necesita más.</summary>
    public const int MaximoDeIteraciones = 3;

    public const string Instrucciones = """
        Sos el asistente del ABM de localidades de Movilidad Urbana. Respondé en castellano, en una o dos oraciones.
        Para saber qué localidades tiene cargadas la persona usá las herramientas: listar_localidades y contar_localidades.
        Usá sólo lo que devuelven las herramientas. Si un dato no está en lo que devolvieron, decí que no lo tenés; no lo inventes ni lo estimes.
        Si una herramienta devuelve un error, explicá el error y no des una cantidad.
        """;

    private readonly IChatClient _bucle;

    /// <param name="capa">Cliente YA envuelto en la capa única (FabricaDeClientes).</param>
    public AsistenteDeConsulta(IChatClient capa)
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

    /// <summary>Una pregunta de la persona, con las herramientas de SU sesión montadas.</summary>
    public async Task<ChatResponse> PreguntarAsync(string pregunta, HerramientasDeLectura herramientas, CancellationToken cancellationToken = default)
    {
        Grabador.Llamadas.Clear();
        List<ChatMessage> mensajes = [new(ChatRole.User, pregunta)];
        var opciones = new ChatOptions { Instructions = Instrucciones, Tools = herramientas.Crear() };
        return await _bucle.GetResponseAsync(mensajes, opciones, cancellationToken);
    }
}
