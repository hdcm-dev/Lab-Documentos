// Nuevo (plan §E.2, fila 07; guía §7.4). Recupera top-k, arma el contexto con la Fuente de cada
// fragmento, pide la respuesta y la valida en código. El modelo propone; ValidadorDeCitas decide
// qué se muestra.
using System.Text;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Application;

public sealed record ConsultaDeAyuda(
    IReadOnlyList<Recuperacion> Recuperados,
    string Contexto,
    IReadOnlyList<ChatMessage> Mensajes,
    ChatOptions Opciones,
    ChatResponse Respuesta,
    RespuestaConCitas Validada);

public sealed class AsistenteDeAyuda(IChatClient cliente, IndiceDeAyuda indice, int top = AsistenteDeAyuda.TopPorDefecto)
{
    public const int TopPorDefecto = 3;

    // REC-28 (revisión de las instrucciones): traen un ejemplo de cita, «[Fuente: LocalidadPolicy.
    // RequisitoDelCodigoPostal]», que es además una fuente real del corpus (la relevante de r03). Lección
    // «el formato sale del ejemplo»: el modelo tiende a copiar la forma del ejemplo, y con ella puede
    // copiar el id. No se cambian acá para no mover la medición; el registro guarda hash_instrucciones.
    public const string Instrucciones =
        "Sos el asistente de ayuda del sistema de Movilidad Urbana. Respondé en castellano, en una o dos oraciones, " +
        "usando SÓLO los fragmentos del contexto. Al final de la respuesta poné la fuente entre corchetes, exactamente así: " +
        "[Fuente: LocalidadPolicy.RequisitoDelCodigoPostal]. Citá sólo fuentes que aparezcan en el contexto. " +
        "Si el contexto no alcanza para responder la pregunta, respondé exactamente: No sé.";

    public int Top { get; } = top;

    /// <summary>Contexto de grounding: un renglón por fragmento, con su fuente adelante.</summary>
    public static string ArmarContexto(IEnumerable<Recuperacion> recuperados)
    {
        var sb = new StringBuilder();
        foreach (var r in recuperados) sb.Append("[Fuente: ").Append(r.Fuente).Append("] ").AppendLine(r.Texto);
        return sb.ToString();
    }

    public async Task<ConsultaDeAyuda> PreguntarAsync(string pregunta, CancellationToken cancellationToken = default)
    {
        var recuperados = await indice.BuscarAsync(pregunta, Top, cancellationToken);
        var contexto = ArmarContexto(recuperados);
        // Las instrucciones van en ChatOptions.Instructions (como en 01–04); el contexto y la pregunta,
        // en el mensaje de la persona, con la pregunta al final.
        List<ChatMessage> mensajes = [new(ChatRole.User, $"Contexto:\n{contexto}\nPregunta: {pregunta}")];
        var opciones = new ChatOptions { Instructions = Instrucciones };
        var respuesta = await cliente.GetResponseAsync(mensajes, opciones, cancellationToken);
        var validada = ValidadorDeCitas.Validar(respuesta.Text ?? string.Empty, recuperados.Select(r => r.Fuente).ToHashSet(StringComparer.Ordinal));
        return new(recuperados, contexto, mensajes, opciones, respuesta, validada);
    }
}
