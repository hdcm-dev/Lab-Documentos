using Foundation;
using WebKit;

namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// iOS — Que el puente funcione desde cualquier ruta del contenido, no sólo desde «/».
///
/// Causa (MAUI 10.0.20, HybridWebViewHandler.HasExpectedHeaders): POST /__hwvInvokeDotNet sólo se acepta si, además del
/// token X-Maui-Invoke-Token, el Origin o el Referer son EXACTAMENTE el origen local («app://0.0.0.1»). En iOS WebKit no
/// manda Origin en ese POST y el Referer es la URL completa de la página: desde /localidades o /encuesta (rutas de
/// Blazor) no coincide y MAUI contesta 400 con cuerpo vacío (simulador de iOS 26). Ni «Referrer-Policy: origin» en la
/// respuesta ni referrerPolicy en el fetch lo arreglan: WebKit no recorta el Referer para el esquema «app».
///
/// Arreglo: un script de usuario, al empezar cada documento, que fija «referrer: &lt;origen&gt;/» SÓLO en el fetch que
/// hybridwebview.js de MAUI le hace a /__hwvInvokeDotNet. El transporte sigue siendo el de MAUI (mismo pedido, mismo
/// token, misma respuesta) y la página sigue siendo la del origen local, que es lo que el chequeo quiere asegurar.
/// Los scripts de usuario no pasan por la CSP del contenido. Se puede quitar cuando MAUI compare el Referer por prefijo.
/// </summary>
public static class RefererDelPuente
{
    private const string Script = """
        (function () {
            'use strict';
            var destino = location.origin + '/__hwvInvokeDotNet';
            var original = window.fetch;
            window.fetch = function (recurso, opciones) {
                if (recurso === destino && opciones && opciones.referrer === undefined)
                    opciones = Object.assign({}, opciones, { referrer: location.origin + '/' });
                return original.call(this, recurso, opciones);
            };
        })();
        """;

    /// <summary>
    /// Agrega el script al WKWebView del HybridWebView. Se llama al conectarse el handler (HandlerChanged), antes de que
    /// llegue la respuesta del primer documento: el WKUserContentController lo aplica a ese documento y a los siguientes.
    /// </summary>
    public static void Instalar(HybridWebView vista)
    {
        if (vista.Handler?.PlatformView is not WKWebView wk) return;
        using var script = new WKUserScript(new NSString(Script), WKUserScriptInjectionTime.AtDocumentStart, isForMainFrameOnly: true);
        wk.Configuration.UserContentController.AddUserScript(script);
    }
}
