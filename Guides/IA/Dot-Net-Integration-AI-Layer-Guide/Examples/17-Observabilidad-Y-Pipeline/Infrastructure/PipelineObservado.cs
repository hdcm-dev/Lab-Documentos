// Nuevo (ejemplo 17; plan §E.2: «UseOpenTelemetry, UseLogging, UseDistributedCache; el orden de las
// capas y lo que ve la telemetría; EnableSensitiveData sólo en desarrollo»). Patrón de N5 §4.1 y §4.7,
// con la caché pasada como instancia (sin DI, .UseDistributedCache() sin argumento lanza: N5 V4).
// Modificado (recuperación 2026-10-28, REC-12): la caché ya no usa la clave por defecto de
// UseDistributedCache; va CacheConContador, con la sesión y la versión de los datos en la clave.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MovilidadUrbana.Asistente.Infrastructure;

/// <summary>Qué capa queda más afuera (la primera que recibe el pedido).</summary>
public enum OrdenDeCapas
{
    /// <summary>caché → telemetría → registro → capa de opciones → adaptador (N5 E2).</summary>
    CacheAfuera,

    /// <summary>telemetría → caché → registro → capa de opciones → adaptador (N5 V3).</summary>
    TelemetriaAfuera
}

public static class Ambientes
{
    public const string Desarrollo = "Development";
    public const string Produccion = "Production";

    /// <summary>El ambiente del proceso, como lo lee .NET (DOTNET_ENVIRONMENT); sin la variable, producción.</summary>
    public static string DelProceso() =>
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") is { Length: > 0 } a ? a : Produccion;

    /// <summary>Contenido (mensajes e instrucciones) en la telemetría: sólo en desarrollo (N5 §4.7, P2).</summary>
    public static bool ContenidoEnTelemetria(string ambiente) =>
        string.Equals(ambiente, Desarrollo, StringComparison.OrdinalIgnoreCase);
}

public static class PipelineObservado
{
    public static IDistributedCache CacheEnMemoria() =>
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    /// <summary>
    /// Arma el pipeline sobre <paramref name="conCapaUnica" /> (un cliente que ya salió de
    /// FabricaDeClientes, con la capa de opciones pegada al adaptador). Entre los dos queda
    /// <paramref name="registrador" />: cuenta lo que de verdad llega al modelo.
    /// <paramref name="valoresDeLaClave" /> se lee en cada pedido y se suma a la clave de la caché
    /// (<see cref="ContextoDeLaCache.ValoresDeLaClave" />: sesión y versión de los datos).
    /// </summary>
    public static IChatClient Construir(
        IChatClient conCapaUnica, OrdenDeCapas orden, IDistributedCache cache, ILoggerFactory logs,
        string ambiente, RegistradorDeLlamadas registrador, Func<object?[]> valoresDeLaClave)
    {
        ArgumentNullException.ThrowIfNull(conCapaUnica);
        ArgumentNullException.ThrowIfNull(registrador);
        ArgumentNullException.ThrowIfNull(valoresDeLaClave);
        if (!ReferenceEquals(registrador.Interno, conCapaUnica))
        {
            throw new ArgumentException("El registrador tiene que envolver al cliente de la capa única.", nameof(registrador));
        }

        var contenido = Ambientes.ContenidoEnTelemetria(ambiente);
        // Se fija la propiedad siempre: gana sobre OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT (N5 F06).
        void Telemetria(OpenTelemetryChatClient c) => c.EnableSensitiveData = contenido;
        // La clave por defecto (mensajes y opciones) cruza sesiones y sirve datos viejos: REC-12.
        IChatClient Cache(IChatClient interno) => new CacheConContador(interno, cache, valoresDeLaClave);

        var builder = new ChatClientBuilder(registrador);
        // En ChatClientBuilder, la primera capa que se agrega es la de más afuera.
        builder = orden switch
        {
            OrdenDeCapas.CacheAfuera => builder.Use(Cache).UseOpenTelemetry(configure: Telemetria),
            OrdenDeCapas.TelemetriaAfuera => builder.UseOpenTelemetry(configure: Telemetria).Use(Cache),
            _ => throw new ArgumentOutOfRangeException(nameof(orden))
        };
        return builder.UseLogging(logs).Build();
    }
}
