// Nuevo: el servidor MCP HTTP del ejemplo 13 (§8.9; H-013). Tres decisiones, y ninguna es control
// de acceso (el servidor NO autentica a nadie; por eso publica sólo lecturas):
//   1. Escucha SÓLO en loopback (IPAddress.Loopback, puerto que elige el sistema). Nada fuera de la
//      máquina lo alcanza.
//   2. AllowedHosts en loopback (HostFiltering): un Host que no es loopback recibe 400. Protege del
//      rebinding de DNS, no de un proceso local.
//   3. Un filtro que rechaza con 403 todo Origin que no sea loopback (MCP 2026-07-28, Streamable HTTP:
//      el servidor MUST validar Origin). Un pedido sin Origin (un cliente que no es navegador) pasa.
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MovilidadUrbana.Asistente.Infrastructure.Mcp;

public sealed class ServidorMcp : IAsyncDisposable
{
    public const string Ruta = "/mcp";

    /// <summary>Los únicos Host admitidos (sin puerto: HostFiltering compara sólo el nombre).</summary>
    public static readonly string[] HostsPermitidos = ["localhost", "127.0.0.1", "[::1]"];

    private readonly WebApplication _app;

    private readonly int[] _rechazos;

    private ServidorMcp(WebApplication app, Uri endpoint, IReadOnlyList<Uri> direcciones, int[] rechazos)
    {
        _app = app;
        _rechazos = rechazos;
        Endpoint = endpoint;
        Direcciones = direcciones;
    }

    /// <summary>La URL del endpoint MCP (loopback, puerto asignado al arrancar).</summary>
    public Uri Endpoint { get; }

    /// <summary>Todas las direcciones en las que quedó escuchando (la garantía las revisa).</summary>
    public IReadOnlyList<Uri> Direcciones { get; }

    /// <summary>Cuántos pedidos rechazó el filtro de Origin.</summary>
    public int RechazosPorOrigen => Volatile.Read(ref _rechazos[0]);

    /// <param name="bd">La base: el servidor lee de ella, siempre en la sesión fija.</param>
    /// <param name="guionado">true: publica además la escritura del servidor guionado (sólo pruebas de cableado).</param>
    public static async Task<ServidorMcp> IniciarAsync(BaseDeEjemplo bd, bool guionado = false, ContadorDeBorrados? contador = null,
        CancellationToken cancellationToken = default)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0));
        builder.Services.Configure<HostFilteringOptions>(o =>
        {
            o.AllowedHosts = [.. HostsPermitidos];
            o.AllowEmptyHosts = false;
        });
        builder.Services.AddSingleton(bd);
        builder.Services.AddSingleton(contador ?? new ContadorDeBorrados());

        var mcp = builder.Services.AddMcpServer().WithHttpTransport(o => o.Stateless = true).WithTools<LecturasMcp>();
        if (guionado) mcp.WithTools<HerramientasDelServidorGuionado>();

        var rechazos = new int[1];
        var app = builder.Build();
        app.UseHostFiltering();
        app.Use(async (contexto, siguiente) =>
        {
            if (!OrigenPermitido(contexto.Request.Headers.Origin.ToString()))
            {
                Interlocked.Increment(ref rechazos[0]);
                contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
            await siguiente(contexto);
        });
        app.MapMcp(Ruta);
        await app.StartAsync(cancellationToken);

        var direcciones = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!
            .Addresses.Select(a => new Uri(a)).ToList();
        return new ServidorMcp(app, new Uri(direcciones[0], Ruta), direcciones, rechazos);
    }

    /// <summary>
    /// Sin Origin (cliente que no es navegador): pasa. Con Origin: sólo si es una URL http(s) de
    /// loopback. «null», un esquema raro o cualquier otro host: no pasa.
    /// </summary>
    public static bool OrigenPermitido(string? origen)
    {
        if (string.IsNullOrEmpty(origen)) return true;
        return Uri.TryCreate(origen, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
               && uri.IsLoopback;
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
