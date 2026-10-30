using System.Text;
using FrameworkConcept.Core.Update;

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
public sealed class ServidorDeContenido(PuertaDelArranque puerta)
{
    /// <summary>El origen fijo de la página en Android, medido en E01 ([C6]): https://0.0.0.1/.</summary>
    public const string HostLocal = "0.0.0.1";

    /// <summary>El archivo cuyo pedido prueba que el contenido EMPEZÓ a cargar: el runtime de .NET en el navegador.</summary>
    public const string ArchivoDeInicioDeCarga = "_framework/dotnet.js";

    private int _contador;
    private int _cargaAvisada;
    private string? _ultimoFramework;
    private string? _raizDeLaCarga;

    /// <summary>
    /// Directorio de la versión activa (content/versions/vN). Lo fija la pantalla de contenido al arrancar; con E04, la
    /// de arranque con la versión especulada, o la preparación cuando el actualizador decide.
    /// </summary>
    public string? Raiz { get; set; }

#if FC_E04_SEMILLA
    /// <summary>
    /// E04 V1S — La semilla del APK verificada, servida desde memoria en el primer arranque. Se sirve mientras
    /// <see cref="Raiz"/> sea su identificador («apk:semilla/vX»); se libera cuando el arranque decide otra cosa.
    /// </summary>
    public SemillaEnMemoria? Semilla { get; set; }
#endif

    /// <summary>
    /// E04 — Si la carga servida desde <paramref name="raizServida"/> es la de la versión decidida. Sin V1S es la
    /// comparación de directorios de siempre; con V1S también lo es la semilla del APK de la misma versión: son los mismos
    /// bytes (el sha256 firmado del zip que el actualizador vuelve a comprobar antes de descomprimir).
    /// </summary>
    public bool EsLaDecidida(string? raizServida, ResultadoDeArranque decidido)
    {
        if (string.Equals(raizServida, decidido.Directorio, StringComparison.Ordinal)) return true;
#if FC_E04_SEMILLA
        return Semilla is { } s && raizServida is not null && string.Equals(raizServida, s.Raiz, StringComparison.Ordinal)
            && s.Version.Equals(decidido.Version);
#else
        return false;
#endif
    }

    /// <summary>
    /// P2 — Se dispara una vez por carga, cuando el WebView pide <see cref="ArchivoDeInicioDeCarga"/>. Es la evidencia
    /// de que el contenido llegó a cargar: un arranque que se corta antes no cuenta para el rollback por marca. Lleva la
    /// raíz desde la que se sirvió (E04: puede ser una versión especulada que después se descarta).
    /// </summary>
    public event Action<string?>? CargaIniciada;

    /// <summary>E04 — El último pedido atendido bajo _framework/ («req#N /ruta t=…ms»), para la bitácora del saludo y de Listo.</summary>
    public string UltimoFramework => Volatile.Read(ref _ultimoFramework) ?? "sin _framework";

    /// <summary>E04 — La raíz desde la que se sirvió el runtime de la carga en curso (null si todavía no se pidió).</summary>
    public string? RaizDeLaCarga => Volatile.Read(ref _raizDeLaCarga);

    /// <summary>E04 — Antes de recargar por una especulación descartada: la carga nueva vuelve a contar y a avisar.</summary>
    public void Reiniciar()
    {
        Interlocked.Exchange(ref _contador, 0);
        Interlocked.Exchange(ref _cargaAvisada, 0);
        Volatile.Write(ref _raizDeLaCarga, null);
    }

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

#if FC_E04
        // E04 — Sin versión especulada (primer arranque, R1) todavía no hay raíz: se espera la decisión del actualizador.
        // Corre en el hilo de E/S del WebView (ShouldInterceptRequest), no en el de la interfaz.
        if (Raiz is null && !puerta.Preparado.IsCompleted)
        {
            Bitacora.Log($"servidor: req#{n} /{ruta} espera la decisión del actualizador");
            try { puerta.Preparado.Wait(TimeSpan.FromSeconds(30)); }
            catch (AggregateException) { /* la preparación falló: la página se descarta igual */ }
        }
#else
        _ = puerta;
#endif
        var raiz = Raiz;
#if FC_E04_SEMILLA
        if (Semilla is { } semilla && raiz is not null && string.Equals(raiz, semilla.Raiz, StringComparison.Ordinal))
        {
            if (semilla.Leer(ruta) is not { } leido)
            {
                if (n <= 60) Bitacora.Log($"servidor: req#{n} {e.Method} /{ruta} → sin manejar (semilla del APK)");
                return;
            }
            e.Handled = true;
            e.SetResponse(200, "OK", TipoMime(leido.Ruta), new MemoryStream(leido.Bytes, writable: false));
            if (n <= 60) Bitacora.Log($"servidor: req#{n} {e.Method} /{ruta} (semilla del APK)");
            Atendido(n, ruta, raiz);
            return;
        }
#endif
        var archivo = Resolver(raiz, ruta);
        if (archivo is null)
        {
            if (n <= 60) Bitacora.Log($"servidor: req#{n} {e.Method} /{ruta} → sin manejar");
            return;
        }

        e.Handled = true;
        e.SetResponse(200, "OK", TipoMime(archivo), File.OpenRead(archivo));
        if (n <= 60) Bitacora.Log($"servidor: req#{n} {e.Method} /{ruta}");
        Atendido(n, ruta, raiz);
    }

    // Lo que sigue a cada pedido servido: el último _framework y el aviso de inicio de carga (P2).
    private void Atendido(int n, string ruta, string? raiz)
    {
        if (ruta.StartsWith("_framework/", StringComparison.Ordinal))
            Volatile.Write(ref _ultimoFramework, $"req#{n} /{ruta} t={Bitacora.MsDesdeArranque()}ms");

        if (ruta == ArchivoDeInicioDeCarga && Interlocked.Exchange(ref _cargaAvisada, 1) == 0)
        {
            Volatile.Write(ref _raizDeLaCarga, raiz);
            CargaIniciada?.Invoke(raiz);
        }
    }

    /// <summary>El archivo pedido dentro de la raíz, o index.html para las rutas de página de Blazor («/encuesta»).</summary>
    private static string? Resolver(string? raiz, string ruta)
    {
        if (raiz is null || !Directory.Exists(raiz)) return null;

        var raizCompleta = Path.GetFullPath(raiz) + Path.DirectorySeparatorChar;
        var candidato = Path.GetFullPath(Path.Combine(raiz, ruta));

        // Nada fuera de la raíz: «../» no sale del directorio de la versión.
        if (!candidato.StartsWith(raizCompleta, StringComparison.Ordinal)) return null;
        if (File.Exists(candidato)) return candidato;

        // Una ruta sin extensión es una página del enrutador de Blazor: se responde index.html y Blazor la resuelve.
        var indice = Path.Combine(raiz, "index.html");
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
