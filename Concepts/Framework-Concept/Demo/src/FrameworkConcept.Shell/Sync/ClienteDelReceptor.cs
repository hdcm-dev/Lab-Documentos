using System.Net;
using System.Text;
using FrameworkConcept.Shell.Data;

namespace FrameworkConcept.Shell.Sync;

/// <summary>Cómo terminó un envío: el sincronizador decide con esto si la fila sale, se reintenta o va a carta muerta.</summary>
public enum ResultadoDeEnvio
{
    Aceptada,

    /// <summary>Red caída, tiempo agotado, 408, 429 o 5xx: se reintenta con backoff.</summary>
    Reintentar,

    /// <summary>El servidor dijo que no (412 u otro 4xx): carta muerta, visible, nunca borrada.</summary>
    Rechazada
}

/// <summary>
/// Envía una fila del outbox al receptor (en la demo, el receptor didáctico de tools/FrameworkConcept.Receptor).
///
/// Lo que el cascarón manda por cada respuesta:
///   POST {ReceptorUrl}/respuestas
///   Content-Type: application/json          cuerpo = EnvioDeRespuesta (Core/Contracts/Encuestas)
///   Idempotency-Key: {claveIdempotencia}    el receptor deduplica por ella: reenviar no duplica (P13.6)
///   If-Match: "{versionEsperada}"           sólo si la acción modifica algo existente (D-04: versión + 412)
/// Respuestas: 2xx = aceptada (también la repetida); 412 = conflicto de versión; otro 4xx = rechazo (por ejemplo, una
/// versión de definición retirada, P13.5); 408, 429 y 5xx = reintentar.
/// Folio 038 — Con sesión OIDC, el pedido lleva «Authorization: Bearer» (ManejadorDeSesion). Un 401 NO es un rechazo
/// de la fila: la sesión dejó de valer; se trata como «reintentar» (DISENO §3.5).
/// </summary>
public sealed class ClienteDelReceptor(ConfiguracionDelCascaron configuracion, FabricaHttp fabrica)
{
    public const string AccionCrearRespuesta = "respuestas.crear";

    // Folio 038 — Por la cadena de FabricaHttp: Bearer de la sesión (P4) y registro + estado de red (borrador §6).
    private readonly HttpClient _http = fabrica.CrearParaReceptor(configuracion.ReceptorUrl, TimeSpan.FromSeconds(20));

    /// <param name="token">Corrector E03 — El access token del dueño de la fila (null en modo Dispositivo).</param>
    public async Task<(ResultadoDeEnvio Resultado, string Detalle)> EnviarAsync(FilaDeOutbox fila, string? token, CancellationToken ct)
    {
        if (fila.Accion != AccionCrearRespuesta)
            return (ResultadoDeEnvio.Rechazada, $"acción desconocida «{fila.Accion}»");

        using var pedido = new HttpRequestMessage(HttpMethod.Post, "respuestas")
        {
            Content = new StringContent(fila.Payload, Encoding.UTF8, "application/json")
        };
        pedido.Headers.TryAddWithoutValidation("Idempotency-Key", fila.ClaveIdempotencia);
        if (token is not null)
            pedido.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        if (fila.VersionEsperada is int esperada)
            pedido.Headers.TryAddWithoutValidation("If-Match", $"\"{esperada}\"");

        try
        {
            using var respuesta = await _http.SendAsync(pedido, ct);
            var codigo = (int)respuesta.StatusCode;
            var cuerpo = await respuesta.Content.ReadAsStringAsync(ct);
            var detalle = $"HTTP {codigo} {Recortar(cuerpo)}";

            if (respuesta.IsSuccessStatusCode) return (ResultadoDeEnvio.Aceptada, detalle);
            // Folio 038, P4 — 401: la sesión dejó de valer, la fila no tiene la culpa. Se reintenta (ManejadorDeSesion
            // ya le avisó a la sesión, que renueva una vez o pasa a Revocada y deja de enviar).
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return (ResultadoDeEnvio.Reintentar, detalle);
            if (respuesta.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || codigo >= 500)
                return (ResultadoDeEnvio.Reintentar, detalle);
            return (ResultadoDeEnvio.Rechazada, detalle);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            // Corrector E03 (H-4) — Toda falla de transporte se reintenta (en Android, también WebException).
            return (ResultadoDeEnvio.Reintentar, ex.Message);
        }
    }

    private static string Recortar(string texto) => texto.Length <= 200 ? texto : texto[..200] + "…";
}
