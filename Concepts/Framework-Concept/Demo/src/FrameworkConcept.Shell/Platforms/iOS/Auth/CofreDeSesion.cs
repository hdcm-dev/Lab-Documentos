using System.Security.Cryptography;
using System.Text;
using FrameworkConcept.Core.Auth;
using Foundation;
using LocalAuthentication;
using Security;

namespace FrameworkConcept.Shell.Auth;

// La misma excepción que Auth/CofreDeSesion.cs (Android). Cada plataforma compila sólo uno de los dos archivos
// (el .csproj saca el de Android de los otros destinos), así que el resto del cascarón la ve igual en las dos.
/// <summary>La clave del Keychain ya no sirve (código del dispositivo quitado, copia restaurada en otro teléfono): hay que ingresar con la cuenta.</summary>
public sealed class ClaveInvalidadaException(string mensaje, Exception? causa = null) : Exception(mensaje, causa);

/// <summary>
/// P4 — La rama iOS del cofre (DISENO-RED-Y-AUTENTICACION.md §3.4; borrador §4 «Por plataforma»). Misma superficie y
/// mismo contrato que el de Android (Auth/CofreDeSesion.cs), con las piezas de iOS:
///
///   — clave AES-256 al azar, guardada como ítem genérico del Keychain (servicio «com.frameworkconcept.demo», cuenta
///     <see cref="Alias"/>) con SecAccessControl(.userPresence): leerla exige Face ID, Touch ID o el código del
///     dispositivo, lo que el teléfono tenga. Es el equivalente de BIOMETRIC_STRONG | DEVICE_CREDENTIAL;
///   — accesibilidad WhenPasscodeSetThisDeviceOnly: el ítem sólo existe mientras el dispositivo tiene código, no viaja
///     en copias de seguridad ni a otro teléfono, y se borra si se quita el código. Es la invalidación de Android
///     «bloqueo de pantalla quitado» y «copia restaurada»;
///   — sellar y abrir piden la autenticación del sistema con LocalAuthentication (LAContext, política
///     DeviceOwnerAuthentication) ANTES de tocar la clave, como el BiometricPrompt de Android en cada uso (timeout 0);
///     el LAContext ya evaluado se le pasa al Keychain (kSecUseAuthenticationContext), que entonces no vuelve a pedir;
///   — el texto se cifra con AES-256-GCM en .NET (AesGcm) y queda «iv.datos» en base64, el mismo formato que en
///     Android: SesionDelUsuario lo guarda en la tabla de configuración sin saber de qué plataforma salió.
///
/// Igual que en Android (H-1), enrolar otra cara o huella NO invalida la clave: .userPresence acepta el código del
/// dispositivo, y sólo los ítems exclusivamente biométricos (.biometryCurrentSet) se invalidan por enrolamiento.
/// Por qué no la Secure Enclave: una clave EC de la Secure Enclave (cifrado ECIES) dejaría la clave privada en el
/// hardware como el Keystore, pero el simulador no la tiene y la demo se ensaya ahí; con el ítem protegido por
/// .userPresence el secreto tampoco sale sin la persona frente al teléfono. Es un camino de crecimiento.
/// Diferencia con Android que se declara (README, DISENO §3.4): acá la clave NO queda atada al hardware. El Keychain
/// la entrega entera a la memoria del proceso en cada apertura y AES-GCM corre en .NET; en Android la clave nunca sale
/// del Keystore y cada operación pasa por un Cipher autorizado. Por eso el byte[] se pone en cero y el NSData
/// leído se libera apenas se copia.
/// </summary>
public sealed class CofreDeSesion
{
    /// <summary>Cuenta del ítem en el Keychain (el mismo nombre que el alias del AndroidKeyStore).</summary>
    public const string Alias = "fc.sesion.refresh";

    private const string Servicio = "com.frameworkconcept.demo";
    private const int BytesDeClave = 32;
    private const int BytesDeIv = 12;
    private const int BytesDeEtiqueta = 16;

    /// <summary>
    /// ¿Este sistema puede usar el cofre? En iOS el piso del binario (16.4, ver el .csproj) ya trae todo lo necesario;
    /// queda AesGcm.IsSupported, que .NET resuelve con CryptoKit. La regla de API mínima de Android no aplica.
    /// </summary>
    public static bool SistemaAdmite => AesGcm.IsSupported;

