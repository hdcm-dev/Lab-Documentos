// Nuevo (ejemplo 01, plan §E.2; guía §5.1–§5.2). La aplicación sólo conoce IChatClient: no sabe
// qué adaptador hay debajo ni fija opciones propias del proveedor (eso lo hace la capa única).
// Delta (2026-10-29, REC-09): una vuelta cancelada a mitad del streaming no entra al historial; se
// mira el token en los dos casos (el proveedor que lanza y el que termina la enumeración en silencio).
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>
/// Lo que salió en una vuelta: lo enviado, las opciones, la respuesta y los fragmentos. Con
/// <paramref name="Cancelada" />, la respuesta es el parcial que llegó antes del corte y no está en el historial.
/// </summary>
public sealed record VueltaDeChat(
    IReadOnlyList<ChatMessage> Enviados, ChatOptions Opciones, ChatResponse Respuesta, IReadOnlyList<string> Fragmentos,
    bool Cancelada = false);

/// <summary>
/// Asistente de ayuda del ABM y de la encuesta. Lleva el historial EXPLÍCITO: el modelo no
/// recuerda nada entre llamadas, así que cada vuelta reenvía la conversación entera.
/// </summary>
public sealed class AsistenteDeAyuda(IChatClient cliente)
{
    /// <summary>Tope de salida de cada vuelta (clase K del presupuesto: respuestas cortas).</summary>
    public const int TopeDeSalida = 200;

    private readonly List<ChatMessage> _historial = [];

    /// <summary>
    /// Instrucciones armadas desde las *Policy del caso: si un requisito cambia en las reglas del
    /// dominio, cambia también lo que el modelo recibe. Van en ChatOptions.Instructions, no en el historial.
    /// </summary>
    public static string Instrucciones => $"""
        Sos el asistente de ayuda de Movilidad Urbana. Respondé en castellano, en una o dos oraciones,
        nombrando cada campo que pide el formulario por el que te preguntan.
        El alta de una localidad pide: nombre ({LocalidadPolicy.RequisitoDelNombre}), provincia
        ({LocalidadPolicy.RequisitoDeLaProvincia}), código postal ({LocalidadPolicy.RequisitoDelCodigoPostal})
        y habitantes ({LocalidadPolicy.RequisitoDeLosHabitantes}).
        La encuesta de movilidad pide: nombre ({EncuestaPolicy.RequisitoDelNombre}), edad ({EncuestaPolicy.RequisitoDeLaEdad}),
        localidad ({EncuestaPolicy.RequisitoDeLaLocalidad}), medios de transporte ({EncuestaPolicy.RequisitoDeLosMedios}),
        frecuencia ({EncuestaPolicy.RequisitoDeLaFrecuencia}), distancia ({EncuestaPolicy.RequisitoDeLaDistancia}),
        minutos de viaje ({EncuestaPolicy.RequisitoDeLosMinutos}) y motivo ({EncuestaPolicy.RequisitoDelMotivo}).
        """;

    public IReadOnlyList<ChatMessage> Historial => _historial;

    /// <summary>
    /// Agrega la pregunta al historial, pide la respuesta en streaming (cada fragmento va a
    /// <paramref name="alFragmento" /> apenas llega) y suma la respuesta completa al historial.
    /// Si se cancela a mitad, devuelve la vuelta marcada «cancelada» y deja el historial como estaba.
    /// </summary>
    public async Task<VueltaDeChat> PreguntarAsync(string pregunta, Action<string> alFragmento, CancellationToken cancellationToken = default)
    {
        _historial.Add(new ChatMessage(ChatRole.User, pregunta));
        var enviados = _historial.ToList();
        var opciones = new ChatOptions { Instructions = Instrucciones, MaxOutputTokens = TopeDeSalida };

        var updates = new List<ChatResponseUpdate>();
        var fragmentos = new List<string>();
        try
        {
            await foreach (var update in cliente.GetStreamingResponseAsync(enviados, opciones, cancellationToken))
            {
                updates.Add(update);
                if (!string.IsNullOrEmpty(update.Text))
                {
                    fragmentos.Add(update.Text);
                    alFragmento(update.Text);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Un proveedor corta lanzando la excepción…
        }

        // …y otro termina la enumeración sin lanzar: se mira el token en los dos casos.
        if (cancellationToken.IsCancellationRequested)
        {
            // El parcial se devuelve, pero ni él ni la pregunta quedan en el historial.
            _historial.RemoveAt(_historial.Count - 1);
            return new VueltaDeChat(enviados, opciones, updates.ToChatResponse(), fragmentos, Cancelada: true);
        }

        var respuesta = updates.ToChatResponse();
        // La métrica final de Ollama (duraciones, conteos) viaja en la cruda del último update.
        respuesta.RawRepresentation ??= updates.LastOrDefault(u => u.RawRepresentation is not null)?.RawRepresentation;
        _historial.AddRange(respuesta.Messages);
        return new VueltaDeChat(enviados, opciones, respuesta, fragmentos);
    }
}
