using System.Text.Json;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Contenido;
using FrameworkConcept.Core.Contracts.Puente;
using Microsoft.JSInterop;

namespace FrameworkConcept.Content.Bridge;

/// <summary>En qué quedó el encuentro con el cascarón.</summary>
public enum EstadoDelPuente
{
    Conectando,
    /// <summary>Handshake hecho y compatible: el contenido puede trabajar.</summary>
    Listo,
    /// <summary>No hay window.HybridWebView: el contenido se abrió fuera del cascarón.</summary>
    SinPuente,
    /// <summary>El puente o una capacidad de «requires» no es la que este contenido necesita.</summary>
    Incompatible
}

/// <summary>Si una capacidad opcional se puede usar y, si no, por qué (para explicarlo en pantalla).</summary>
public sealed record Disponibilidad(bool Disponible, string Motivo);

/// <summary>
/// La sesión del contenido con el cascarón: hace el handshake una sola vez, guarda lo que el cascarón
/// informó (versión del puente, catálogo, huella del contrato), avisa «Listo» tras el primer render y
/// recibe los eventos. Las páginas la leen para decidir qué mostrar.
/// </summary>
public sealed class BridgeSession(BridgeClient puente) : IDisposable
{
    private Task? _inicio;
    private bool _listoAvisado;
    private DotNetObjectReference<BridgeSession>? _referencia;

    public EstadoDelPuente Estado { get; private set; } = EstadoDelPuente.Conectando;

    /// <summary>Por qué el estado no es <see cref="EstadoDelPuente.Listo"/>.</summary>
    public string? Motivo { get; private set; }

    public RespuestaDeHandshake? Handshake { get; private set; }

    /// <summary>null mientras no se sepa, o si el cascarón no trae «network.status» (capacidad opcional).</summary>
    public bool? Conectado { get; private set; }

    public EstadoDelOutbox? Outbox { get; private set; }

    /// <summary>Se dispara cuando cambia algo que las pantallas muestran.</summary>
    public event Action? Cambio;

    /// <summary>
    /// Folio 038 — Todo evento del cascarón, tal cual llega (método y parámetros). Lo escuchan los servicios de
    /// Red/ y Auth/ (network.changed con estado, actions.changed, auth.changed) sin tocar esta clase.
    /// </summary>
    public event Action<string, JsonElement>? EventoRecibido;

    /// <summary>Hace el handshake la primera vez; las siguientes devuelven la misma tarea.</summary>
    public Task IniciarAsync() => _inicio ??= HacerHandshakeAsync();

    private async Task HacerHandshakeAsync()
    {
        try
        {
            Handshake = await puente.HandshakeAsync();
        }
        catch (BridgeException excepcion) when (excepcion.EsSinPuente)
        {
            Estado = EstadoDelPuente.SinPuente;
            Motivo = excepcion.Message;
            return;
        }
        catch (BridgeException excepcion) when (excepcion.Code == CodigosDeError.VersionDelPuenteIncompatible)
        {
            Estado = EstadoDelPuente.Incompatible;
            Motivo = excepcion.Message;
            return;
        }

        // P3 — El actualizador ya evaluó el predicado antes de activar este paquete. Se repite acá porque es
        // barato y porque el contenido puede haberse abierto por otro camino (la semilla, una prueba).
        if (!ContentInfo.Declaracion.CompatibleConPuente.Contiene(Handshake.VersionPuente))
        {
            Estado = EstadoDelPuente.Incompatible;
            Motivo = $"El puente del cascarón es {Handshake.VersionPuente}; este contenido necesita {ContentInfo.Declaracion.CompatibleConPuente}.";
            return;
        }
        var faltante = ContentInfo.Declaracion.Capabilities.Requires.FirstOrDefault(r => !Evaluar(r).Disponible);
        if (faltante is not null)
        {
            Estado = EstadoDelPuente.Incompatible;
            Motivo = Evaluar(faltante).Motivo;
            return;
        }

        Estado = EstadoDelPuente.Listo;

        _referencia = DotNetObjectReference.Create(this);
        await puente.SubscribeAsync(_referencia);
        await RefrescarAsync();
    }

    /// <summary>Vuelve a leer la red (si la capacidad está) y el outbox.</summary>
    public async Task RefrescarAsync()
    {
        if (Estado != EstadoDelPuente.Listo) return;

        if (Capacidad("network.status").Disponible) Conectado = await puente.NetworkStatusAsync();
        Outbox = await puente.OutboxStatusAsync();
        Cambio?.Invoke();
    }

    /// <summary>
    /// P2 — «Listo(version)»: se llama después del primer render con el puente sano. Si el contenido no
    /// llega acá (se colgó, o es incompatible), la marca «pendiente» queda y el próximo arranque en frío
    /// vuelve a la versión anterior. Por eso un contenido incompatible NO avisa listo.
    /// </summary>
    public async Task AvisarListoAsync()
    {
        if (_listoAvisado) return;
        _listoAvisado = true;

        await puente.MarkFirstRenderAsync(ContentInfo.Version);
        // P2 — La variante defectuosa de la demo (ContenidoSinListo=true) se dibuja pero no avisa «Listo».
#if !CONTENIDO_SIN_LISTO
        if (Estado == EstadoDelPuente.Listo) await puente.ReadyAsync(ContentInfo.Version);
#endif
    }

    /// <summary>
    /// P12 — ¿Se puede usar esta capacidad? Se pregunta por id; la capacidad tiene que estar declarada en
    /// declaracion-del-contenido.json (requires u optional), que es donde está su rango.
    /// </summary>
    public Disponibilidad Capacidad(string id)
    {
        var declarada = ContentInfo.Declaracion.Capabilities.Requires
            .Concat(ContentInfo.Declaracion.Capabilities.Optional)
            .FirstOrDefault(r => r.Id == id)
            ?? throw new InvalidOperationException($"«{id}» no está en declaracion-del-contenido.json: declarala antes de usarla.");
        return Evaluar(declarada);
    }

    private Disponibilidad Evaluar(RequisitoDeCapacidad requisito)
    {
        if (Handshake is null) return new(false, "Todavía no se habló con el cascarón.");

        var entrada = Handshake.Catalogo.FirstOrDefault(e => e.Id == requisito.Id);
        if (entrada is null)
            return new(false, $"Esta versión de la app no trae «{requisito.Id}». La función aparece cuando una versión de la app (por tienda) la incorpore.");
        if (!requisito.Rango.Contiene(entrada.Version))
            return new(false, $"La app trae «{requisito.Id}» {entrada.Version}; esta pantalla necesita {requisito.Rango}.");
        return new(true, string.Empty);
    }

    // --- Eventos: los llama puente.js cuando el cascarón envía un sobre sin id ------------------------------

    [JSInvokable]
    public void RecibirEvento(string metodo, string? parametrosJson)
    {
        var parametros = string.IsNullOrEmpty(parametrosJson) ? default : JsonDocument.Parse(parametrosJson).RootElement;
        EventoRecibido?.Invoke(metodo, parametros);

        switch (metodo)
        {
            case "network.changed":
                Conectado = parametros.GetProperty("conectado").GetBoolean();
                break;
            case "outbox.changed":
                Outbox = parametros.Deserialize<EstadoDelOutbox>(JsonDelContrato.Opciones);
                break;
            default:
                // Un evento que este contenido no conoce se ignora: el cascarón puede ser más nuevo (P3, menor).
                return;
        }
        Cambio?.Invoke();
    }

    public void Dispose() => _referencia?.Dispose();
}
