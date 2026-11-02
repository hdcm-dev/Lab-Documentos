// Nuevo. Delta: Agregado por la IA — límite de concurrencia del modelo en la entrada HTTP, compartido por la API y la página de chat (REC-13; patrón del limitador de la línea archivada, adaptado a la composición de la demo).
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace MovilidadUrbana.Asistente.Concurrencia;

/// <summary>
/// Sección <c>Concurrencia</c> de la configuración (también por entorno: <c>Concurrencia__Permisos</c>…).
/// Valores elegidos, no medidos: un permiso porque el servidor de modelos atiende un pedido por vez, y
/// una cola corta para no dejar a nadie esperando un pedido largo sin saberlo.
/// </summary>
public sealed class OpcionesDeConcurrencia
{
    public const string Seccion = "Concurrencia";

    [Range(1, 64)] public int Permisos { get; set; } = 1;
    [Range(0, 64)] public int Cola { get; set; } = 2;
    [Range(1, 3_600)] public int ReintentarEnSegundos { get; set; } = 30;
}

/// <summary>
/// Un solo límite de pedidos simultáneos al modelo, compartido por las dos entradas: los controllers
/// que llaman al modelo (política <see cref="Politica" />, 503 con <c>Retry-After</c> al saturarse) y la
/// página de chat, que llama a <see cref="CorrerAsync" /> en el mismo proceso. No está dentro de la cadena
/// del <c>IChatClient</c>: limita en la entrada, antes de armar el pedido.
/// </summary>
public sealed class LimitadorDelModelo : IDisposable
{
    public const string Politica = "modelo";

    public LimitadorDelModelo(IOptions<OpcionesDeConcurrencia> opciones)
    {
        var o = opciones.Value;
        Limitador = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = o.Permisos,
            QueueLimit = o.Cola,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    public ConcurrencyLimiter Limitador { get; }

    /// <summary>
    /// El limitador visto por la partición del middleware. El middleware descarta y libera los limitadores
    /// que quedan ociosos un rato; esta vista no es dueña del limitador compartido, así que liberarla no lo
    /// cierra y la página sigue usando el mismo.
    /// </summary>
    public RateLimiter Vista() => new VistaSinDueno(Limitador);

    /// <summary>
    /// La entrada de la página: corre <paramref name="accion" /> con un permiso del mismo limitador que la
    /// API. Devuelve <c>false</c>, sin correrla, si el limitador está saturado.
    /// </summary>
    public async Task<bool> CorrerAsync(Func<Task> accion, CancellationToken cancellationToken = default)
    {
        using var permiso = await Limitador.AcquireAsync(1, cancellationToken);
        if (!permiso.IsAcquired) return false;
        await accion();
        return true;
    }

    public void Dispose() => Limitador.Dispose();

    private sealed class VistaSinDueno(RateLimiter interno) : RateLimiter
    {
        public override TimeSpan? IdleDuration => interno.IdleDuration;
        public override RateLimiterStatistics? GetStatistics() => interno.GetStatistics();
        protected override RateLimitLease AttemptAcquireCore(int permitCount) => interno.AttemptAcquire(permitCount);
        protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken) =>
            interno.AcquireAsync(permitCount, cancellationToken);
        protected override void Dispose(bool disposing) { }
        protected override ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;
    }
}

public static class LimitadorDelModeloExtensions
{
    public static IServiceCollection AddLimitadorDelModelo(this IServiceCollection services, IConfiguration configuracion)
    {
        services.AddOptions<OpcionesDeConcurrencia>()
            .Bind(configuracion.GetSection(OpcionesDeConcurrencia.Seccion))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<LimitadorDelModelo>();
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status503ServiceUnavailable;
            o.OnRejected = (contexto, _) =>
            {
                var segundos = contexto.HttpContext.RequestServices.GetRequiredService<IOptions<OpcionesDeConcurrencia>>().Value.ReintentarEnSegundos;
                contexto.HttpContext.Response.Headers.RetryAfter = segundos.ToString(CultureInfo.InvariantCulture);
                return ValueTask.CompletedTask;
            };
            // La partición devuelve siempre el mismo limitador, el que usa también la página, por una vista
            // que el middleware puede liberar sin cerrarlo.
            o.AddPolicy(LimitadorDelModelo.Politica, contexto =>
                RateLimitPartition.Get(LimitadorDelModelo.Politica,
                    _ => contexto.RequestServices.GetRequiredService<LimitadorDelModelo>().Vista()));
        });
        return services;
    }
}
