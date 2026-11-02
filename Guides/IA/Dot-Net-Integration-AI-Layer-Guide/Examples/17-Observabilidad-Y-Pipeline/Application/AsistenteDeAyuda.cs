// Nuevo (ejemplo 17, plan §E.2; guía §12.1–§12.3). La aplicación sólo conoce IChatClient: no sabe
// si debajo hay caché, telemetría o registro, ni en qué orden. Eso lo decide la raíz de composición.
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>Un pedido de la entrada: el texto y, si la entrada la fija, la temperatura (variable del experimento).</summary>
public sealed record Pedido(string Id, string Texto, float? Temperatura);

/// <summary>Una entrada de <c>entradas.v1.json</c>: tres pedidos, dos iguales y uno con otra temperatura.</summary>
public sealed record EntradaDeEjemplo(string Id, IReadOnlyList<Pedido> Pedidos, int LlamadasEsperadas, int SpansConCacheAfuera, int SpansConTelemetriaAfuera)
{
    public static IReadOnlyList<EntradaDeEjemplo> Cargar(string ruta)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(ruta));
        return doc.RootElement.GetProperty("entradas").EnumerateArray().Select(e =>
        {
            var esperado = e.GetProperty("esperado");
            return new EntradaDeEjemplo(
                e.GetProperty("id").GetString()!,
                e.GetProperty("pedidos").EnumerateArray().Select(p => new Pedido(
                    p.GetProperty("id").GetString()!,
                    p.GetProperty("texto").GetString()!,
                    p.GetProperty("temperatura").ValueKind == JsonValueKind.Null ? null : p.GetProperty("temperatura").GetSingle())).ToList(),
                esperado.GetProperty("llamadas_al_modelo").GetInt32(),
                esperado.GetProperty("spans_con_cache_afuera").GetInt32(),
                esperado.GetProperty("spans_con_telemetria_afuera").GetInt32());
        }).ToList();
    }
}

/// <summary>
/// Asistente de ayuda del alta de localidades. Cada pedido es una conversación de un solo turno:
/// así dos pedidos con el mismo texto y las mismas opciones son idénticos para la caché.
/// </summary>
public sealed class AsistenteDeAyuda(IChatClient cliente)
{
    /// <summary>Tope de salida (clase K del presupuesto: una oración).</summary>
    public const int TopeDeSalida = 120;

    /// <summary>Instrucciones armadas desde <see cref="LocalidadPolicy" />: van en ChatOptions, no en el historial.</summary>
    public static string Instrucciones => $"""
        Sos el asistente de ayuda de Movilidad Urbana. Respondé en castellano, en una sola oración.
        El alta de una localidad pide: nombre ({LocalidadPolicy.RequisitoDelNombre}), provincia
        ({LocalidadPolicy.RequisitoDeLaProvincia}), código postal ({LocalidadPolicy.RequisitoDelCodigoPostal})
        y habitantes ({LocalidadPolicy.RequisitoDeLosHabitantes}).
        """;

    /// <summary>
    /// Arma el pedido. La temperatura sólo se pone si la entrada la trae (es la variable del
    /// experimento); si no, queda vacía y la completa la capa de opciones, debajo del pipeline.
    /// </summary>
    public static ChatOptions OpcionesDe(Pedido pedido) => new()
    {
        Instructions = Instrucciones,
        MaxOutputTokens = TopeDeSalida,
        Temperature = pedido.Temperatura
    };

    public static IReadOnlyList<ChatMessage> MensajesDe(Pedido pedido) => [new ChatMessage(ChatRole.User, pedido.Texto)];

    public Task<ChatResponse> PedirAsync(Pedido pedido, CancellationToken cancellationToken = default) =>
        cliente.GetResponseAsync(MensajesDe(pedido), OpcionesDe(pedido), cancellationToken);
}
