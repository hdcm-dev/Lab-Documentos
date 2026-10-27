using System.Text.Json;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Puente;
using Microsoft.JSInterop;

namespace FrameworkConcept.Content.Bridge;

/// <summary>
/// Cliente del puente, lado contenido. Toda llamada al cascarón pasa por acá.
///
/// P3 — El cascarón expone UN solo método nativo, <c>Call(sobre)</c>. Este cliente arma los parámetros
/// en C#, y <c>wwwroot/js/puente.js</c> los mete en un sobre JSON-RPC 2.0 con «v» y los entrega a
/// <c>window.HybridWebView.InvokeDotNet</c>. La respuesta vuelve como sobre: con <c>result</c> o con <c>error</c>.
///
/// Las páginas no llaman a este cliente con nombres de método sueltos: usan los métodos con tipo de abajo,
/// uno por fila de la tabla de métodos (<see cref="TablaDeMetodos"/>).
///
/// Es «partial» desde el puente 1.1 (folio 038): las filas de red y acciones están en Red/BridgeClient.Red.cs y las de
/// sesión en Auth/BridgeClient.Auth.cs, para que cada parte del incremento tenga su archivo.
/// </summary>
public sealed partial class BridgeClient(IJSRuntime js)
{
    /// <summary>
    /// La versión del puente contra la que se escribió este contenido. Viaja en el campo «v» de cada sobre:
    /// el cascarón rechaza un mayor distinto del suyo (<see cref="CodigosDeError.VersionDelPuenteIncompatible"/>).
    /// </summary>
    public static VersionSemantica VersionEsperada => ContratoDelPuente.Version;

    /// <summary>Llama a un método de la tabla y devuelve el objeto «result» tal cual.</summary>
    /// <exception cref="BridgeException">El cascarón contestó con «error», o no hay puente.</exception>
    public async Task<JsonElement> CallAsync(string metodo, object? parametros = null)
    {
        var parametrosJson = parametros is null ? null : JsonSerializer.Serialize(parametros, JsonDelContrato.Opciones);

        string respuestaJson;
        try
        {
            respuestaJson = await js.InvokeAsync<string>("puente.llamar", VersionEsperada.ToString(), metodo, parametrosJson);
        }
        catch (JSException excepcion) when (excepcion.Message.Contains(BridgeException.MarcaSinPuente, StringComparison.Ordinal))
        {
            throw BridgeException.SinPuente();
        }

        var respuesta = JsonDelContrato.Leer<SobreRespuesta>(respuestaJson);
        if (respuesta.Error is { } error) throw new BridgeException(metodo, error.Code, error.Message);
        return respuesta.Result ?? default;
    }

    /// <summary>Como <see cref="CallAsync(string, object?)"/>, leyendo «result» como <typeparamref name="T"/>.</summary>
    public async Task<T> CallAsync<T>(string metodo, object? parametros = null)
    {
        var resultado = await CallAsync(metodo, parametros);
        return resultado.Deserialize<T>(JsonDelContrato.Opciones)
               ?? throw new BridgeException(metodo, CodigosDeError.ErrorInterno, "La respuesta llegó vacía.");
    }

    // --- Una función por fila de la tabla de métodos ------------------------------------------------------

    public Task<RespuestaDeHandshake> HandshakeAsync() =>
        CallAsync<RespuestaDeHandshake>("bridge.handshake");

    /// <summary>«Listo(version)»: borra la marca «pendiente» del cascarón (P2, rollback local).</summary>
    public async Task<bool> ReadyAsync(VersionSemantica versionContenido) =>
        (await CallAsync("bridge.ready", new { version = versionContenido.ToString() })).GetProperty("aceptado").GetBoolean();

    public async Task<Documento?> GetDocumentAsync(string coleccion, string id)
    {
        var resultado = await CallAsync("documents.get", new { coleccion, id });
        return resultado.TryGetProperty("documento", out var documento) && documento.ValueKind == JsonValueKind.Object
            ? documento.Deserialize<Documento>(JsonDelContrato.Opciones)
            : null;
    }

