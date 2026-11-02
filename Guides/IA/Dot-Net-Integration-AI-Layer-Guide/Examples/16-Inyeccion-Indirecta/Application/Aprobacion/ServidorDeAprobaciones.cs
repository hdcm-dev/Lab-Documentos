// Nuevo (ejemplo 16; plan §F.4, «Pedido de aprobación»; H-010, H2-009). Como en 09 y 11: el pedido
// pendiente vive en el servidor y la respuesta se arma con CreateResponse sobre el pedido GUARDADO.
// Lo que ve la persona lo arma el código desde la FunctionCallContent, resuelta en la sesión; la prosa
// del modelo no entra. Si el pedido está marcado (leyó texto que la app no escribió), lo dice.
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Herramientas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Application.Origen;

namespace MovilidadUrbana.Asistente.Application.Aprobacion;

/// <summary>Un pedido que vio la persona, con lo que resolvió el código.</summary>
/// <param name="IdResuelto">Id de la localidad que se va a borrar (resuelto en la sesión); null para una lectura.</param>
/// <param name="Marcado">El pedido viene después de leer texto que la app no escribió (regla 2).</param>
public sealed record PedidoPendiente(
    string Id, string CallId, string Herramienta, IReadOnlyDictionary<string, object?> Argumentos, string Muestra,
    int? IdResuelto, bool Marcado, ToolApprovalRequestContent Pedido);

public sealed class ServidorDeAprobaciones : IAprobaciones, IBajasAprobadas
{
    public const string AvisoDeMarca = "Este pedido viene después de leer texto que la app no escribió.";

    private readonly Dictionary<string, PedidoPendiente> _pendientes = [];
    private readonly Dictionary<string, int> _bajasAprobadas = [];
    private readonly List<ToolApprovalResponseContent> _automaticas = [];

    /// <summary>CallId (no id del pedido) de toda llamada que una persona aprobó.</summary>
    public HashSet<string> Aprobados { get; } = [];

    /// <summary>Pedidos que el código rechazó sin preguntar, con su motivo.</summary>
    public List<(string Id, string Motivo)> RechazadosSinPreguntar { get; } = [];

    public bool Aprobada(string callId) => Aprobados.Contains(callId);

    public int? IdAprobado(string callId) => _bajasAprobadas.TryGetValue(callId, out var id) ? id : null;

    /// <summary>
    /// Fin de una petición: guarda cada pedido de la respuesta y devuelve los que tiene que ver la
    /// persona. Una baja que no resuelve en la sesión (inexistente o ambigua) se rechaza sin preguntar.
    /// </summary>
    public async Task<IReadOnlyList<PedidoPendiente>> RegistrarAsync(
        IEnumerable<ChatMessage> mensajes, LocalidadService localidades, EstadoDelPedido estado, CancellationToken cancellationToken = default)
    {
        var pedidos = mensajes.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();
        var aviso = estado.Marcado ? " " + AvisoDeMarca : string.Empty;
        var aMostrar = new List<PedidoPendiente>();
        foreach (var pedido in pedidos)
        {
            var llamada = (FunctionCallContent)pedido.ToolCall;
            var argumentos = Copiar(llamada.Arguments);
            switch (llamada.Name)
            {
                case HerramientasDeLaInyeccion.Eliminar:
                    var r = await localidades.GetByNombreYProvinciaAsync(Texto(argumentos, "nombre"), Texto(argumentos, "provincia"), cancellationToken);
                    if (r.Localidad is not { } l)
                    {
                        RechazarSinPreguntar(pedido, string.Join(" ", r.Error!.Errors.Values));
                        continue;
                    }
                    aMostrar.Add(Guardar(new PedidoPendiente(pedido.RequestId, llamada.CallId, llamada.Name, argumentos,
                        $"Borrar \"{l.Nombre}\" ({l.Provincia}), id {l.Id}, de tu sesión.{aviso}", l.Id, estado.Marcado, pedido)));
                    break;
                case HerramientasDeLaInyeccion.Listar:
                    // M.E.AI pide aprobación para todas las llamadas de una respuesta si alguna es aprobable.
                    aMostrar.Add(Guardar(new PedidoPendiente(pedido.RequestId, llamada.CallId, llamada.Name, argumentos,
                        $"Leer las localidades de tu sesión (sólo lectura).{aviso}", null, estado.Marcado, pedido)));
                    break;
                default:
                    RechazarSinPreguntar(pedido, $"La herramienta «{llamada.Name}» no está montada.");
                    break;
            }
        }
        return aMostrar;
    }

    /// <summary>
    /// Segunda petición: llega el id y la decisión. Si además llega una llamada con argumentos distintos
    /// de los del pedido guardado, se rechaza: lo que se ejecuta es exactamente lo que se mostró (AC-4).
    /// </summary>
    public ToolApprovalResponseContent Responder(string id, bool aprobar, FunctionCallContent? recibida = null)
    {
        if (!_pendientes.Remove(id, out var pendiente))
        {
            throw new InvalidOperationException($"No hay un pedido de aprobación pendiente con id {id}.");
        }
        if (recibida is not null && (recibida.Name != pendiente.Herramienta || Canonico(Copiar(recibida.Arguments)) != Canonico(pendiente.Argumentos)))
        {
            return pendiente.Pedido.CreateResponse(false, "Los datos no coinciden con el pedido que se mostró.");
        }
        if (aprobar)
        {
            Aprobados.Add(pendiente.CallId);
            if (pendiente.IdResuelto is int idResuelto) _bajasAprobadas[pendiente.CallId] = idResuelto;
        }
        return pendiente.Pedido.CreateResponse(aprobar, aprobar ? null : "La persona no lo aprobó.");
    }

    /// <summary>Las respuestas que armó el código solo (rechazos sin preguntar); se vacía al leerla.</summary>
    public IReadOnlyList<ToolApprovalResponseContent> TomarAutomaticas()
    {
        var copia = _automaticas.ToList();
        _automaticas.Clear();
        return copia;
    }

    private void RechazarSinPreguntar(ToolApprovalRequestContent pedido, string motivo)
    {
        RechazadosSinPreguntar.Add((pedido.RequestId, motivo));
        _automaticas.Add(pedido.CreateResponse(false, motivo));
    }

    private PedidoPendiente Guardar(PedidoPendiente p)
    {
        _pendientes[p.Id] = p;
        return p;
    }

    public static IReadOnlyDictionary<string, object?> Copiar(IDictionary<string, object?>? argumentos) =>
        new Dictionary<string, object?>(argumentos ?? new Dictionary<string, object?>(), StringComparer.Ordinal);

    /// <summary>Valor de un argumento como texto: el modelo puede mandar «Chaco» o un JsonElement.</summary>
    public static string Texto(IReadOnlyDictionary<string, object?> argumentos, string clave) =>
        argumentos.TryGetValue(clave, out var v) ? v switch
        {
            null => string.Empty,
            JsonElement { ValueKind: JsonValueKind.String } e => e.GetString() ?? string.Empty,
            JsonElement e => e.GetRawText(),
            IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => v.ToString() ?? string.Empty
        } : string.Empty;

    public static string Canonico(IReadOnlyDictionary<string, object?> argumentos) =>
        string.Join("\n", argumentos.Keys.Order(StringComparer.Ordinal).Select(k => $"{k}={Texto(argumentos, k)}"));
}
