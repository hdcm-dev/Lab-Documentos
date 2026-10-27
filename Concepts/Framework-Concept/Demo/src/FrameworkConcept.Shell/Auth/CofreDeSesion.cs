using System.Runtime.Versioning;
using Android.Content;
using Android.Hardware.Biometrics;
using Android.OS;
using Android.Security.Keystore;
using FrameworkConcept.Core.Auth;
using Java.Lang;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;
using Exception = System.Exception;

namespace FrameworkConcept.Shell.Auth;

/// <summary>La clave del Keystore ya no sirve (huella nueva, bloqueo quitado, copia restaurada): hay que ingresar con la cuenta.</summary>
public sealed class ClaveInvalidadaException(string mensaje, Exception? causa = null) : Exception(mensaje, causa);

/// <summary>
/// P4 — El refresh token protegido por el hardware y atado a la autenticación del dispositivo (borrador §4 «Por
/// plataforma», ajustado por P4: setUserAuthenticationParameters, API ≥ 30). DISENO-RED-Y-AUTENTICACION.md §3.4:
///
///   — clave AES-256-GCM en AndroidKeyStore, alias «fc.sesion.refresh», con setUserAuthenticationRequired(true) +
///     setUserAuthenticationParameters(0, AUTH_BIOMETRIC_STRONG | AUTH_DEVICE_CREDENTIAL): timeout 0 = cada uso de la
///     clave (sellar o abrir) pide autenticación, y la clave sólo sirve dentro de un CryptoObject del BiometricPrompt;
///   — sellar y abrir con android.hardware.biometrics.BiometricPrompt (de la plataforma, sin paquete AndroidX) y
///     autenticadores BIOMETRIC_STRONG | DEVICE_CREDENTIAL: huella, rostro, PIN o patrón, lo que el teléfono tenga;
///   — se ofrece sólo con API ≥ 30 y bloqueo de pantalla (D-03 = T1, <see cref="ReglaDelDesbloqueo"/>);
///   — KeyPermanentlyInvalidatedException → <see cref="ClaveInvalidadaException"/>, y la clave se borra.
///
/// El texto sellado («iv.datos» en base64) lo guarda SesionDelUsuario en la tabla de configuración; acá sólo vive la clave.
/// </summary>
public sealed class CofreDeSesion
{
    /// <summary>Alias de la clave en AndroidKeyStore.</summary>
    public const string Alias = "fc.sesion.refresh";

    private const string Proveedor = "AndroidKeyStore";
    private const string Transformacion = "AES/GCM/NoPadding";
    private const int BitsDeEtiqueta = 128;

    /// <summary>D-03 = T1 — ¿Se ofrece el desbloqueo? API ≥ 30 y KeyguardManager.IsDeviceSecure.</summary>
    public (DesbloqueoLocal Desbloqueo, string? Motivo) Evaluar(bool activado)
    {
        var contexto = Android.App.Application.Context;
        var guardia = contexto.GetSystemService(Context.KeyguardService) as Android.App.KeyguardManager;
        return ReglaDelDesbloqueo.Evaluar((int)Build.VERSION.SdkInt, guardia?.IsDeviceSecure == true, activado);
    }

    /// <summary>Crea una clave nueva (borra la anterior) y cifra <paramref name="texto"/> después del diálogo del sistema.</summary>
    /// <exception cref="ExcepcionDeAutenticacion">La persona canceló el diálogo, o el sistema no pudo autenticar.</exception>
    [SupportedOSPlatform("android30.0")]
    public async Task<string> SellarAsync(string texto, CancellationToken ct)
    {
        Borrar();
        CrearClave();
        var cifrador = Cipher.GetInstance(Transformacion)!;
        cifrador.Init(CipherMode.EncryptMode, Clave());   // el Keystore elige el IV
        var autorizado = await PedirAutenticacionAsync(cifrador, "Usar la seguridad del teléfono",
            "Confirmá para guardar tu sesión en este teléfono.", ct);
        var datos = autorizado.DoFinal(System.Text.Encoding.UTF8.GetBytes(texto))!;
        return Convert.ToBase64String(autorizado.GetIV()!) + "." + Convert.ToBase64String(datos);
    }

    /// <summary>Descifra el texto sellado después del diálogo del sistema (huella, rostro, PIN o patrón).</summary>
    /// <exception cref="ClaveInvalidadaException">La clave no está o quedó invalidada: se borra.</exception>
    /// <exception cref="ExcepcionDeAutenticacion">La persona canceló el diálogo, o el sistema no pudo autenticar.</exception>
    [SupportedOSPlatform("android30.0")]
    public async Task<string> AbrirAsync(string sellado, CancellationToken ct)
    {
        var partes = sellado.Split('.');
        if (partes.Length != 2) throw new ClaveInvalidadaException("El texto sellado no tiene el formato esperado.");

        var clave = Clave();
        if (clave is null)
            // Hay texto sellado pero no clave: se restauró una copia de seguridad en otro teléfono (allowBackup) o se borró.
            throw new ClaveInvalidadaException("La clave de este teléfono ya no está.");

        var cifrador = Cipher.GetInstance(Transformacion)!;
        try
        {
            cifrador.Init(CipherMode.DecryptMode, clave, new GCMParameterSpec(BitsDeEtiqueta, Convert.FromBase64String(partes[0])));
        }
        catch (KeyPermanentlyInvalidatedException ex)
        {
            // Borrador §4 «si se agrega una huella nueva, Android puede invalidar la clave».
            Borrar();
            throw new ClaveInvalidadaException("La seguridad del teléfono cambió (huella nueva o bloqueo de pantalla quitado).", ex);
        }

        var autorizado = await PedirAutenticacionAsync(cifrador, "Desbloquear",
            "Usá la seguridad del teléfono para entrar.", ct);
        try
        {
            return System.Text.Encoding.UTF8.GetString(autorizado.DoFinal(Convert.FromBase64String(partes[1]))!);
        }
        catch (AEADBadTagException ex)
        {
            Borrar();
            throw new ClaveInvalidadaException("El texto sellado no corresponde a la clave de este teléfono.", ex);
        }
    }

