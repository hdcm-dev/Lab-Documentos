// Nuevo (plan §E.1, «Guarda de truncado»; R-03). Ollama no avisa cuando corta el prompt: lo único
// que la app ve es que el servidor informó menos tokens de entrada que los que mandó.
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Comun.AI;

public static class GuardaDeTruncado
{
    /// <summary>Caracteres por token de la estimación; DEP-03 la calibra (variable RLLM_CARACTERES_POR_TOKEN).</summary>
    public static double CaracteresPorToken { get; set; } =
        double.TryParse(Environment.GetEnvironmentVariable("RLLM_CARACTERES_POR_TOKEN"),
            System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var c) && c > 0 ? c : 4.0;

    /// <summary>
    /// M-F (REC-21 C): la calibración es por modelo, porque el largo depende del tokenizador. Modelo →
    /// caracteres por token, de RLLM_CARACTERES_POR_TOKEN_POR_MODELO («modelo=valor;modelo=valor»).
    /// Un modelo sin calibrar usa <see cref="CaracteresPorToken" />. Vacía por defecto: los valores
    /// salen de una corrida de DEP-03, no del código.
    /// </summary>
    public static Dictionary<string, double> CalibracionPorModelo { get; } =
        LeerCalibracion(Environment.GetEnvironmentVariable("RLLM_CARACTERES_POR_TOKEN_POR_MODELO"));

    public const double FraccionMinima = 0.8;

    /// <summary>Caracteres por token del modelo de la llamada (o el valor general si no está calibrado).</summary>
    public static double CaracteresPorTokenDe(string? modelo) =>
        modelo is not null && CalibracionPorModelo.TryGetValue(modelo, out var c) ? c : CaracteresPorToken;

    /// <summary>
    /// Estimación del adaptador: caracteres del prompt completo sobre los caracteres por token del
    /// modelo de la llamada. El modelo es <c>options.ModelId</c> si la llamada lo fija; si no,
    /// <paramref name="modelo" /> (el del cliente, que pasa el escritor del registro).
    /// </summary>
    public static int Estimar(IEnumerable<ChatMessage> mensajes, ChatOptions? options = null, string? modelo = null)
    {
        var caracteres = mensajes.Sum(m => m.Text?.Length ?? 0) + (options?.Instructions?.Length ?? 0);
        if (options?.Tools is { } herramientas)
        {
            caracteres += herramientas.OfType<AIFunctionDeclaration>()
                .Sum(f => f.Name.Length + (f.Description?.Length ?? 0) + f.JsonSchema.GetRawText().Length);
        }
        return (int)Math.Ceiling(caracteres / CaracteresPorTokenDe(options?.ModelId ?? modelo));
    }

    private static Dictionary<string, double> LeerCalibracion(string? texto)
    {
        var mapa = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var par in (texto ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var i = par.LastIndexOf('=');
            if (i > 0 && double.TryParse(par[(i + 1)..], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var c) && c > 0)
            {
                mapa[par[..i].Trim()] = c;
            }
        }
        return mapa;
    }

    /// <summary>
    /// true: puede haber truncado (no cuenta para ninguna tasa). null: no verificable (vía /v1, o
    /// sin <c>InputTokenCount</c>). false: el servidor procesó lo que se mandó.
    /// </summary>
    public static bool? Evaluar(long? tokensInformados, int estimacion, int? numCtx, Adaptador adaptador)
    {
        if (adaptador != Adaptador.OllamaSharp || tokensInformados is null || numCtx is null) return null;
        return tokensInformados < FraccionMinima * estimacion || tokensInformados >= numCtx;
    }
}
