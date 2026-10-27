using System.Security.Cryptography;

namespace FrameworkConcept.Tools;

/// <summary>
/// «claves»: genera el par de claves de DEMOSTRACIÓN con que se firma el índice del canal (P2).
///
/// — Algoritmo: ECDSA sobre la curva P-256 (con SHA-256 al firmar, «ES256»), de la plataforma, sin paquetes.
/// — La privada (PKCS#8, PEM) queda en claves/privada-demo.pem, sólo legible por su dueño. NO se versiona:
///   .gitignore la excluye. Custodia de la demo: la máquina de quien la corre (P2, «Custodia de la clave»).
/// — La pública (SubjectPublicKeyInfo, PEM) queda en claves/publica-demo.pem y SÍ se versiona: el cascarón la
///   embebe. Si se regenera el par, hay que regenerar semilla/ (está firmada con el par anterior) y recompilar el
///   cascarón, porque el instalado no conoce la clave nueva. Por eso tampoco pisa una pública existente sin --forzar.
/// </summary>
internal static class ClavesDeDemostracion
{
    /// <summary>Identificador de la clave activa de la demo; va en el índice y en su firma.</summary>
    public const string KeyIdPorDefecto = "demo-2026";

    public const string Privada = "privada-demo.pem";
    public const string Publica = "publica-demo.pem";

    public static int Generar(Argumentos a)
    {
        var carpeta = a.Opcion("dir", "claves");
        var privada = Path.Combine(carpeta, Privada);
        var publica = Path.Combine(carpeta, Publica);

        // Regenerar sin querer dejaría a los cascarones instalados sin poder verificar nada: se pide --forzar.
        if (File.Exists(privada) && !a.Bandera("forzar"))
        {
            Console.WriteLine($"claves: ya existe {privada}; no se regenera (usá --forzar y recompilá el cascarón).");
            Mostrar(publica);
            return 0;
        }

        // Un clon nuevo trae la pública versionada pero no la privada. Pisarla en silencio dejaría a semilla/
        // (firmada con el par anterior) sin verificar, y el APK la descartaría. Se pide --forzar, y quien lo
        // usa sabe que después tiene que rehacer publicar, definicion, firmar y semilla antes del APK.
        if (File.Exists(publica) && !a.Bandera("forzar"))
        {
            Console.Error.WriteLine($"claves: existe {publica} (versionada) y falta su privada {privada}.");
            Console.Error.WriteLine("  Con un par nuevo, semilla/ deja de verificar. Para generarlo igual: «claves --forzar»,");
            Console.Error.WriteLine("  y después «publicar v1», «definicion v1», «firmar» y «semilla» antes de compilar el APK.");
            return 1;
        }

        Directory.CreateDirectory(carpeta);
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        File.WriteAllText(privada, ecdsa.ExportPkcs8PrivateKeyPem() + "\n");
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(privada, UnixFileMode.UserRead | UnixFileMode.UserWrite);

        File.WriteAllText(publica, ecdsa.ExportSubjectPublicKeyInfoPem() + "\n");

        Console.WriteLine($"claves: par ECDSA P-256 de demostración generado.");
        Console.WriteLine($"  privada (NO se versiona): {privada}");
        Mostrar(publica);
        return 0;
    }

    private static void Mostrar(string publica)
    {
        if (!File.Exists(publica)) return;
        using var ecdsa = CargarPublica(publica);
        var spki = ecdsa.ExportSubjectPublicKeyInfo();
        Console.WriteLine($"  pública (se versiona):    {publica}");
        Console.WriteLine($"  keyId:                    {KeyIdPorDefecto}");
        Console.WriteLine($"  SPKI en base64:           {Convert.ToBase64String(spki)}");
        Console.WriteLine($"  huella sha256 del SPKI:   {CarpetaDelCanal.Sha256(spki)}");
    }

    public static ECDsa CargarPrivada(string ruta)
    {
        if (!File.Exists(ruta))
            throw new ErrorDeUso($"no existe la clave privada {ruta}; generala con «claves»");
        var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(File.ReadAllText(ruta));
        return ecdsa;
    }

    public static ECDsa CargarPublica(string ruta)
    {
        if (!File.Exists(ruta))
            throw new ErrorDeUso($"no existe la clave pública {ruta}");
        var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(File.ReadAllText(ruta));
        return ecdsa;
    }
}