    /// <summary>
    /// D-03 = T1 — ¿Se ofrece el desbloqueo? Que el dispositivo tenga código (DeviceOwnerAuthentication se puede
    /// evaluar) es lo que en Android es KeyguardManager.IsDeviceSecure. La API mínima de la regla es de Android: acá se
    /// le pasa el piso de la regla porque el piso de iOS del binario ya lo cubre.
    /// </summary>
    public (DesbloqueoLocal Desbloqueo, string? Motivo) Evaluar(bool activado)
    {
        using var contexto = new LAContext();
        var seguro = SistemaAdmite && contexto.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, out _);
        return ReglaDelDesbloqueo.Evaluar(ParametrosDeSesion.ApiMinimaDelDesbloqueoLocal, seguro, activado);
    }

    /// <summary>Crea una clave nueva (borra la anterior) y cifra <paramref name="texto"/> después del diálogo del sistema.</summary>
    /// <exception cref="ExcepcionDeAutenticacion">La persona canceló el diálogo, o el sistema no pudo autenticar.</exception>
    public async Task<string> SellarAsync(string texto, CancellationToken ct)
    {
        Borrar();
        using var contexto = await PedirAutenticacionAsync("Confirmá para guardar tu sesión en este teléfono.", ct);

        var clave = RandomNumberGenerator.GetBytes(BytesDeClave);
        try
        {
            GuardarClave(clave, contexto);
            var iv = RandomNumberGenerator.GetBytes(BytesDeIv);
            var claro = Encoding.UTF8.GetBytes(texto);
            var datos = new byte[claro.Length + BytesDeEtiqueta];   // cifrado || etiqueta, como el GCM de Android
            using (var aes = new AesGcm(clave, BytesDeEtiqueta))
                aes.Encrypt(iv, claro, datos.AsSpan(0, claro.Length), datos.AsSpan(claro.Length));
            return Convert.ToBase64String(iv) + "." + Convert.ToBase64String(datos);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(clave);
        }
    }

    /// <summary>Descifra el texto sellado después del diálogo del sistema (Face ID, Touch ID o código).</summary>
    /// <exception cref="ClaveInvalidadaException">La clave no está o no corresponde: se borra.</exception>
    /// <exception cref="ExcepcionDeAutenticacion">La persona canceló el diálogo, o el sistema no pudo autenticar.</exception>
    public async Task<string> AbrirAsync(string sellado, CancellationToken ct)
    {
        var partes = sellado.Split('.');
        if (partes.Length != 2) throw new ClaveInvalidadaException("El texto sellado no tiene el formato esperado.");

        // Como en Android, primero se mira si la clave está, SIN pedir nada: si se quitó el código del dispositivo o es
        // una copia restaurada en otro teléfono, no tiene sentido mostrar el diálogo.
        if (!ExisteLaClave())
            throw new ClaveInvalidadaException("La clave de este teléfono ya no está.");

        using var contexto = await PedirAutenticacionAsync("Usá la seguridad del teléfono para entrar.", ct);
        var clave = LeerClave(contexto)
            ?? throw new ClaveInvalidadaException("La clave de este teléfono ya no está.");
        try
        {
            var iv = Convert.FromBase64String(partes[0]);
            var datos = Convert.FromBase64String(partes[1]);
            if (iv.Length != BytesDeIv || datos.Length < BytesDeEtiqueta)
                throw new ClaveInvalidadaException("El texto sellado no tiene el formato esperado.");
            var claro = new byte[datos.Length - BytesDeEtiqueta];
            using (var aes = new AesGcm(clave, BytesDeEtiqueta))
                aes.Decrypt(iv, datos.AsSpan(0, claro.Length), datos.AsSpan(claro.Length), claro);
            return Encoding.UTF8.GetString(claro);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            // AuthenticationTagMismatchException hereda de CryptographicException: el sellado es de otra clave.
            Borrar();
            throw new ClaveInvalidadaException("El texto sellado no corresponde a la clave de este teléfono.", ex);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(clave);
        }
    }

    /// <summary>Cerrar sesión: borra la clave. Sin clave, el texto sellado que quede es ilegible.</summary>
    public void Borrar()
    {
        var estado = SecKeyChain.Remove(Consulta());
        if (estado is not (SecStatusCode.Success or SecStatusCode.ItemNotFound))
            Bitacora.Log($"cofre: borrar la clave devolvió {estado}");
    }

    private static SecRecord Consulta() => new(SecKind.GenericPassword) { Service = Servicio, Account = Alias };

    private static void GuardarClave(byte[] clave, LAContext contexto)
    {
        // .userPresence: Face ID, Touch ID o el código; WhenPasscodeSetThisDeviceOnly: sin código no hay ítem.
        using var valor = NSData.FromArray(clave);
        var registro = new SecRecord(SecKind.GenericPassword)
        {
            Service = Servicio,
            Account = Alias,
            ValueData = valor,
            AccessControl = new SecAccessControl(SecAccessible.WhenPasscodeSetThisDeviceOnly, SecAccessControlCreateFlags.UserPresence),
            AuthenticationContext = contexto
        };
        var estado = SecKeyChain.Add(registro);
        if (estado != SecStatusCode.Success)
            throw ExcepcionDeAutenticacion.Fallida($"El Keychain no guardó la clave ({estado}).");
    }

    /// <summary>
    /// ¿Está el ítem? Se pregunta sin interfaz con un LAContext que tiene InteractionNotAllowed: el Keychain contesta
    /// errSecInteractionNotAllowed si el ítem existe y pediría autenticación, y errSecItemNotFound si no está. Es el
    /// reemplazo que indica SecItem.h para kSecUseAuthenticationUIFail. NO se usa kSecUseAuthenticationUISkip: según
    /// el mismo encabezado, Skip saltea en silencio los ítems que piden autenticación, así que un ítem protegido con
    /// .userPresence contestaría ItemNotFound y el desbloqueo borraría una sesión sana.
    /// </summary>
    private static bool ExisteLaClave()
    {
        using var sinInterfaz = new LAContext { InteractionNotAllowed = true };
        var consulta = Consulta();
        consulta.AuthenticationContext = sinInterfaz;
        using var datos = SecKeyChain.QueryAsData(consulta, false, out var estado);
        if (estado is SecStatusCode.Success or SecStatusCode.InteractionNotAllowed) return true;
        if (estado != SecStatusCode.ItemNotFound) Bitacora.Log($"cofre: consultar la clave devolvió {estado}");
        return false;
    }

    /// <summary>Lee la clave con el LAContext ya autenticado. El Keychain no vuelve a mostrar el diálogo.</summary>
    private static byte[]? LeerClave(LAContext contexto)
    {
        var consulta = Consulta();
        consulta.AuthenticationContext = contexto;
        // El NSData es memoria nativa e inmutable (no se puede poner en cero sin romper su contrato): se copia al byte[],
        // que el llamador sí pone en cero, y se libera al salir en vez de esperar al GC.
        using var datos = SecKeyChain.QueryAsData(consulta, false, out var estado);
        return estado switch
        {
            SecStatusCode.Success when datos is not null => datos.ToArray(),
            SecStatusCode.ItemNotFound => null,
            SecStatusCode.UserCanceled => throw ExcepcionDeAutenticacion.Cancelada("Cancelaste el desbloqueo."),
            _ => throw ExcepcionDeAutenticacion.Fallida($"El Keychain no entregó la clave ({estado}).")
        };
    }

    /// <summary>
    /// El diálogo del sistema (LocalAuthentication, política DeviceOwnerAuthentication): Face ID o Touch ID, con el
    /// código del dispositivo como alternativa que ofrece el propio sistema. Devuelve el contexto autenticado, que el
    /// llamador le pasa al Keychain. Cancelar el token invalida el contexto y el diálogo se cierra.
    /// </summary>
    private static async Task<LAContext> PedirAutenticacionAsync(string motivo, CancellationToken ct)
    {
        var contexto = new LAContext();
        try
        {
            await using var _ = ct.Register(contexto.Invalidate);
            var (autorizado, error) = await contexto.EvaluatePolicyAsync(LAPolicy.DeviceOwnerAuthentication, motivo);
            if (autorizado) return contexto;

            var codigo = (LAStatus)(long)(error?.Code ?? (nint)(long)LAStatus.AuthenticationFailed);
            Bitacora.Log($"cofre: diálogo del sistema terminó con {codigo} ({error?.LocalizedDescription})");
            throw codigo is LAStatus.UserCancel or LAStatus.SystemCancel or LAStatus.AppCancel
                ? ExcepcionDeAutenticacion.Cancelada("Cancelaste el desbloqueo.")
                : ExcepcionDeAutenticacion.Fallida($"El sistema no pudo autenticar: {error?.LocalizedDescription}");
        }
        catch
        {
            contexto.Dispose();
            throw;
        }
    }
}
