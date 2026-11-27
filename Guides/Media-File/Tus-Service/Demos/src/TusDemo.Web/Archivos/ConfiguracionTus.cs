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
            // Sin terminación (DELETE): borrar una suba completa y registrada dejaría una fila sin archivo.
            AllowedExtensions = TusExtensions.All.Except(TusExtensions.Termination),
            Events = new Events
            {
                // 1) Primero en todo pedido tus: ¿quién pide?
                //    OPTIONS (GetOptions) queda libre: es el pedido de descubrimiento del protocolo,
                //    que la especificación exime de Tus-Resumable y que los clientes mandan sin credenciales.
                OnAuthorizeAsync = ctx =>
                {
                    if (ctx.Intent == IntentType.GetOptions) return Task.CompletedTask;
                    if (ctx.HttpContext.User.Identity?.IsAuthenticated != true)
                        ctx.FailRequest(HttpStatusCode.Unauthorized);
                    return Task.CompletedTask;
                },

                // 2) Antes de crear la suba: ¿trae los metadatos, una extensión permitida y un filetype válido?
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
                    {
                        ctx.FailRequest($"Extensión no permitida: '{extension}'");
                        return Task.CompletedTask;
                    }
                    // filetype se devuelve después como Content-Type de la descarga: tiene que ser un tipo válido.
                    var tipo = ctx.Metadata["filetype"].GetString(Encoding.UTF8);
                    if (!System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(tipo, out _))
                        ctx.FailRequest($"filetype no es un tipo de contenido válido: '{tipo}'");
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
