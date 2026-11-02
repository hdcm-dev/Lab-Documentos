// Nuevo (plan §E.1, «Timeouts y reintentos por adaptador»; H-056). Un HttpClient propio con 10 min
// de timeout y sin handler de resiliencia: un reintento contra un servidor en CPU duplica la carga.
// REC-22: el único handler es el lector del prefijo cacheado, que no reintenta ni guarda cuerpos.
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace MovilidadUrbana.Asistente.Comun.AI;

public static class FabricaDeClientes
{
    public static readonly TimeSpan TimeoutDelModelo = TimeSpan.FromMinutes(10);

    /// <summary>Cliente de chat OllamaSharp ya envuelto en la capa de opciones.</summary>
    public static IChatClient CrearChatOllama(string endpoint, string modelo, ConfiguracionDelModelo configuracion) =>
        Envolver(new OllamaApiClient(CrearHttp(endpoint), modelo), configuracion, Adaptador.OllamaSharp);

    /// <summary>
    /// Envuelve cualquier <see cref="IChatClient" /> base (OllamaSharp, SDK OpenAI, el guionado del
    /// modo --fake) con la capa única. Así el modo falso pasa por las mismas opciones que el real.
    /// </summary>
    public static IChatClient Envolver(IChatClient interno, ConfiguracionDelModelo configuracion, Adaptador adaptador) =>
        new ChatClientBuilder(new LecturaDelServidorEnLaRespuesta(interno)).UseOpcionesDelProveedor(configuracion, adaptador).Build();

    /// <summary>Generador de embeddings OllamaSharp (el mismo tipo implementa los dos contratos).</summary>
    public static IEmbeddingGenerator<string, Embedding<float>> CrearEmbeddingsOllama(string endpoint, string modelo) =>
        new OllamaApiClient(CrearHttp(endpoint), modelo);

    /// <summary>
    /// HttpClient del proveedor con el lector de la línea final de /api/chat (REC-22): sólo mira
    /// <c>prompt_eval_cached_count</c> cuando hay una lectura abierta y no guarda el cuerpo.
    /// </summary>
    public static HttpClient CrearHttp(string endpoint) =>
        new(new LectorDePrefijoCacheado { InnerHandler = new SocketsHttpHandler() }) { BaseAddress = new Uri(endpoint), Timeout = TimeoutDelModelo };
}
