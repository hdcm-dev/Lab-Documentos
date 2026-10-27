using System.IO.Compression;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Contenido;

namespace FrameworkConcept.Core.Update;

/// <summary>
/// <see cref="IAlmacenDeContenido"/> sobre el sistema de archivos, con sólo System.IO: el cascarón la usa con
/// <c>FileSystem.AppDataDirectory/content</c> como raíz. Disposición (P2):
///
///   content/staging/vN/          lo bajado y verificado (se descomprime en vN.tmp y se renombra al terminar)
///   content/versions/vN/         versiones activables, inmutables
///   content/puntero.json         current, previous, pendiente; se reemplaza con un rename
///   content/fallidas.json        versiones que fallaron en este teléfono
///   content/ultimo-indice.json   el último índice que pasó la verificación
///   content/tmp/                 descompresiones a medio hacer y directorios a borrar (se vacía al abrir)
///
/// Todo vive en el mismo volumen: así un <c>File.Move</c>/<c>Directory.Move</c> es un rename de una sola operación.
/// </summary>
public sealed class AlmacenDeContenidoEnDisco : IAlmacenDeContenido
{
    private readonly string _raiz;
    private readonly string _staging;
    private readonly string _versiones;
    private readonly string _tmp;

    public AlmacenDeContenidoEnDisco(string raiz)
    {
        _raiz = raiz;
        _staging = Path.Combine(raiz, "staging");
        _versiones = Path.Combine(raiz, "versions");
        _tmp = Path.Combine(raiz, "tmp");
        Directory.CreateDirectory(_staging);
        Directory.CreateDirectory(_versiones);
        // Lo que quedó en tmp/ es de una operación cortada: nunca fue staging ni versión.
        if (Directory.Exists(_tmp)) Directory.Delete(_tmp, recursive: true);
        Directory.CreateDirectory(_tmp);
    }

    private string ArchivoPuntero => Path.Combine(_raiz, "puntero.json");
    private string ArchivoFallidas => Path.Combine(_raiz, "fallidas.json");
    private string ArchivoIndice => Path.Combine(_raiz, "ultimo-indice.json");

    public Task<PunteroDeContenido?> LeerPunteroAsync(CancellationToken cancellationToken = default) =>
        LeerJsonAsync<PunteroDeContenido>(ArchivoPuntero, cancellationToken);

    public Task ReemplazarPunteroAsync(PunteroDeContenido puntero, CancellationToken cancellationToken = default) =>
        EscribirConRenameAsync(ArchivoPuntero, puntero, cancellationToken);

    public async Task<IReadOnlySet<VersionSemantica>> LeerFallidasAsync(CancellationToken cancellationToken = default) =>
        (await LeerJsonAsync<List<VersionSemantica>>(ArchivoFallidas, cancellationToken))?.ToHashSet() ?? [];

    public async Task AgregarFallidaAsync(VersionSemantica version, CancellationToken cancellationToken = default)
    {
        var fallidas = (await LeerFallidasAsync(cancellationToken)).ToList();
        if (!fallidas.Contains(version))
            await EscribirConRenameAsync(ArchivoFallidas, fallidas.Append(version).ToList(), cancellationToken);
    }

    public async Task DescomprimirEnStagingAsync(VersionSemantica version, Stream zip, CancellationToken cancellationToken = default)
    {
        // Una sola versión en staging: lo anterior se descarta.
        await LimpiarStagingAsync(cancellationToken);

        var destino = Path.Combine(_staging, Nombre(version));
        var temporal = Path.Combine(_tmp, "descomprimir-" + Guid.NewGuid().ToString("N"));
        // .NET rechaza las entradas que escaparían del directorio («zip slip») con IOException.
        try
        {
            await ZipFile.ExtractToDirectoryAsync(zip, temporal, overwriteFiles: false, cancellationToken);
        }
        catch (IOException ex) when (ex is not FileNotFoundException)
        {
            BorrarDirectorio(temporal);
            throw new InvalidDataException("el zip no se pudo descomprimir: " + ex.Message, ex);
        }
        Directory.Move(temporal, destino);   // recién acá «está» en staging
    }

