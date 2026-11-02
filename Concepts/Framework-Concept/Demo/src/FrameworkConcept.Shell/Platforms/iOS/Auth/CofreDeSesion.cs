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
///
/// Con Secure Enclave (un teléfono): la clave AES NO se guarda en claro en el Keychain. Se genera DENTRO de la Secure
/// Enclave una clave P-256 permanente (etiqueta <see cref="EtiquetaDelEnclave"/>, SecAccessControl
/// .privateKeyUsage | .userPresence, WhenPasscodeSetThisDeviceOnly) y la clave AES se envuelve con su pública por
/// ECIES (eciesEncryptionCofactorVariableIVX963SHA256AESGCM). El ítem del Keychain guarda el envoltorio. Para abrir, la
/// privada descifra el envoltorio dentro del hardware con el LAContext ya autenticado: la privada nunca sale del
/// enclave, que es lo que hace el Keystore en Android. Equivalente EN DISEÑO; NO VERIFICADO (el simulador no tiene
/// enclave y la demo no se corrió en un teléfono). La clave AES igual pasa por la memoria del proceso para AES-GCM.
/// Sin Secure Enclave (el simulador) queda el camino de siempre: la clave AES en claro dentro del ítem protegido.
/// El camino queda escrito en el sellado: con enclave es «iv.datos.e» (<see cref="MarcaDelEnclave"/>), sin enclave
/// «iv.datos», el formato de Android. Abrir exige que el ítem coincida con la marca: un ítem de 32 bytes (clave en
/// claro) frente a un sellado con enclave se rechaza, así el camino débil no se cuela en silencio. En un teléfono, si
/// el enclave no genera la clave, sellar FALLA; sólo se sigue sin enclave si el error dice que no lo hay
/// (errSecUnimplemented). Una sesión sellada antes del enclave («iv.datos», 32 bytes) se sigue abriendo.
/// Por eso el byte[] se pone en cero y el NSData leído se libera apenas se copia.
/// </summary>
public sealed class CofreDeSesion
{
    /// <summary>Cuenta del ítem en el Keychain (el mismo nombre que el alias del AndroidKeyStore).</summary>
    public const string Alias = "fc.sesion.refresh";

    /// <summary>Etiqueta (kSecAttrApplicationTag) de la clave P-256 de la Secure Enclave que envuelve la clave AES.</summary>
    public const string EtiquetaDelEnclave = "fc.sesion.refresh.enclave";

    private const string Servicio = "com.frameworkconcept.demo";
    private static readonly SecKeyAlgorithm Ecies = SecKeyAlgorithm.EciesEncryptionCofactorVariableIvx963Sha256AesGcm;
    private const int BytesDeClave = 32;
    private const int BytesDeIv = 12;
    private const int BytesDeEtiqueta = 16;
    private const string MarcaDelEnclave = "e";
    private const int ErrSecUnimplemented = -4;   // OSStatus: la operación no existe en este dispositivo (sin enclave)

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
            // Con enclave se guarda el envoltorio ECIES; sin enclave (errSecUnimplemented o el simulador), la clave.
            var envoltorio = EnvolverConElEnclave(clave, contexto);
            GuardarClave(envoltorio ?? clave, contexto);
            var iv = RandomNumberGenerator.GetBytes(BytesDeIv);
            var claro = Encoding.UTF8.GetBytes(texto);
            var datos = new byte[claro.Length + BytesDeEtiqueta];   // cifrado || etiqueta, como el GCM de Android
            using (var aes = new AesGcm(clave, BytesDeEtiqueta))
                aes.Encrypt(iv, claro, datos.AsSpan(0, claro.Length), datos.AsSpan(claro.Length));
            var sellado = Convert.ToBase64String(iv) + "." + Convert.ToBase64String(datos);
            return envoltorio is null ? sellado : sellado + "." + MarcaDelEnclave;
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
        var conEnclave = partes.Length == 3 && partes[2] == MarcaDelEnclave;
        if (partes.Length != 2 && !conEnclave)
            throw new ClaveInvalidadaException("El texto sellado no tiene el formato esperado.");

