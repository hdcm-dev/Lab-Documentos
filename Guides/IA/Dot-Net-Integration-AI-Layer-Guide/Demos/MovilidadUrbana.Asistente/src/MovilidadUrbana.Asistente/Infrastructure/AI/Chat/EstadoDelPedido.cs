// Nuevo. Delta: Agregado por la IA — estado del pedido en curso: política de origen y escrituras aprobadas (plan §F.4; H3-008).
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Infrastructure.AI.Chat;

/// <summary>Lo que una escritura aprobada va a ejecutar: exactamente lo que se mostró en el pedido (AC-4).</summary>
public abstract record EscrituraResuelta;

/// <summary>Alta resuelta por el código: los datos ya normalizados que vio la persona.</summary>
public sealed record AltaResuelta(LocalidadModel Modelo) : EscrituraResuelta;

/// <summary>Baja resuelta por el código: la localidad de la sesión, con su id.</summary>
public sealed record BajaResuelta(int Id, string Nombre, string Provincia) : EscrituraResuelta;

/// <summary>
/// Vive en el scope del circuito (o de la petición). Un «pedido» va desde el mensaje de la persona
/// hasta la respuesta final, con las aprobaciones que haya en el medio.
///
/// Política de origen, regla 2: si una herramienta devolvió texto que la app no escribió (los
/// <c>Nombre</c> de <c>listar_localidades</c>), el pedido queda <see cref="Marcado" />; desde ahí,
/// una escritura se ejecuta sólo si su llamada fue aprobada por la persona con un pedido armado por
/// el código; una escritura no aprobable se rechaza sin ejecutar.
/// </summary>
public sealed class EstadoDelPedido
{
    private readonly Dictionary<string, EscrituraResuelta> _aprobadas = new(StringComparer.Ordinal);

    public bool Marcado { get; private set; }

    public void Empezar(bool marcadoPorElHistorial)
    {
        Marcado = marcadoPorElHistorial;
        _aprobadas.Clear();
    }

    public void MarcarNoConfiable() => Marcado = true;

    public void Aprobar(string callId, EscrituraResuelta escritura) => _aprobadas[callId] = escritura;

    public EscrituraResuelta? Aprobada(string? callId) =>
        callId is not null && _aprobadas.TryGetValue(callId, out var e) ? e : null;
}
