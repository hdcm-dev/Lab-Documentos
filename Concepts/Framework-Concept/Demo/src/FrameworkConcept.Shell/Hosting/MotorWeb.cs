namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// C-3 de E01, P1, P3 — Qué Android System WebView tiene el teléfono. El runtime de Blazor de .NET 10 necesita
/// WebView 94 o mayor (E01 §6.3: el 83 del emulador no lo arranca). Es la misma versión que muestra
/// <c>adb shell dumpsys webviewupdate</c> («Current WebView package»).
///
/// iOS: no hay un WebView que se actualice aparte. WKWebView es el WebKit del sistema, así que el motor es el iOS, y
/// su piso (ParametrosDeProducto.IosMinimo, 16.4) lo garantiza la tienda con MinimumOSVersion (ver el .csproj). Como
/// el predicado de compatibilidad de Core compara sólo el eje Android (WebView ≥ 94), en iOS <see cref="VersionMayor"/>
/// informa el equivalente: el mínimo de Android si el sistema alcanza el piso de iOS, 0 si no (no debería pasar).
/// <see cref="Paquete"/> y <see cref="Version"/> muestran lo real: WebKit y la versión de iOS.
/// </summary>
public sealed class MotorWeb
{
    private readonly Lazy<(string? Paquete, string? Version)> _leido = new(Leer);

    /// <summary>Paquete del proveedor activo, por ejemplo com.google.android.webview.</summary>
    public string? Paquete => _leido.Value.Paquete;

    /// <summary>Versión completa, por ejemplo 83.0.4103.106.</summary>
    public string? Version => _leido.Value.Version;

    /// <summary>El mayor (83), o null si no se pudo leer.</summary>
    public int? VersionMayor =>
#if IOS
        Version is null ? null
        : System.Version.TryParse(Core.Contracts.ParametrosDeProducto.IosMinimo, out var piso)
          && OperatingSystem.IsIOSVersionAtLeast(piso.Major, piso.Minor) ? Core.Contracts.ParametrosDeProducto.WebViewMinimo : 0;
#else
        Version is { } v && int.TryParse(v.Split('.')[0], System.Globalization.NumberStyles.None,
            System.Globalization.CultureInfo.InvariantCulture, out var mayor) ? mayor : null;
#endif

    private static (string?, string?) Leer()
    {
#if ANDROID
        try
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                var paquete = Android.Webkit.WebView.CurrentWebViewPackage;
                if (paquete is not null) return (paquete.PackageName, paquete.VersionName);
            }

            // Android 7 (API 24-25): no hay API para el proveedor activo; se lee el paquete de Google.
            var info = Android.App.Application.Context.PackageManager?.GetPackageInfo("com.google.android.webview", 0);
            return (info?.PackageName, info?.VersionName);
        }
        catch (Exception ex)
        {
            Bitacora.Error("motor web: no se pudo leer la versión", ex);
            return (null, null);
        }
#elif IOS
        // NSProcessInfo y no UIDevice: se puede leer desde cualquier hilo (el actualizador lo pide fuera del principal).
        var v = Foundation.NSProcessInfo.ProcessInfo.OperatingSystemVersion;
        return ("WebKit (WKWebView de iOS)", $"{v.Major}.{v.Minor}.{v.PatchVersion}");
#else
        return (null, null);
#endif
    }
}