    /// <summary>Cerrar sesión: borra la clave. Sin clave, el texto sellado que quede es ilegible.</summary>
    public void Borrar()
    {
        try
        {
            var almacen = KeyStore.GetInstance(Proveedor)!;
            almacen.Load(null);
            if (almacen.ContainsAlias(Alias)) almacen.DeleteEntry(Alias);
        }
        catch (Exception ex)
        {
            Bitacora.Error("cofre: borrar la clave", ex);
        }
    }

    [SupportedOSPlatform("android30.0")]
    private static void CrearClave()
    {
        var especificacion = new KeyGenParameterSpec.Builder(Alias, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
            .SetBlockModes(KeyProperties.BlockModeGcm)!
            .SetEncryptionPaddings(KeyProperties.EncryptionPaddingNone)!
            .SetKeySize(256)!
            .SetUserAuthenticationRequired(true)!
            // P4 — timeout 0: cada uso pide autenticación; vale la biometría fuerte o la credencial del dispositivo.
            .SetUserAuthenticationParameters(0, (int)(KeyPropertiesAuthType.BiometricStrong | KeyPropertiesAuthType.DeviceCredential))!
            // Una huella nueva invalida la clave (borrador §4).
            .SetInvalidatedByBiometricEnrollment(true)!
            .Build();
        var generador = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, Proveedor)!;
        generador.Init(especificacion);
        generador.GenerateKey();
    }

    private static IKey? Clave()
    {
        var almacen = KeyStore.GetInstance(Proveedor)!;
        almacen.Load(null);
        return almacen.GetKey(Alias, null);
    }

    /// <summary>
    /// El diálogo del sistema con el cifrador como CryptoObject: sólo después de autenticar, el Keystore deja usar la
    /// clave. Con DEVICE_CREDENTIAL no lleva botón negativo (lo pone el sistema).
    /// </summary>
    [SupportedOSPlatform("android30.0")]
    private static async Task<Cipher> PedirAutenticacionAsync(Cipher cifrador, string titulo, string subtitulo, CancellationToken ct)
    {
        var resultado = new TaskCompletionSource<Cipher>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelacion = new CancellationSignal();
        await using var _ = ct.Register(cancelacion.Cancel);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            var actividad = Platform.CurrentActivity
                ?? throw ExcepcionDeAutenticacion.Fallida("La app no está en primer plano: no se puede mostrar el diálogo del sistema.");
            var dialogo = new BiometricPrompt.Builder(actividad)
                .SetTitle(titulo)!
                .SetSubtitle(subtitulo)!
                .SetAllowedAuthenticators((int)(BiometricManagerAuthenticators.BiometricStrong | BiometricManagerAuthenticators.DeviceCredential))!
                .Build();
            dialogo.Authenticate(new BiometricPrompt.CryptoObject(cifrador), cancelacion, actividad.MainExecutor!, new Respuesta(resultado));
        });
        return await resultado.Task;
    }

    /// <summary>Lo que contesta el diálogo del sistema.</summary>
    [SupportedOSPlatform("android30.0")]
    private sealed class Respuesta(TaskCompletionSource<Cipher> resultado) : BiometricPrompt.AuthenticationCallback
    {
        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult? r)
        {
            if (r?.CryptoObject?.Cipher is { } cifrador) resultado.TrySetResult(cifrador);
            else resultado.TrySetException(ExcepcionDeAutenticacion.Fallida("El sistema autenticó pero no devolvió el cifrador."));
        }

        public override void OnAuthenticationError(BiometricErrorCode codigo, ICharSequence? texto)
        {
            Bitacora.Log($"cofre: diálogo del sistema terminó con {codigo} ({texto})");
            resultado.TrySetException(codigo is BiometricErrorCode.UserCanceled or BiometricErrorCode.Canceled
                ? ExcepcionDeAutenticacion.Cancelada("Cancelaste el desbloqueo.")
                : ExcepcionDeAutenticacion.Fallida($"El sistema no pudo autenticar: {texto}"));
        }

        // Un intento fallido (huella no reconocida): el diálogo sigue abierto y la persona puede reintentar.
        public override void OnAuthenticationFailed() => Bitacora.Log("cofre: intento no reconocido");
    }
}
