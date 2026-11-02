// Nuevo (plan §F.8, «Predicado de salto»; H-059). Decide ANTES de llamar si una prueba con modelo
// puede correr. Distingue «no hay servidor» de «hay servidor pero el modelo no sabe hacer esto»,
// para no confundir el 501 de embeddings del servidor compartido (Sondeo §5) con una falla.
using System.Net.Http.Json;
using System.Text.Json;

namespace MovilidadUrbana.Asistente.Comun.AI;

/// <summary>Resultado del predicado. Si <see cref="Saltar" />, <see cref="Motivo" /> va al mensaje de Assert.Ignore.</summary>
public sealed record ResultadoDelPredicado(bool Saltar, string Motivo, string? VersionDeOllama, IReadOnlyList<string> Capacidades, string? Digest);

public static class PredicadoDeSalto
{
    public static readonly TimeSpan TimeoutVersion = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan TimeoutShow = TimeSpan.FromSeconds(10);

    /// <summary>Capacidades que pide el chat con herramientas.</summary>
    public static readonly string[] Chat = ["completion", "tools"];

    /// <summary>Capacidad que piden los embeddings.</summary>
    public static readonly string[] Embeddings = ["embedding"];

    /// <summary>
    /// GET /api/version (5 s) y POST /api/show {model} (10 s). Nunca lanza: todo problema es un
    /// motivo de salto. El mensaje no incluye el endpoint (R-PRIV, H-053).
    /// </summary>
    public static async Task<ResultadoDelPredicado> EvaluarAsync(
        string endpoint, string modelo, IReadOnlyCollection<string> capacidadesRequeridas, CancellationToken cancellationToken = default)
    {
        using var http = new HttpClient { BaseAddress = new Uri(endpoint) };
        string? version;
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeoutVersion);
            using var doc = JsonDocument.Parse(await http.GetStringAsync("/api/version", cts.Token));
            version = doc.RootElement.GetProperty("version").GetString();
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            return new(true, $"El servidor de modelos no responde a /api/version ({e.GetType().Name}).", null, [], null);
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeoutShow);
            using var respuesta = await http.PostAsJsonAsync("/api/show", new { model = modelo }, cts.Token);
            if (!respuesta.IsSuccessStatusCode)
            {
                return new(true, $"El modelo «{modelo}» no está en el servidor (HTTP {(int)respuesta.StatusCode}).", version, [], null);
            }
            using var doc = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(cts.Token));
            var capacidades = doc.RootElement.TryGetProperty("capabilities", out var c) && c.ValueKind == JsonValueKind.Array
                ? c.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList()
                : [];
            var faltan = capacidadesRequeridas.Where(r => !capacidades.Contains(r)).ToList();
            if (faltan.Count > 0)
            {
                return new(true, $"El modelo «{modelo}» no declara {string.Join(", ", faltan)} en /api/show.", version, capacidades, null);
            }
            return new(false, "ok", version, capacidades, await DigestAsync(http, modelo, cancellationToken));
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new(true, $"/api/show no respondió ({e.GetType().Name}).", version, [], null);
        }
    }

    /// <summary>
    /// ¿Está <paramref name="modelo" /> cargado según GET /api/ps? Se consulta antes de cada entrada
    /// en el servidor compartido: si desapareció, la llamada siguiente se registra «frio» (plan §E.1).
    /// null si no se pudo consultar.
    /// </summary>
    public static async Task<bool?> ModeloCargadoAsync(string endpoint, string modelo, CancellationToken cancellationToken = default)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(endpoint) };
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeoutVersion);
            using var doc = JsonDocument.Parse(await http.GetStringAsync("/api/ps", cts.Token));
            return doc.RootElement.GetProperty("models").EnumerateArray()
                .Any(m => m.GetProperty("name").GetString() == modelo || m.GetProperty("model").GetString() == modelo);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>Primeros 12 caracteres del digest del modelo, de GET /api/tags; null si no aparece.</summary>
    public static async Task<string?> DigestAsync(HttpClient http, string modelo, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeoutShow);
            using var doc = JsonDocument.Parse(await http.GetStringAsync("/api/tags", cts.Token));
            foreach (var m in doc.RootElement.GetProperty("models").EnumerateArray())
            {
                if (m.GetProperty("name").GetString() == modelo || m.GetProperty("model").GetString() == modelo)
                {
                    var digest = m.GetProperty("digest").GetString();
                    return digest is { Length: >= 12 } ? digest[..12] : digest;
                }
            }
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
        }
        return null;
    }
}
