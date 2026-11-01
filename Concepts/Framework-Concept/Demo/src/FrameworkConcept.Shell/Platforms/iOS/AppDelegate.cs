using Foundation;

namespace FrameworkConcept.Shell;

/// <summary>
/// El equivalente iOS de MainApplication (Platforms/Android/): arma la app MAUI con la misma composición
/// (MauiProgram). No sobreescribe OpenUrl: WebAuthenticator usa ASWebAuthenticationSession, que recibe la vuelta
/// del emisor (com.frameworkconcept.demo://callback) sin pasar por el delegado de la app.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
