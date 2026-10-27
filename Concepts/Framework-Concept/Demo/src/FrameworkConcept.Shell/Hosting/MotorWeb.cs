namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// C-3 de E01, P1, P3 — Qué Android System WebView tiene el teléfono. El runtime de Blazor de .NET 10 necesita
/// WebView 94 o mayor (E01 §6.3: el 83 del emulador no lo arranca). Es la misma versión que muestra
/// <c>adb shell dumpsys webviewupdate</c> («Current WebView package»).
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
        Version is { } v && int.TryParse(v.Split('.')[0], System.Globalization.NumberStyles.None,
            System.Globalization.CultureInfo.InvariantCulture, out var mayor) ? mayor : null;

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
#else
        return (null, null);
#endif
    }
}
