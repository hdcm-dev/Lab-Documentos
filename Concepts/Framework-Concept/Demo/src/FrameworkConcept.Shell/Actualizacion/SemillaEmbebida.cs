using System.Security.Cryptography;
using System.Text;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Core.Update;

namespace FrameworkConcept.Shell.Actualizacion;

/// <summary>
/// P2 — La semilla: una versión completa del contenido que viaja dentro del APK, «del mismo pipeline y firmada igual».
///
/// Qué hay en el APK (assets «semilla/…», copiados de Demo/semilla/ al compilar): una copia de la carpeta del canal
///   indice.json, indice.json.sig, paquetes/contenido-X.Y.Z.zip, definiciones/{encuestaId}/vN.json
/// Al cargarla se verifica la firma del índice con el conjunto de claves del cascarón, como cualquier índice; de ahí
/// salen la versión, el tamaño y el sha256 del zip, que el actualizador vuelve a comprobar al instalarla.
///
/// Diferencia deliberada con un índice bajado: NO se mira <c>expires</c>. La semilla viene adentro del binario firmado
/// por la tienda y puede instalarse meses después de compilada; su vigencia es la del binario.
/// </summary>
public sealed class SemillaEmbebida : ISemillaDeContenido
{
    private const string Carpeta = "semilla/";
    private readonly string _rutaDelZip;

    private SemillaEmbebida(EntradaDeContenido entrada, IReadOnlyList<DefinicionEncuesta> definiciones)
    {
        Version = entrada.Version;
        Size = entrada.Size;
        Sha256 = entrada.Sha256;
        _rutaDelZip = Carpeta + entrada.Ruta;
        Definiciones = definiciones;
    }

    public VersionSemantica Version { get; }
    public long Size { get; }
    public string Sha256 { get; }
    public IReadOnlyList<DefinicionEncuesta> Definiciones { get; }

    public Task<Stream> AbrirAsync(CancellationToken cancellationToken = default) =>
        FileSystem.OpenAppPackageFileAsync(_rutaDelZip);

    /// <summary>La semilla del APK, o null si no trae una o si no verifica (se anota el motivo en la bitácora).</summary>
    public static async Task<SemillaEmbebida?> CargarAsync(IVerificadorDeFirma verificador, string canal, int puenteMayor)
    {
        byte[] indiceJson, firmaJson;
        try
        {
            indiceJson = await LeerAsync("indice.json");
            firmaJson = await LeerAsync("indice.json.sig");
        }
        catch (Exception ex) when (ex is FileNotFoundException or IOException)
        {
            Bitacora.Log("semilla: el APK no trae semilla (Demo/semilla/ estaba vacía al compilar)");
            return null;
        }

        try
        {
            var firma = JsonDelContrato.Leer<FirmaDelIndice>(Encoding.UTF8.GetString(firmaJson));
            var verificacion = verificador.Verificar(indiceJson, firma);
            if (!verificacion.Valida)
            {
                Bitacora.Log($"semilla: firma rechazada ({verificacion.Motivo}); no se usa");
                return null;
            }

            var indice = JsonDelContrato.Leer<IndiceDelCanal>(Encoding.UTF8.GetString(indiceJson));
            if (indice.KeyId != firma.KeyId || indice.Canal != canal || indice.PuenteMayor != puenteMayor)
            {
                Bitacora.Log($"semilla: índice de {indice.Canal}/puente-{indice.PuenteMayor} con keyId {indice.KeyId}; este cascarón es {canal}/puente-{puenteMayor}");
                return null;
            }

            var entrada = indice.Entradas.Contenido.MaxBy(e => e.Version);
            if (entrada is null)
            {
                Bitacora.Log("semilla: el índice no trae contenido");
                return null;
            }

            var definiciones = new List<DefinicionEncuesta>();
            foreach (var d in indice.Entradas.Definiciones)
            {
                var bytes = await LeerAsync(d.Ruta);
                if (bytes.Length != d.Size || Convert.ToHexStringLower(SHA256.HashData(bytes)) != d.Sha256.ToLowerInvariant())
                {
                    Bitacora.Log($"semilla: definición {d.EncuestaId} v{d.Version} no coincide con el índice; se omite");
                    continue;
                }
                definiciones.Add(DefinicionEncuesta.Leer(Encoding.UTF8.GetString(bytes)));
            }

            Bitacora.Log($"semilla: contenido {entrada.Version}, {definiciones.Count} definición(es)");
            return new SemillaEmbebida(entrada, definiciones);
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or FileNotFoundException or IOException)
        {
            Bitacora.Error("semilla: ilegible", ex);
            return null;
        }
    }

