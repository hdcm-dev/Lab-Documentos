namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// El único camino por el que el cascarón ejecuta JavaScript propio en la página del contenido: la lectura de la marca
/// «primer-render» (Pages/ContenidoPage.xaml.cs) y los eventos del puente (Bridge/EmisorDeEventos.cs).
///
/// Android: HybridWebView.EvaluateJavaScriptAsync, que llama a evaluateJavascript tal cual.
/// iOS: esa misma API de MAUI (y también InvokeJavaScriptAsync, que se apoya en ella) envuelve el script en
/// «eval(…)», y la CSP del contenido (script-src sin 'unsafe-eval', P3) lo bloquea: la marca volvía null y los eventos
/// no llegaban nunca, sin error del lado .NET (visto en el simulador de iOS 26). Se evalúa entonces en el WKWebView,
/// que inyecta el script como la app y no como la página, igual que evaluateJavascript en Android.
/// </summary>
public static class ScriptEnLaVista
{
    /// <summary>
    /// Evalúa <paramref name="expresion"/> en la página y devuelve su valor como texto (String(…) en iOS, para que vuelva
    /// un NSString y no un NSNumber con su propio formato). null si la vista todavía no tiene su WebView nativo.
    /// Un error de JavaScript sale como excepción en iOS; en Android no (vuelve null): quien llama compara el valor
    /// esperado y decide cómo dejarlo en la bitácora.
    /// </summary>
    public static async Task<string?> EvaluarAsync(HybridWebView vista, string expresion)
    {
#if IOS
        if (vista.Handler?.PlatformView is not WebKit.WKWebView wk) return null;
        return (await wk.EvaluateJavaScriptAsync($"String({expresion})"))?.ToString();
#else
        return await vista.EvaluateJavaScriptAsync(expresion);
#endif
    }
}
