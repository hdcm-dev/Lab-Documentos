using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace TusDemo.ClienteConsola;

/// <summary>
/// Cliente tus 1.0.0 mínimo sobre HttpClient: crear (POST), consultar el avance (HEAD) y enviar (PATCH).
/// Es el mismo esqueleto que usaría una aplicación móvil (.NET MAUI).
/// </summary>
public sealed class ClienteTus(HttpClient http)
{
    private const string Version = "1.0.0";

    private static string B64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s));

    public async Task<Uri> CrearAsync(Uri endpoint, long largo, string nombre, string tipo, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
        req.Headers.Add("Tus-Resumable", Version);
        req.Headers.Add("Upload-Length", largo.ToString());
        req.Headers.Add("Upload-Metadata", $"filename {B64(nombre)},filetype {B64(tipo)}");
        using var res = await http.SendAsync(req, ct);
        if (res.StatusCode != HttpStatusCode.Created)
            throw new HttpRequestException($"Crear: {(int)res.StatusCode} {await res.Content.ReadAsStringAsync(ct)}");
        return new Uri(endpoint, res.Headers.Location!); // Location puede ser relativa
    }

    public async Task<long> AvanceAsync(Uri suba, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Head, suba);
        req.Headers.Add("Tus-Resumable", Version);
        using var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        return long.Parse(res.Headers.GetValues("Upload-Offset").First());
    }

    /// <summary>
    /// Envía desde lo que el servidor ya tiene hasta completar. <paramref name="alEnviarTrozo"/> se llama
    /// después de cada trozo aceptado (sirve para mostrar avance o, en la demo, para simular un corte).
    /// </summary>
    public async Task EnviarAsync(Uri suba, Stream origen, long largo, int trozo, CancellationToken ct,
        Action<long>? alEnviarTrozo = null)
    {
        var offset = await AvanceAsync(suba, ct);
        var buffer = new byte[trozo];
        while (offset < largo)
        {
            origen.Seek(offset, SeekOrigin.Begin);
            var n = await origen.ReadAsync(buffer.AsMemory(0, (int)Math.Min(trozo, largo - offset)), ct);
            using var req = new HttpRequestMessage(HttpMethod.Patch, suba) { Content = new ByteArrayContent(buffer, 0, n) };
            req.Headers.Add("Tus-Resumable", Version);
            req.Headers.Add("Upload-Offset", offset.ToString());
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");
            using var res = await http.SendAsync(req, ct);
            if (res.StatusCode == HttpStatusCode.Conflict) { offset = await AvanceAsync(suba, ct); continue; }
            if (res.StatusCode != HttpStatusCode.NoContent)
                throw new HttpRequestException($"Enviar: {(int)res.StatusCode}");
            offset = long.Parse(res.Headers.GetValues("Upload-Offset").First());
            alEnviarTrozo?.Invoke(offset);
        }
    }
}
