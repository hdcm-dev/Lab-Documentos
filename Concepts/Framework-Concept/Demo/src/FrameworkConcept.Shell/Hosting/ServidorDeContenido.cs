using System.Text;

namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// Sirve al HybridWebView los archivos de la versión activa del contenido, atrapando cada pedido con
/// <c>WebResourceRequested</c> (.NET 10). Es lo que funcionó en el ensayo E01, con sus condiciones:
///
///   C-1  el contenido descargado se sirve desde acá, no con HybridRoot (HybridRoot sólo ve archivos del APK);
///   C-2  <c>_framework/hybridwebview.js</c> y <c>POST /__hwvInvokeDotNet</c> NO se atrapan: son el transporte del
///        puente y los sirve el propio HybridWebView;
///   C-4  los archivos se devuelven tal cual (el paquete trae archivos planos, sin .br ni .gz).
/// </summary>
public sealed class ServidorDeContenido
{
    /// <summary>El origen fijo de la página en Android, medido en E01 ([C6]): https://0.0.0.1/.</summary>
    public const string HostLocal = "0.0.0.1";

    /// <summary>El archivo cuyo pedido prueba que el contenido EMPEZÓ a cargar: el runtime de .NET en el navegador.</summary>
    public const string ArchivoDeInicioDeCarga = "_framework/dotnet.js";

    private int _contador;
    private int _cargaAvisada;

    /// <summary>Directorio de la versión activa (content/versions/vN). Lo fija la pantalla de contenido al arrancar.</summary>
    public string? Raiz { get; set; }

    /// <summary>
    /// P2 — Se dispara una vez por proceso, cuando el WebView pide <see cref="ArchivoDeInicioDeCarga"/>. Es la evidencia
    /// de que el contenido llegó a cargar: un arranque que se corta antes no cuenta para el rollback por marca.
    /// </summary>
    public event Action? CargaIniciada;

    public void AlPedirRecurso(object? sender, WebViewWebResourceRequestedEventArgs e)
    {
        var n = Interlocked.Increment(ref _contador);
        var uri = e.Uri;

        // P3 — Sólo el origen local. Cualquier pedido a otro origen (una navegación, un script o un fetch de afuera)
        // se corta acá, además de la CSP del index.html.
        if (!string.Equals(uri.Host, HostLocal, StringComparison.OrdinalIgnoreCase))
        {
            e.Handled = true;
            e.SetResponse(403, "Forbidden", "text/plain", new MemoryStream(Encoding.UTF8.GetBytes("Origen no permitido")));
            Bitacora.Log($"servidor: bloqueado {e.Method} {uri}");
            return;
        }

        var ruta = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');
        if (ruta.Length == 0) ruta = "index.html";

        // C-2 — el transporte del puente no se toca.
        if (ruta.StartsWith("__hwv", StringComparison.Ordinal) || ruta == "_framework/hybridwebview.js")
            return;

        var archivo = Resolver(ruta);
        if (archivo is null)
        {
            if (n <= 60) Bitacora.Log($"servidor: req#{n} {e.Method} /{ruta} → sin manejar");
            return;
        }

        e.Handled = true;
        e.SetResponse(200, "OK", TipoMime(archivo), File.OpenRead(archivo));
        if (n <= 60) Bitacora.Log($"servidor: req#{n} {e.Method} /{ruta}");

        if (ruta == ArchivoDeInicioDeCarga && Interlocked.Exchange(ref _cargaAvisada, 1) == 0)
            CargaIniciada?.Invoke();
    }

    /// <summary>El archivo pedido dentro de la raíz, o index.html para las rutas de página de Blazor («/encuesta»).</summary>
    private string? Resolver(string ruta)
    {
        if (Raiz is null || !Directory.Exists(Raiz)) return null;

        var raizCompleta = Path.GetFullPath(Raiz) + Path.DirectorySeparatorChar;
        var candidato = Path.GetFullPath(Path.Combine(Raiz, ruta));

        // Nada fuera de la raíz: «../» no sale del directorio de la versión.
        if (!candidato.StartsWith(raizCompleta, StringComparison.Ordinal)) return null;
        if (File.Exists(candidato)) return candidato;

        // Una ruta sin extensión es una página del enrutador de Blazor: se responde index.html y Blazor la resuelve.
        var indice = Path.Combine(Raiz, "index.html");
        return Path.HasExtension(ruta) || !File.Exists(indice) ? null : indice;
    }

    private static string TipoMime(string archivo) => Path.GetExtension(archivo).ToLowerInvariant() switch
    {
        ".html" => "text/html",
        ".js" or ".mjs" => "text/javascript",
        ".css" => "text/css",
        ".wasm" => "application/wasm",
        ".json" or ".webmanifest" => "application/json",
        ".png" => "image/png",
        ".svg" => "image/svg+xml",
        ".ico" => "image/x-icon",
        ".woff2" => "font/woff2",
        ".txt" => "text/plain",
        _ => "application/octet-stream"
    };
}