    public async Task<IReadOnlyList<Documento>> ListDocumentsAsync(string coleccion) =>
        (await CallAsync("documents.list", new { coleccion })).GetProperty("documentos")
            .Deserialize<List<Documento>>(JsonDelContrato.Opciones) ?? [];

    /// <summary>
    /// Crea o reemplaza un documento. Con <paramref name="versionEsperada"/>, el cascarón rechaza el cambio si
    /// otro lo modificó antes (D-04: versión + conflicto). Devuelve la versión nueva del documento.
    /// </summary>
    public async Task<int> PutDocumentAsync(string coleccion, string id, int versionDeForma, object json, int? versionEsperada = null) =>
        (await CallAsync("documents.put", new { coleccion, id, versionDeForma, json, versionEsperada }))
            .GetProperty("version").GetInt32();

    public async Task<bool> DeleteDocumentAsync(string coleccion, string id) =>
        (await CallAsync("documents.delete", new { coleccion, id })).GetProperty("borrado").GetBoolean();

    public async Task<Core.Contracts.Encuestas.DefinicionEncuesta> GetDefinicionAsync(string encuestaId) =>
        (await CallAsync("encuestas.getDefinicion", new { encuestaId })).GetProperty("definicion")
            .Deserialize<Core.Contracts.Encuestas.DefinicionEncuesta>(JsonDelContrato.Opciones)
        ?? throw new BridgeException("encuestas.getDefinicion", CodigosDeError.ErrorInterno, "Sin definición.");

    /// <summary>P5, P13 — Guarda la respuesta y su fila del outbox en una sola transacción del cascarón.</summary>
    public Task<RespuestaDeEncolado> EnqueueRespuestaAsync(Core.Contracts.Encuestas.EnvioDeRespuesta envio) =>
        CallAsync<RespuestaDeEncolado>("encuestas.enqueueRespuesta", new { envio });

    public Task<EstadoDelOutbox> OutboxStatusAsync() => CallAsync<EstadoDelOutbox>("outbox.status");

    public Task<InformacionDelDispositivo> DeviceInfoAsync() => CallAsync<InformacionDelDispositivo>("device.info");

    /// <summary>
    /// «conectado» como en el puente 1.0. El estado combinado (1.1) lo lee <c>NetworkStatusCombinadoAsync</c>
    /// (Red/BridgeClient.Red.cs).
    /// </summary>
    public async Task<bool> NetworkStatusAsync() =>
        (await CallAsync("network.status")).GetProperty("conectado").GetBoolean();

    // --- Eventos del cascarón -----------------------------------------------------------------------------

    /// <summary>
    /// Registra a <paramref name="receptor"/> para los eventos que el cascarón envía con
    /// <c>HybridWebView.InvokeJavaScriptAsync</c> (network.changed, outbox.changed).
    /// </summary>
    public ValueTask SubscribeAsync<T>(DotNetObjectReference<T> receptor) where T : class =>
        js.InvokeVoidAsync("puente.suscribir", receptor);

    /// <summary>Marca de rendimiento «primer-render» (P1: la métrica TPR se toma desde esta marca).</summary>
    public ValueTask MarkFirstRenderAsync(VersionSemantica versionContenido) =>
        js.InvokeVoidAsync("puente.marcarPrimerRender", versionContenido.ToString());
}

/// <summary>Respuesta de <c>encuestas.enqueueRespuesta</c>.</summary>
public sealed record RespuestaDeEncolado(string ClaveIdempotencia, bool Encolada);

/// <summary>Respuesta de <c>outbox.status</c> y parámetros del evento <c>outbox.changed</c>.</summary>
public sealed record EstadoDelOutbox(int Pendientes, int Rechazadas);

/// <summary>Respuesta de <c>device.info</c>.</summary>
public sealed record InformacionDelDispositivo(string Fabricante, string Modelo, string Plataforma, string VersionSistema);
