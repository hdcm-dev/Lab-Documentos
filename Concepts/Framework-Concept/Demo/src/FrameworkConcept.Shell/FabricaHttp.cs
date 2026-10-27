using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Shell.Auth;
using FrameworkConcept.Shell.Red;

namespace FrameworkConcept.Shell;

/// <summary>
/// Borrador §6 «Actividad» — Todo HttpClient del framework sale de acá, con la cadena de DelegatingHandler propia:
///
///   receptor:  ManejadorDeSesion (Bearer, 401)  →  ManejadorDeRed (registro, falla → RedSinServidor)  →  HttpClientHandler
///   canal:                                         ManejadorDeRed                                    →  HttpClientHandler
///
/// El canal no lleva token: es un servidor estático público cuyo índice va firmado (P2).
/// HttpClientHandler, en .NET para Android, usa el manejador nativo (AndroidMessageHandler) como el «new HttpClient()»
/// de A3: respeta la configuración de red de Android (sólo HTTPS y la CA de demostración en Debug).
/// El chequeo de salud del monitor NO usa esta fábrica: si pasara por ManejadorDeRed se contaría dos veces.
/// </summary>
public sealed class FabricaHttp(ISesionDelUsuario sesion, IMonitorDeRed red)
{
    public HttpClient CrearParaReceptor(Uri baseAddress, TimeSpan timeout) =>
        new(new ManejadorDeSesion(sesion) { InnerHandler = new ManejadorDeRed(red, "receptor") { InnerHandler = new HttpClientHandler() } })
        {
            BaseAddress = baseAddress,
            Timeout = timeout
        };

    public HttpClient CrearParaCanal(Uri baseAddress, TimeSpan timeout) =>
        new(new ManejadorDeRed(red, "canal") { InnerHandler = new HttpClientHandler() })
        {
            BaseAddress = baseAddress,
            Timeout = timeout
        };
}
