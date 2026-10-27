using FrameworkConcept.Core.Update;

namespace FrameworkConcept.Shell.Actualizacion;

// Los adaptadores de Android para el actualizador de Core. El actualizador es «puro» (P2): no conoce MAUI. El cascarón
// le da el reloj, la red y el canal por estas tres clases cortas.

/// <summary>El reloj del sistema, en UTC.</summary>
public sealed class RelojDelSistema : IReloj
{
    public DateTimeOffset Ahora => DateTimeOffset.UtcNow;
}

/// <summary>¿Hay internet ahora? Lo lee MAUI Essentials (necesita ACCESS_NETWORK_STATE, que el catálogo declara).</summary>
public sealed class EstadoDeRedMaui : IEstadoDeRed
{
    public bool HayRed => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
}

/// <summary>
/// El servidor estático del canal por HTTPS (P2: sólo HTTPS). Cualquier falla de red o un código HTTP de error se
/// informa como <see cref="CanalNoDisponibleException"/>: para el actualizador, «no se pudo bajar» nunca es un rechazo,
/// y lo instalado sigue funcionando.
/// </summary>
public sealed class ClienteDeCanalHttp : IClienteDeCanal, IDisposable
{
    private readonly HttpClient _http;
    private readonly bool _esHttps;

    public ClienteDeCanalHttp(ConfiguracionDelCascaron configuracion, FabricaHttp fabrica)
    {
        _esHttps = configuracion.CanalUrl.Scheme == Uri.UriSchemeHttps;
        // Folio 038 — Por ManejadorDeRed (borrador §6 «Actividad»): sin token, el canal es público y firmado (P2).
        _http = fabrica.CrearParaCanal(configuracion.CanalUrl, TimeSpan.FromSeconds(30));
    }

    public async Task<byte[]> ObtenerBytesAsync(string ruta, CancellationToken cancellationToken = default)
    {
        using var respuesta = await EnviarAsync(ruta, cancellationToken);
        return await respuesta.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<Stream> AbrirAsync(string ruta, CancellationToken cancellationToken = default)
    {
        var respuesta = await EnviarAsync(ruta, cancellationToken);
        return await respuesta.Content.ReadAsStreamAsync(cancellationToken);
    }

    private async Task<HttpResponseMessage> EnviarAsync(string ruta, CancellationToken ct)
    {
        if (!_esHttps)
            throw new CanalNoDisponibleException($"el canal {_http.BaseAddress} no es HTTPS (P2); no se consulta");

        Bitacora.Log($"canal: GET {ruta}");
        try
        {
            var respuesta = await _http.GetAsync(ruta, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!respuesta.IsSuccessStatusCode)
            {
                respuesta.Dispose();
                throw new CanalNoDisponibleException($"{ruta}: HTTP {(int)respuesta.StatusCode}");
            }
            return respuesta;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
        {
            throw new CanalNoDisponibleException($"{ruta}: {ex.Message}", ex);
        }
    }

    public void Dispose() => _http.Dispose();
}
