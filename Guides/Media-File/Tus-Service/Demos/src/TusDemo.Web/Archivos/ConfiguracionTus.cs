using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

namespace TusDemo.Web.Archivos;

/// <summary>Arma la configuración del endpoint tus para cada pedido.</summary>
public static class ConfiguracionTus
{
    public static Task<DefaultTusConfiguration> CrearAsync(HttpContext httpContext)
    {
        var servicios = httpContext.RequestServices;
        var opciones = servicios.GetRequiredService<IOptions<OpcionesArchivos>>().Value;
        var almacen = servicios.GetRequiredService<TusDiskStore>();
        var log = servicios.GetRequiredService<ILoggerFactory>().CreateLogger("Tus");
        var extensiones = new HashSet<string>(opciones.Extensiones, StringComparer.OrdinalIgnoreCase);

        var configuracion = new DefaultTusConfiguration
        {
            Store = almacen,
            MaxAllowedUploadSizeInBytesLong = opciones.TopeBytes,
            Expiration = new SlidingExpiration(TimeSpan.FromHours(opciones.HorasDeVencimiento)),
            Events = new Events
            {
                // 1) Primero en todo pedido tus: ¿quién pide?
                //    OPTIONS (GetOptions) queda libre: el navegador lo manda sin credenciales
                //    para descubrir el servidor y en el preflight de CORS.
                OnAuthorizeAsync = ctx =>
                {
                    if (ctx.Intent == IntentType.GetOptions) return Task.CompletedTask;
                    if (ctx.HttpContext.User.Identity?.IsAuthenticated != true)
                        ctx.FailRequest(HttpStatusCode.Unauthorized);
                    return Task.CompletedTask;
                },

                // 2) Antes de crear la suba: ¿trae los metadatos y una extensión permitida?
                OnBeforeCreateAsync = ctx =>
                {
                    if (!ctx.Metadata.ContainsKey("filename") || !ctx.Metadata.ContainsKey("filetype"))
                    {
                        ctx.FailRequest("Faltan los metadatos filename y filetype");
                        return Task.CompletedTask;
                    }
                    var nombre = ctx.Metadata["filename"].GetString(Encoding.UTF8);
                    var extension = Path.GetExtension(nombre);
                    if (!extensiones.Contains(extension))
                        ctx.FailRequest($"Extensión no permitida: '{extension}'");
                    return Task.CompletedTask;
                },

                // 3) Una sola vez, cuando llega el último byte.
                OnFileCompleteAsync = async ctx =>
                {
                    var archivo = await ctx.GetFileAsync();
                    log.LogInformation("Suba completa {Id}", archivo.Id);
                    // Acá se encolaría un antivirus o la generación de miniaturas.
                }
            }
        };
        return Task.FromResult(configuracion);
    }
}
