// Procedencia: Lab-E2E.WebBlazor/tests/MovilidadUrbana.ApiWeb.Tests/ApiWebApplicationFactory.cs @ 8fb4c3a
// Delta: Modificado a causa de la IA — sin servidor de modelos: EMB-C, endpoints a un puerto cerrado y un cliente guionado (con espía debajo de la capa de opciones) en lugar del cliente real; ValidateScopes y ValidateOnBuild encendidos; ya no es sealed (las pruebas con modelo la extienden); Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Tests.Cableado;

namespace MovilidadUrbana.Asistente.Tests;

/// <summary>
/// Levanta el host en proceso sobre una base SQLite propia de la corrida, para que las pruebas no
/// toquen `datos/` ni se pisen entre corridas. Sin servidor de modelos: el cliente base es
/// <see cref="Guion" />, envuelto por la misma capa de opciones que el real, con <see cref="Espia" />
/// debajo para ver lo que saldría hacia el servidor.
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string PuertoCerrado = "http://127.0.0.1:9";
    private readonly string _archivo = Path.Combine(Path.GetTempPath(), $"movilidad-api-{Guid.NewGuid():n}.db");
    private readonly Dictionary<string, string?> _ajustes;

    public ApiWebApplicationFactory(IDictionary<string, string?>? ajustes = null)
    {
        _ajustes = new Dictionary<string, string?>
        {
            ["EMB_PLAN"] = "EMB-C",
            ["OLLAMA_ENDPOINT"] = PuertoCerrado,
            ["OLLAMA_PROPIO_ENDPOINT"] = PuertoCerrado,
            ["OLLAMA_EMBED_ENDPOINT"] = PuertoCerrado,
            ["OLLAMA_SERVIDOR"] = "propio"
        };
        foreach (var (clave, valor) in ajustes ?? new Dictionary<string, string?>()) _ajustes[clave] = valor;
    }

    /// <summary>Cliente guionado intercambiable: cada prueba le pone su guion.</summary>
    public ClienteIntercambiable Guion { get; } = new();

    /// <summary>Espía debajo de la capa de opciones: las opciones efectivas de cada llamada.</summary>
    public ClienteEspia Espia { get; private set; } = default!;

    /// <summary>true: el cliente base es el del host (las pruebas con modelo lo reemplazan en <see cref="ConfigurarCliente" />).</summary>
    protected virtual bool ClienteReal => false;

    protected virtual void ConfigurarCliente(IServiceCollection services)
    {
        Espia = new ClienteEspia(Guion);
        var config = ConfiguracionDelModelo.DesdeEntorno(k => _ajustes.TryGetValue(k, out var v) ? v : null);
        services.AddSingleton<IChatClient>(FabricaDeClientes.Envolver(Espia, config, Adaptador.OllamaSharp));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:BaseDeDatos", $"Data Source={_archivo};Default Timeout=30");
        foreach (var (clave, valor) in _ajustes) builder.UseSetting(clave, valor);
        builder.ConfigureTestServices(ConfigurarCliente);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseDefaultServiceProvider(o => { o.ValidateScopes = true; o.ValidateOnBuild = true; });
        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var f in new[] { _archivo, _archivo + "-wal", _archivo + "-shm" }) File.Delete(f);
    }
}
