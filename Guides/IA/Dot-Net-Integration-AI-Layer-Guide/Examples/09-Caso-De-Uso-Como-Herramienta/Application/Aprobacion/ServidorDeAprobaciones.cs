// Nuevo (ejemplo 09; plan §F.4, «Pedido de aprobación»; H-010, H2-009, DEP-09). El pedido de
// aprobación vive en el servidor: se guarda al terminar la primera petición y la segunda petición
// trae sólo el id y la decisión. La respuesta se arma con CreateResponse sobre el pedido GUARDADO,
// nunca con datos del cliente: FunctionInvokingChatClient ejecuta la llamada que trae la respuesta.
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Herramientas;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Application.Aprobacion;

/// <summary>Lo que la persona ve: lo arma el código desde los argumentos, no la prosa del modelo.</summary>
public sealed record PedidoPendiente(string Id, string CallId, string Herramienta, IReadOnlyDictionary<string, object?> Argumentos, string Muestra, ToolApprovalRequestContent Pedido);

public sealed class ServidorDeAprobaciones(LocalidadService localidades)
{
    private readonly Dictionary<string, PedidoPendiente> _pendientes = [];

    /// <summary>Ids de llamada del modelo (CallId, no el id del pedido) que una persona aprobó (lo usa el invariante «ninguna escritura sin aprobación»).</summary>
    public HashSet<string> Aprobados { get; } = [];

    /// <summary>Pedidos que el código rechazó sin preguntar (baja que no resuelve en la sesión), con su motivo.</summary>
    public List<(string Id, string Motivo)> RechazadosSinPreguntar { get; } = [];

    /// <summary>Respuestas armadas por el código sin pasar por la persona, que van en la próxima petición.</summary>
    private readonly List<ToolApprovalResponseContent> _automaticas = [];

    /// <summary>
    /// Fin de la primera petición: guarda cada pedido de la respuesta y devuelve los que tiene que ver
    /// la persona. Una baja que no resuelve (inexistente o ambigua) se rechaza sin preguntar.
    /// </summary>
    public async Task<IReadOnlyList<PedidoPendiente>> RegistrarAsync(ChatResponse respuesta, CancellationToken cancellationToken = default)
    {
        var aMostrar = new List<PedidoPendiente>();
        foreach (var pedido in respuesta.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>())
        {
            var llamada = (FunctionCallContent)pedido.ToolCall;
            var argumentos = Copiar(llamada.Arguments);
            if (llamada.Name == HerramientasDeLocalidades.Eliminar)
            {
                var r = await localidades.GetByNombreYProvinciaAsync(Texto(argumentos, "nombre"), Texto(argumentos, "provincia"), cancellationToken);
                if (r.Localidad is null)
                {
                    var motivo = string.Join(" ", r.Error!.Errors.Values);
                    RechazadosSinPreguntar.Add((pedido.RequestId, motivo));
                    _automaticas.Add(pedido.CreateResponse(false, motivo));
                    continue;
                }
                var l = r.Localidad;
                aMostrar.Add(Guardar(pedido, llamada.Name, argumentos,
                    $"Dar de BAJA la localidad {l.Nombre} ({l.Provincia}, CP {l.CodigoPostal}, {l.Habitantes} habitantes) de tu sesión."));
                continue;
            }
            aMostrar.Add(Guardar(pedido, llamada.Name, argumentos,
                $"Dar de ALTA la localidad «{Texto(argumentos, "nombre")}», provincia «{Texto(argumentos, "provincia")}», " +
                $"CP «{Texto(argumentos, "codigoPostal")}», {Texto(argumentos, "habitantes")} habitantes."));
        }
        return aMostrar;
    }

    /// <summary>
    /// Segunda petición: llega el id y la decisión. Si además llega una llamada (por ejemplo, un
    /// cliente que reenvía lo que cree haber aprobado) y sus argumentos no son los del pedido
    /// guardado, se rechaza: lo que se ejecuta es exactamente lo que se mostró (AC-4).
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
        if (aprobar) Aprobados.Add(pendiente.CallId);
        return pendiente.Pedido.CreateResponse(aprobar, aprobar ? null : "La persona no lo aprobó.");
    }

    /// <summary>Las respuestas que el código armó solo (rechazos sin preguntar); se vacía al leerla.</summary>
    public IReadOnlyList<ToolApprovalResponseContent> TomarAutomaticas()
    {
        var copia = _automaticas.ToList();
        _automaticas.Clear();
        return copia;
    }

    private PedidoPendiente Guardar(ToolApprovalRequestContent pedido, string herramienta, IReadOnlyDictionary<string, object?> argumentos, string muestra)
    {
        var p = new PedidoPendiente(pedido.RequestId, ((FunctionCallContent)pedido.ToolCall).CallId, herramienta, argumentos, muestra, pedido);
        _pendientes[p.Id] = p;
        return p;
    }

    public static IReadOnlyDictionary<string, object?> Copiar(IDictionary<string, object?>? argumentos) =>
        new Dictionary<string, object?>(argumentos ?? new Dictionary<string, object?>(), StringComparer.Ordinal);

    /// <summary>Valor de un argumento como texto: el modelo puede mandar «90000» o 90000.</summary>
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
