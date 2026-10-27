using System.Net.Http.Headers;
using FrameworkConcept.Core.Auth;

namespace FrameworkConcept.Shell.Auth;

/// <summary>
/// Borrador §6 «Actividad» («agrega el token») y P4 — Pone «Authorization: Bearer {access token}» en los pedidos al
/// receptor si hay sesión válida. Con un 401, le avisa a la sesión (que renueva una vez o pasa a Revocada).
/// El refresh token nunca sale de acá hacia el contenido (P3).
///
/// Lo arma FabricaHttp encima de ManejadorDeRed y sólo para el receptor: el canal es público (índice firmado, P2).
/// Un 401 no se reintenta acá (el cuerpo del pedido ya se consumió): la sesión renueva una vez y la fila vuelve a salir
/// con el backoff del sincronizador (ClienteDelReceptor lo trata como «reintentar», DISENO §3.5).
/// </summary>
public sealed class ManejadorDeSesion(ISesionDelUsuario sesion) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Corrector E03 — El sincronizador pone el token de la fila (elegido junto con su dueño, bajo el cerrojo de la
        // sesión): se respeta. Si no vino, el de la sesión del momento.
        var token = request.Headers.Authorization?.Parameter;
        if (token is null && await sesion.ObtenerAccessTokenAsync(cancellationToken) is { } actual)
        {
            token = actual;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Corrector E03 — Con cuenta, nada sale sin token: un receptor sin --oidc-emisor lo aceptaría sin dueño.
        // Se contesta 401 local (ClienteDelReceptor → reintentar), sin tocar la red.
        if (token is null && sesion.Actual.Modo == ModoDeSesion.Oidc)
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { RequestMessage = request, ReasonPhrase = "sin token de sesión (local)" };

        var respuesta = await base.SendAsync(request, cancellationToken);
        if (respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized && token is not null)
            await sesion.InformarNoAutorizadoAsync(token, cancellationToken);
        return respuesta;
    }
}
