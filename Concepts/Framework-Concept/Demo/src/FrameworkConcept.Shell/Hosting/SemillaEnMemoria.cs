#if FC_E04_SEMILLA
using System.IO.Compression;
using FrameworkConcept.Core.Contracts;

namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// E04 V1S — La semilla del APK, ya verificada (firma del índice y sha256 del zip), servida tal cual desde memoria en el
/// primer arranque, mientras el actualizador la instala en disco por el camino de siempre. Son los mismos bytes que el
/// actualizador vuelve a comprobar antes de descomprimir; por eso la carga desde acá cuenta como la de la decidida.
/// </summary>
public sealed class SemillaEnMemoria
{
    private readonly ZipArchive _zip;
    private readonly object _candado = new();   // ZipArchive no es seguro entre hilos; los pedidos llegan en paralelo

    public SemillaEnMemoria(VersionSemantica version, byte[] zipVerificado)
    {
        Version = version;
        _zip = new ZipArchive(new MemoryStream(zipVerificado, writable: false), ZipArchiveMode.Read);
    }

    public VersionSemantica Version { get; }

    /// <summary>La «raíz» desde la que se sirve: no es un directorio y nunca coincide con una ruta de disco.</summary>
    public string Raiz => "apk:semilla/" + Version;

    /// <summary>
    /// El contenido de la entrada pedida, o index.html para las rutas de página de Blazor («/encuesta»), con la misma
    /// regla que el servidor en disco. Devuelve también la ruta resuelta (para el tipo MIME). null si no existe.
    /// </summary>
    public (string Ruta, byte[] Bytes)? Leer(string ruta)
    {
        if (ruta.Length == 0 || ruta.Contains('\\') || ruta.StartsWith('/') || ruta.Contains(':')
            || ruta.Split('/').Any(p => p == ".." || p == "."))
            return null;

        lock (_candado)
        {
            var entrada = _zip.GetEntry(ruta);
            if (entrada is null && !Path.HasExtension(ruta))
            {
                entrada = _zip.GetEntry("index.html");
                ruta = "index.html";
            }
            if (entrada is null || entrada.FullName.EndsWith('/')) return null;

            using var flujo = entrada.Open();
            using var memoria = new MemoryStream((int)entrada.Length);
            flujo.CopyTo(memoria);
            return (ruta, memoria.ToArray());
        }
    }
}
#endif
