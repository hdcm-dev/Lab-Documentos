// Nuevo (plan §E.1, tabla de variables canónicas; H-058). Ningún valor de infraestructura vive en
// el repo: todo sale del entorno, con los defaults de la tabla.
using System.Globalization;

namespace MovilidadUrbana.Asistente.Comun.AI;

/// <summary>Tipo del servidor de chat; va al campo <c>servidor</c> del registro.</summary>
public enum TipoDeServidor { Propio, Compartido, Nube }

/// <summary>De dónde salió el <c>num_ctx</c>; va al campo <c>num_ctx_origen</c> del registro.</summary>
public static class OrigenDelNumCtx
{
    public const string Arnes = "arnes";
    public const string ArnesPsTrasCarga = "arnes-ps-tras-carga";
    public const string Default = "default";
}

/// <summary>La corrida no se puede hacer con esta configuración; el mensaje dice por qué.</summary>
public sealed class ConfiguracionInvalidaException(string mensaje) : Exception(mensaje);

/// <summary>
/// Las variables canónicas, leídas una vez. <see cref="Validar" /> aplica las reglas de §E.1 que
/// dependen sólo de la configuración (p. ej. compartido sin <c>num_ctx</c> del arnés → rechazo).
/// </summary>
public sealed record ConfiguracionDelModelo
{
    public const int NumCtxPorDefecto = 8192;
    public const string EndpointPorDefecto = "http://localhost:11434";

    public string Endpoint { get; init; } = EndpointPorDefecto;
    public string Modelo { get; init; } = "qwen3.5:4b";
    public TipoDeServidor Servidor { get; init; } = TipoDeServidor.Propio;
    public int NumCtx { get; init; } = NumCtxPorDefecto;
    public string NumCtxOrigen { get; init; } = OrigenDelNumCtx.Default;
    public string PropioEndpoint { get; init; } = EndpointPorDefecto;
    public string PropioModelo { get; init; } = "qwen3.5:4b";
    public string EmbedEndpoint { get; init; } = EndpointPorDefecto;
    public string EmbedModelo { get; init; } = "qwen3-embedding:0.6b";
    public string EmbPlan { get; init; } = "EMB-B";
    public string RegistroDir { get; init; } = "./registros";
    public float Temperatura { get; init; }
    public int? Semilla { get; init; } = 42;

    /// <summary>Lee el entorno. <paramref name="entorno" /> permite probar sin tocar las variables del proceso.</summary>
    public static ConfiguracionDelModelo DesdeEntorno(Func<string, string?>? entorno = null)
    {
        entorno ??= Environment.GetEnvironmentVariable;
        string Leer(string nombre, string porDefecto) =>
            entorno(nombre) is { Length: > 0 } valor ? valor : porDefecto;

        var numCtxTexto = entorno("OLLAMA_NUM_CTX");
        var hayNumCtx = int.TryParse(numCtxTexto, NumberStyles.None, CultureInfo.InvariantCulture, out var numCtx) && numCtx > 0;
        var origen = hayNumCtx
            ? Leer("OLLAMA_NUM_CTX_ORIGEN", OrigenDelNumCtx.Arnes)
            : OrigenDelNumCtx.Default;

        var propio = Leer("OLLAMA_PROPIO_ENDPOINT", EndpointPorDefecto);
        return new ConfiguracionDelModelo
        {
            Endpoint = Leer("OLLAMA_ENDPOINT", EndpointPorDefecto),
            Modelo = Leer("OLLAMA_MODEL", "qwen3.5:4b"),
            Servidor = Leer("OLLAMA_SERVIDOR", "propio") switch
            {
                "compartido" => TipoDeServidor.Compartido,
                "nube" => TipoDeServidor.Nube,
                "propio" => TipoDeServidor.Propio,
                var otro => throw new ConfiguracionInvalidaException($"OLLAMA_SERVIDOR inválido: «{otro}» (compartido | propio | nube).")
            },
            NumCtx = hayNumCtx ? numCtx : NumCtxPorDefecto,
            NumCtxOrigen = origen,
            PropioEndpoint = propio,
            PropioModelo = Leer("OLLAMA_PROPIO_MODEL", "qwen3.5:4b"),
            // Nunca el de OLLAMA_ENDPOINT: el servidor compartido responde 501 a embeddings (Sondeo §5).
            EmbedEndpoint = Leer("OLLAMA_EMBED_ENDPOINT", propio),
            EmbedModelo = Leer("OLLAMA_EMBED_MODEL", "qwen3-embedding:0.6b"),
            EmbPlan = Leer("EMB_PLAN", "EMB-B"),
            RegistroDir = Leer("RLLM_DIR", "./registros")
        };
    }

    /// <summary>Texto del servidor para el registro (rótulo, nunca dirección).</summary>
    public string ServidorTexto => Servidor switch
    {
        TipoDeServidor.Compartido => "compartido",
        TipoDeServidor.Nube => "nube",
        _ => "propio"
    };

    /// <summary>
    /// Reglas de §E.1 que se pueden comprobar antes de llamar. Con servidor compartido, el
    /// <c>num_ctx</c> tiene que venir del arnés (leído de <c>/api/ps</c> dentro del turno): con el
    /// default, la llamada podría recargar el modelo de otros clientes (R-04).
    /// </summary>
    public ConfiguracionDelModelo Validar()
    {
        if (Servidor == TipoDeServidor.Compartido && NumCtxOrigen == OrigenDelNumCtx.Default)
        {
            throw new ConfiguracionInvalidaException(
                "Servidor compartido sin num_ctx del arnés: se rechaza la corrida (leer /api/ps dentro del turno y exportar OLLAMA_NUM_CTX).");
        }
        if (EmbPlan is not ("EMB-A" or "EMB-B" or "EMB-C"))
        {
            throw new ConfiguracionInvalidaException($"EMB_PLAN inválido: «{EmbPlan}» (EMB-A | EMB-B | EMB-C).");
        }
        return this;
    }
}
