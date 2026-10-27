using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Authentication;

namespace FrameworkConcept.Shell.Auth;

/// <summary>
/// P4, RFC 8252 §7.1 — El navegador del sistema vuelve a la app por el esquema privado
/// <c>com.frameworkconcept.demo://callback</c> (el applicationId, como pide la RFC). Esta actividad recibe esa URL y
/// se la entrega a WebAuthenticator, que completa el AuthenticateAsync pendiente con el «code» y el «state».
/// El redirect está registrado en el realm de ensayo (infra/keycloak/realm-fc-demo.json) y en
/// ParametrosDeSesion.Redireccion: los tres tienen que coincidir.
/// </summary>
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter([Intent.ActionView],
    Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = "com.frameworkconcept.demo",
    DataHost = "callback")]
public sealed class CallbackDeAutenticacion : WebAuthenticatorCallbackActivity
{
}
