using System.Collections.Concurrent;
using System.Diagnostics;
using FrameworkConcept.Core.Red;

namespace FrameworkConcept.Shell.Red;

/// <summary>
/// Borrador §6 «Actividad» — Todo HttpClient del framework pasa por acá: registra método, URL, estado, duración y bytes
/// (registro circular en memoria, ParametrosDeRed.RegistroDeLlamadas; se ve en Diagnóstico) y le avisa al monitor si la
/// llamada falló por red o 5xx (→ RedSinServidor) o si respondió (→ confirma el servidor).
///
/// Lo arma FabricaHttp: es el manejador MÁS INTERNO de la cadena (debajo de ManejadorDeSesion), así ve la respuesta
/// real del servidor. Un 4xx NO es una falla de red: el servidor respondió (DISENO §2.2).
/// </summary>
public sealed class ManejadorDeRed(IMonitorDeRed monitor, string nombre) : DelegatingHandler
{
    /// <summary>«receptor» o «canal»: para el registro.</summary>
    public string Nombre { get; } = nombre;

    public IMonitorDeRed Monitor { get; } = monitor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var reloj = Stopwatch.StartNew();
        var enviados = request.Content?.Headers.ContentLength ?? 0;
        try
        {
            var respuesta = await base.SendAsync(request, cancellationToken);
            var codigo = (int)respuesta.StatusCode;
            RegistroDeLlamadas.Anotar(new LlamadaRegistrada(DateTimeOffset.Now, Nombre, request.Method.Method, request.RequestUri,
                codigo.ToString(System.Globalization.CultureInfo.InvariantCulture), reloj.Elapsed, enviados, respuesta.Content.Headers.ContentLength));

            if (codigo >= 500) Monitor.InformarLlamadaFallida($"{Nombre} HTTP {codigo}");
            else Monitor.InformarLlamadaExitosa();
            return respuesta;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Red caída, DNS, TLS o el Timeout del HttpClient (llega como TaskCanceledException sin que el que llama
            // haya cancelado): el servidor no respondió.
            var tipo = ex is TaskCanceledException ? "tiempo agotado" : ex.GetType().Name;
            RegistroDeLlamadas.Anotar(new LlamadaRegistrada(DateTimeOffset.Now, Nombre, request.Method.Method, request.RequestUri,
                tipo, reloj.Elapsed, enviados, null));
            Monitor.InformarLlamadaFallida($"{Nombre} {tipo}");
            throw;
        }
    }
}

/// <summary>Una fila del registro de llamadas (borrador §6 «Actividad»).</summary>
/// <param name="Resultado">Código HTTP, o el tipo de falla si no hubo respuesta.</param>
/// <param name="BytesRecibidos">null si el servidor no informó Content-Length (o no respondió).</param>
public sealed record LlamadaRegistrada(
    DateTimeOffset Cuando, string Cliente, string Metodo, Uri? Url, string Resultado, TimeSpan Duracion, long BytesEnviados, long? BytesRecibidos);

/// <summary>
/// El registro circular de llamadas del proceso: las últimas <see cref="ParametrosDeRed.RegistroDeLlamadas"/>. En
/// memoria (DISENO §2.4, arq): el borrador §6 lo pide en SQLite; la demo no cambia el esquema (C-1). Es estático porque
/// FabricaHttp crea un ManejadorDeRed por cliente y todos anotan en el mismo registro.
/// </summary>
public static class RegistroDeLlamadas
{
    private static readonly ConcurrentQueue<LlamadaRegistrada> Filas = new();

    public static void Anotar(LlamadaRegistrada llamada)
    {
        Filas.Enqueue(llamada);
        while (Filas.Count > ParametrosDeRed.RegistroDeLlamadas && Filas.TryDequeue(out _)) { }
    }

    /// <summary>Las más recientes primero.</summary>
    public static IReadOnlyList<LlamadaRegistrada> Ultimas(int cuantas) => [.. Filas.Reverse().Take(cuantas)];

    public static int Total => Filas.Count;
}
