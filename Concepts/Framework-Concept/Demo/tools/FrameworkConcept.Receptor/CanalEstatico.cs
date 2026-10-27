using FrameworkConcept.Core.Contracts.Canal;

namespace FrameworkConcept.Receptor;

/// <summary>
/// El servidor estático del canal (P2): devuelve archivos de una carpeta, tal cual, sin lógica.
///
///   GET /{canal}/puente-{mayor}/indice.json
///   GET /{canal}/puente-{mayor}/indice.json.sig
///   GET /{canal}/puente-{mayor}/paquetes/…
///   GET /{canal}/puente-{mayor}/definiciones/…
///
/// Sólo esas rutas: el teléfono entra siempre por el índice firmado, y lo demás de la carpeta (el estado y la
/// bitácora de quien publica) no se sirve. En producción esto es un servidor estático cualquiera con HTTPS.
/// </summary>
internal static class CanalEstatico
{
    public static void Mapear(WebApplication app, string raiz)
    {
        var raizCompleta = Path.GetFullPath(raiz);

        app.MapGet("/{canal}/puente-{mayor:int}/{**ruta}", (string canal, int mayor, string? ruta, HttpContext http, ILogger<Program> log) =>
        {
            if (!Canales.Todos.Contains(canal) || ruta is null || !EsRutaPublica(ruta))
                return Results.NotFound();

            var carpeta = Path.GetFullPath(Path.Combine(raizCompleta, RutasDelCanal.Carpeta(canal, mayor)));
            var archivo = Path.GetFullPath(Path.Combine(carpeta, ruta));

            // Nada fuera de la carpeta del canal, aunque la ruta traiga «..».
            if (!archivo.StartsWith(carpeta + Path.DirectorySeparatorChar, StringComparison.Ordinal) || !File.Exists(archivo))
                return Results.NotFound();

            log.LogInformation("canal: {Canal}/puente-{Mayor}/{Ruta} → {Ip}", canal, mayor, ruta, http.Connection.RemoteIpAddress);

            // El índice cambia en el mismo lugar: que nadie lo guarde en caché. Los paquetes no cambian nunca (P7).
            http.Response.Headers.CacheControl = ruta.StartsWith("indice.json", StringComparison.Ordinal) ? "no-cache" : "public, max-age=31536000, immutable";
            return Results.File(archivo, TipoDeContenido(archivo), enableRangeProcessing: true);
        });
    }

    private static bool EsRutaPublica(string ruta) =>
        ruta is "indice.json" or "indice.json.sig"
        || ruta.StartsWith("paquetes/", StringComparison.Ordinal)
        || ruta.StartsWith("definiciones/", StringComparison.Ordinal);

    private static string TipoDeContenido(string archivo) => Path.GetExtension(archivo) switch
    {
        ".json" or ".sig" => "application/json; charset=utf-8",
        ".zip" => "application/zip",
        _ => "application/octet-stream"
    };
}
