// Nuevo (recuperación 2026-10-28, REC-12; patrón de la línea archivada del 2026-10-28, ver Procedencia).
// La caché de respuestas de M.E.AI arma la clave con los mensajes y las opciones, y nada más: dos sesiones
// que mandan el mismo texto comparten la respuesta, y una escritura confirmada no invalida lo guardado.
// Esta capa suma a la clave la sesión y la versión de los datos, leídas EN CADA PEDIDO, y cuenta los
// aciertos por los dos caminos de lectura.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;

namespace MovilidadUrbana.Asistente.Infrastructure;

/// <summary>
/// Lo que la caché tiene que leer en cada pedido para armar la clave: la sesión que pide y la versión de
/// los datos. En una aplicación, la sesión sale del ámbito del pedido (o del circuito) y la versión es un
/// singleton que sube con cada escritura confirmada; el ejemplo las junta en un objeto para las pruebas.
/// </summary>
public sealed class ContextoDeLaCache(string sesionId)
{
    private long _versionDeDatos;

    /// <summary>La sesión del pedido en curso (la frontera entre visitantes).</summary>
    public string SesionId { get; set; } = sesionId;

    /// <summary>Sube con cada escritura confirmada de los datos que la respuesta pudo leer.</summary>
    public long VersionDeDatos => Interlocked.Read(ref _versionDeDatos);

    public void RegistrarEscritura() => Interlocked.Increment(ref _versionDeDatos);

    /// <summary>Los valores que se suman a la clave: <c>[SessionId, VersionDeDatos]</c>, leídos al llamar.</summary>
    public object?[] ValoresDeLaClave() => [SesionId, VersionDeDatos];
}

/// <summary>
/// <see cref="DistributedCachingChatClient" /> con la sesión y la versión de los datos en la clave, y con un
/// contador de aciertos.
/// <list type="bullet">
/// <item>La clave suma lo que devuelve <paramref name="valoresDeLaClave" /> en cada pedido. Leer los valores
/// al crear la instancia deja la versión vieja: el acierto sirve datos de antes de la escritura.</item>
/// <item>Se sobrescriben los DOS métodos de lectura. En streaming, con <c>CoalesceStreamingUpdates</c> en
/// <c>true</c> (el valor por defecto) se lee por <c>ReadCacheAsync</c>; con <c>false</c>, por
/// <c>ReadCacheStreamingAsync</c>. Un contador que mira sólo el primero da 0 aciertos en el segundo caso.
/// En cada acierto se llama uno solo de los dos: no hay doble conteo.</item>
/// <item>Un acierto no llama a nada de abajo: tampoco vuelve a ejecutar herramientas. Con herramientas sobre
/// datos que cambian, sin la versión en la clave, la respuesta guardada queda vieja.</item>
/// </list>
/// </summary>
public sealed class CacheConContador(IChatClient interno, IDistributedCache cache, Func<object?[]> valoresDeLaClave)
    : DistributedCachingChatClient(interno, cache)
{
    private long _aciertos;

    /// <summary>Aciertos de caché, por cualquiera de los dos caminos de lectura.</summary>
    public long Aciertos => Interlocked.Read(ref _aciertos);

    protected override string GetCacheKey(IEnumerable<ChatMessage> messages, ChatOptions? options, params ReadOnlySpan<object?> additionalValues)
        => base.GetCacheKey(messages, options, [.. additionalValues, .. valoresDeLaClave()]);

    protected override async Task<ChatResponse?> ReadCacheAsync(string key, CancellationToken cancellationToken)
    {
        var respuesta = await base.ReadCacheAsync(key, cancellationToken);
        if (respuesta is not null) Interlocked.Increment(ref _aciertos);
        return respuesta;
    }

    protected override async Task<IReadOnlyList<ChatResponseUpdate>?> ReadCacheStreamingAsync(string key, CancellationToken cancellationToken)
    {
        var fragmentos = await base.ReadCacheStreamingAsync(key, cancellationToken);
        if (fragmentos is not null) Interlocked.Increment(ref _aciertos);
        return fragmentos;
    }
}