    private static async Task<byte[]> LeerAsync(string ruta)
    {
        await using var flujo = await FileSystem.OpenAppPackageFileAsync(Carpeta + ruta);
        using var memoria = new MemoryStream();
        await flujo.CopyToAsync(memoria);
        return memoria.ToArray();
    }
}

/// <summary>
/// P1 — La semilla del APK sin leerla: el actualizador llama a <see cref="CargarAsync"/> sólo en el primer arranque o
/// cuando <see cref="Huella"/> cambió. Leer el zip y verificar el índice tardaba segundos en cada arranque en frío,
/// dentro del camino crítico del primer render.
/// </summary>
public sealed class FuenteDeSemillaEmbebida(IVerificadorDeFirma verificador, string canal, int puenteMayor) : IFuenteDeSemilla
{
    /// <summary>La semilla, si ya se cargó en este proceso (la muestra el diagnóstico).</summary>
    public SemillaEmbebida? Cargada { get; private set; }

    /// <summary>
    /// La instalación del APK: versión, número de build y hora de la última instalación. Cambia con cada
    /// «adb install -r» o actualización por tienda; leerla es una consulta al PackageManager, sin abrir archivos.
    /// </summary>
    public string Huella { get; } = CalcularHuella();

#if FC_E04_SEMILLA
    // E04 V1S — Una sola verificación del índice por proceso: la comparten la especulación del primer arranque y el
    // actualizador, que la vuelve a pedir al instalar la semilla.
    private Task<SemillaEmbebida?>? _carga;

    public async Task<ISemillaDeContenido?> CargarAsync(CancellationToken cancellationToken = default)
    {
        var carga = Volatile.Read(ref _carga);
        if (carga is null)
        {
            var nueva = SemillaEmbebida.CargarAsync(verificador, canal, puenteMayor);
            carga = Interlocked.CompareExchange(ref _carga, nueva, null) ?? nueva;
        }
        Cargada = await carga;
        return Cargada;
    }

    /// <summary>
    /// E04 V1S — La semilla lista para servirse desde el APK, sin copiarla: firma ES256 del índice (canal, puente, keyId)
    /// y después tamaño y sha256 del zip completo, los mismos que el actualizador comprueba antes de descomprimir. Ningún
    /// byte se sirve sin pasar las dos. null si algo no verifica (V1S se comporta entonces como V1).
    /// </summary>
    public async Task<FrameworkConcept.Shell.Hosting.SemillaEnMemoria?> ServibleAsync()
    {
        try
        {
            if (await CargarAsync() is not SemillaEmbebida semilla) return null;

            await using var flujo = await semilla.AbrirAsync();
            using var memoria = new MemoryStream(capacity: (int)Math.Min(semilla.Size + 1, int.MaxValue));
            var bufer = new byte[81920];
            int leidos;
            while ((leidos = await flujo.ReadAsync(bufer)) > 0)
            {
                memoria.Write(bufer, 0, leidos);
                if (memoria.Length > semilla.Size) break;   // no se lee más de Size + 1
            }

            if (memoria.Length != semilla.Size)
            {
                Bitacora.Log($"semilla: no servible desde el APK (el zip mide {memoria.Length}, el índice dice {semilla.Size})");
                return null;
            }
            var bytes = memoria.ToArray();
            if (Convert.ToHexStringLower(SHA256.HashData(bytes)) != semilla.Sha256.ToLowerInvariant())
            {
                Bitacora.Log("semilla: no servible desde el APK (el sha256 del zip no coincide con el índice firmado)");
                return null;
            }
            return new FrameworkConcept.Shell.Hosting.SemillaEnMemoria(semilla.Version, bytes);
        }
        catch (Exception ex)
        {
            Bitacora.Error("semilla: no servible desde el APK", ex);
            return null;
        }
    }
#else
    public async Task<ISemillaDeContenido?> CargarAsync(CancellationToken cancellationToken = default)
    {
        Cargada = await SemillaEmbebida.CargarAsync(verificador, canal, puenteMayor);
        return Cargada;
    }
#endif

    private static string CalcularHuella()
    {
        var instalada = 0L;
#if ANDROID
        var contexto = Android.App.Application.Context;
        if (contexto.PackageName is { } paquete)
            instalada = contexto.PackageManager?.GetPackageInfo(paquete, 0)?.LastUpdateTime ?? 0;
#endif
        return $"{AppInfo.VersionString}+{AppInfo.BuildString}@{instalada}";
    }
}