    public Task<VersionSemantica?> VersionEnStagingAsync(CancellationToken cancellationToken = default)
    {
        var version = Directory.EnumerateDirectories(_staging)
            .Select(d => VersionSemantica.TryParse(Path.GetFileName(d), out var v) ? v : null)
            .OfType<VersionSemantica>()
            .Max();
        return Task.FromResult(version);
    }

    public async Task<ManifiestoContenido?> LeerManifiestoAsync(VersionSemantica version, CancellationToken cancellationToken = default)
    {
        foreach (var carpeta in new[] { _staging, _versiones })
        {
            var archivo = Path.Combine(carpeta, Nombre(version), ManifiestoContenido.NombreDeArchivo);
            if (File.Exists(archivo)) return await LeerJsonAsync<ManifiestoContenido>(archivo, cancellationToken);
        }
        return null;
    }

    public Task PromoverAsync(VersionSemantica version, CancellationToken cancellationToken = default)
    {
        Directory.Move(Path.Combine(_staging, Nombre(version)), DirectorioDe(version));
        return Task.CompletedTask;
    }

    public bool ExisteVersion(VersionSemantica version) => Directory.Exists(DirectorioDe(version));

    public string DirectorioDe(VersionSemantica version) => Path.Combine(_versiones, Nombre(version));

    public Task BorrarVersionAsync(VersionSemantica version, CancellationToken cancellationToken = default)
    {
        BorrarDirectorio(DirectorioDe(version));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VersionSemantica>> ListarVersionesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VersionSemantica> versiones = [.. Directory.EnumerateDirectories(_versiones)
            .Select(d => VersionSemantica.TryParse(Path.GetFileName(d), out var v) ? v : null)
            .OfType<VersionSemantica>()];
        return Task.FromResult(versiones);
    }

    public Task LimpiarStagingAsync(CancellationToken cancellationToken = default)
    {
        foreach (var directorio in Directory.EnumerateDirectories(_staging))
            BorrarDirectorio(directorio);
        return Task.CompletedTask;
    }

    public Task<IndiceDelCanal?> LeerUltimoIndiceValidoAsync(CancellationToken cancellationToken = default) =>
        LeerJsonAsync<IndiceDelCanal>(ArchivoIndice, cancellationToken);

    public Task GuardarUltimoIndiceValidoAsync(IndiceDelCanal indice, CancellationToken cancellationToken = default) =>
        EscribirConRenameAsync(ArchivoIndice, indice, cancellationToken);

    // El nombre del directorio es la versión entera, con su +commit: es la identidad del paquete (P7).
    private static string Nombre(VersionSemantica version) => version.ToString();

    private static async Task<T?> LeerJsonAsync<T>(string archivo, CancellationToken ct) where T : class =>
        File.Exists(archivo) ? JsonDelContrato.Leer<T>(await File.ReadAllTextAsync(archivo, ct)) : null;

    /// <summary>Escribe un temporal, lo baja a disco y lo renombra encima: o queda el archivo viejo o el nuevo, nunca medio.</summary>
    private static async Task EscribirConRenameAsync<T>(string archivo, T valor, CancellationToken ct)
    {
        var temporal = archivo + ".tmp";
        await using (var flujo = new FileStream(temporal, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await flujo.WriteAsync(System.Text.Encoding.UTF8.GetBytes(JsonDelContrato.Escribir(valor)), ct);
            flujo.Flush(flushToDisk: true);
        }
        File.Move(temporal, archivo, overwrite: true);
    }

    /// <summary>Se mueve a tmp/ antes de borrar: si el borrado se corta, lo que queda no parece una versión.</summary>
    private void BorrarDirectorio(string directorio)
    {
        if (!Directory.Exists(directorio)) return;
        var basura = Path.Combine(_tmp, "borrar-" + Guid.NewGuid().ToString("N"));
        Directory.Move(directorio, basura);
        Directory.Delete(basura, recursive: true);
    }
}