        // Como en Android, primero se mira si la clave está, SIN pedir nada: si se quitó el código del dispositivo o es
        // una copia restaurada en otro teléfono, no tiene sentido mostrar el diálogo.
        if (!ExisteLaClave())
            throw new ClaveInvalidadaException("La clave de este teléfono ya no está.");

        using var contexto = await PedirAutenticacionAsync("Usá la seguridad del teléfono para entrar.", ct);
        var guardado = LeerClave(contexto)
            ?? throw new ClaveInvalidadaException("La clave de este teléfono ya no está.");
        // El sellado dice el camino y el ítem tiene que coincidir: 32 bytes son una clave en claro, otro largo un
        // envoltorio. Un ítem en claro frente a un sellado con enclave (o al revés) no se acepta.
        var enClaro = guardado.Length == BytesDeClave;
        if (enClaro == conEnclave)
        {
            CryptographicOperations.ZeroMemory(guardado);
            Borrar();
            throw new ClaveInvalidadaException("La clave de este teléfono no corresponde al camino con que se selló la sesión.");
        }
        var clave = enClaro ? guardado : DesenvolverConElEnclave(guardado, contexto);
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
        estado = SecKeyChain.Remove(ConsultaDelEnclave());
        if (estado is not (SecStatusCode.Success or SecStatusCode.ItemNotFound))
            Bitacora.Log($"cofre: borrar la clave del enclave devolvió {estado}");
    }

    private static SecRecord ConsultaDelEnclave() => new(SecKind.Key)
    {
        KeyClass = SecKeyClass.Private,
        ApplicationTag = NSData.FromString(EtiquetaDelEnclave, NSStringEncoding.UTF8)
    };

    /// <summary>
    /// ¿Hay Secure Enclave? El simulador nunca la tiene (se evita el intento y su error); en un dispositivo la
    /// respuesta la da el propio intento de generar la clave en <see cref="EnvolverConElEnclave"/>.
    /// </summary>
    private static bool PuedeHaberEnclave => ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.DEVICE;

    /// <summary>
    /// Genera la clave P-256 dentro de la Secure Enclave y envuelve <paramref name="clave"/> con su pública (ECIES).
    /// Cifrar con la pública no pide autenticación; el contexto viaja igual en la generación para que el sistema no
    /// muestre otro diálogo. Devuelve null si no hay enclave: el llamador sigue con el camino sin enclave.
    /// </summary>
    /// <exception cref="ExcepcionDeAutenticacion">Hay enclave (o no se sabe) y no generó la clave o no envolvió.</exception>
    private static byte[]? EnvolverConElEnclave(byte[] clave, LAContext contexto)
    {
        if (!PuedeHaberEnclave) return null;
        using var control = new SecAccessControl(SecAccessible.WhenPasscodeSetThisDeviceOnly,
            SecAccessControlCreateFlags.PrivateKeyUsage | SecAccessControlCreateFlags.UserPresence);
        var parametros = new SecKeyGenerationParameters
        {
            KeyType = SecKeyType.ECSecPrimeRandom,
            KeySizeInBits = 256,
            TokenID = SecTokenID.SecureEnclave,
            PrivateKeyAttrs = new SecKeyParameters
            {
                IsPermanent = true,
                ApplicationTag = NSData.FromString(EtiquetaDelEnclave, NSStringEncoding.UTF8),
                AccessControl = control
            }
        };
        using var privada = SecKey.CreateRandomKey(parametros, out var errorAlGenerar);
        if (privada is null)
        {
            // Sólo «no hay enclave» deja seguir sin él. Cualquier otro error (pasajero, control de acceso, LAContext)
            // frena el sellado: bajar en silencio al camino débil en un teléfono que sí tiene enclave no se acepta.
            if (errorAlGenerar?.Domain == NSError.OsStatusErrorDomain && errorAlGenerar.Code == ErrSecUnimplemented)
            {
                Bitacora.Log($"cofre: sin Secure Enclave ({errorAlGenerar.LocalizedDescription}); clave en el Keychain");
                return null;
            }
            SecKeyChain.Remove(ConsultaDelEnclave());
            throw ExcepcionDeAutenticacion.Fallida(
                $"La Secure Enclave no generó la clave ({errorAlGenerar?.Code}: {errorAlGenerar?.LocalizedDescription}).");
        }
        using var publica = privada.GetPublicKey();
        using var claro = NSData.FromArray(clave);
        using var envoltorio = publica?.CreateEncryptedData(Ecies, claro, out var errorAlCifrar);
        if (envoltorio is null)
        {
            SecKeyChain.Remove(ConsultaDelEnclave());
            throw ExcepcionDeAutenticacion.Fallida("La Secure Enclave no envolvió la clave.");
        }
        Bitacora.Log("cofre: clave envuelta por la Secure Enclave");
        return envoltorio.ToArray();
    }

    /// <summary>
    /// Desenvuelve la clave AES con la privada del enclave. La consulta de la privada lleva el LAContext autenticado
    /// (kSecUseAuthenticationContext) y la operación ECIES corre dentro del hardware sin otro diálogo.
    /// </summary>
    /// <exception cref="ClaveInvalidadaException">La privada ya no está o el envoltorio no corresponde: se borra todo.</exception>
    /// <exception cref="ExcepcionDeAutenticacion">Cancelación o fallo que no prueba que la clave se perdió: NO se borra.</exception>
    private byte[] DesenvolverConElEnclave(byte[] envoltorio, LAContext contexto)
    {
        var consulta = ConsultaDelEnclave();
        consulta.AuthenticationContext = contexto;
        var privada = SecKeyChain.QueryAsConcreteType(consulta, out var estado) as SecKey;
        if (privada is null)
        {
            // Como LeerClave: sólo ItemNotFound prueba que la clave se perdió. Cancelar o un fallo pasajero no borran
            // una sesión sana.
            Bitacora.Log($"cofre: consultar la clave del enclave devolvió {estado}");
            if (estado == SecStatusCode.ItemNotFound)
            {
                Borrar();
                throw new ClaveInvalidadaException("La clave de este teléfono ya no está.");
            }
            throw estado == SecStatusCode.UserCanceled
                ? ExcepcionDeAutenticacion.Cancelada("Cancelaste el desbloqueo.")
                : ExcepcionDeAutenticacion.Fallida($"El Keychain no entregó la clave del enclave ({estado}).");
        }
        using (privada)
        using (var datos = NSData.FromArray(envoltorio))
        using (var claro = privada.CreateDecryptedData(Ecies, datos, out var error))
        {
            if (claro is null || claro.Length != BytesDeClave)
            {
                Bitacora.Log($"cofre: la Secure Enclave no abrió el envoltorio ({error?.Code}: {error?.LocalizedDescription})");
                // Sólo un fallo criptográfico (errSecDecode, o un largo que no es una clave) dice que el envoltorio no
                // corresponde. Cancelar, autenticación o interacción no permitida no borran nada.
                if (claro is not null || (error?.Domain == NSError.OsStatusErrorDomain && error.Code == (long)SecStatusCode.Decode))
                {
                    Borrar();
                    throw new ClaveInvalidadaException("El texto sellado no corresponde a la clave de este teléfono.");
                }
                throw error?.Code == (long)SecStatusCode.UserCanceled
                    ? ExcepcionDeAutenticacion.Cancelada("Cancelaste el desbloqueo.")
                    : ExcepcionDeAutenticacion.Fallida($"La Secure Enclave no abrió el envoltorio ({error?.Code}).");
            }
            return claro.ToArray();
        }
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
