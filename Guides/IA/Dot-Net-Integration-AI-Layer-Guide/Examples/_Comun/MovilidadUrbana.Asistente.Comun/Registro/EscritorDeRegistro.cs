// Nuevo (plan §E.1). Escribe el registro R-LLM en RLLM_DIR/<ejemplo>.jsonl y arma las líneas de
// llamada a partir de la respuesta, con las métricas que informa Ollama.
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun.AI;

namespace MovilidadUrbana.Asistente.Comun.Registro;

/// <summary>Lo que es fijo durante una corrida de un ejemplo y se repite en cada línea.</summary>
public sealed record ContextoDeCorrida
{
    public required string Ejemplo { get; init; }
    public required ConfiguracionDelModelo Configuracion { get; init; }
    public required Adaptador Adaptador { get; init; }
    public string? Modelo { get; init; }
    public string? Digest { get; init; }
    public string? OllamaVersion { get; init; }
    public string? EntradasVersion { get; init; }
    public string? EntradasHash { get; init; }
    public string? PaquetesHash { get; init; }
    /// <summary>Rótulo de máquina (RLLM_MAQUINA); <c>guion</c> en modo --fake.</summary>
    public string? Maquina { get; init; } = Environment.GetEnvironmentVariable("RLLM_MAQUINA");
    public string? TurnoId { get; init; } = Environment.GetEnvironmentVariable("RLLM_TURNO_ID");
}

public sealed class EscritorDeRegistro : IDisposable
{
    private static readonly JsonSerializerOptions Json = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    private readonly StreamWriter _salida;
    private readonly ContextoDeCorrida _contexto;

    public EscritorDeRegistro(ContextoDeCorrida contexto)
    {
        _contexto = contexto;
        Directory.CreateDirectory(contexto.Configuracion.RegistroDir);
        Ruta = Path.Combine(contexto.Configuracion.RegistroDir, $"{contexto.Ejemplo}.jsonl");
        _salida = new StreamWriter(File.Open(Ruta, FileMode.Append, FileAccess.Write, FileShare.Read), new UTF8Encoding(false)) { AutoFlush = true };
    }

    public string Ruta { get; }

    public int RenglonesEscritos { get; private set; }

    public void Escribir(RegistroDeLlamada linea) => EscribirObjeto(linea);

    public void Escribir(RegistroDeEntrada linea) => EscribirObjeto(linea with
    {
        EntradasVersion = linea.EntradasVersion ?? _contexto.EntradasVersion,
        EntradasHash = linea.EntradasHash ?? _contexto.EntradasHash
    });

    private void EscribirObjeto<T>(T linea)
    {
        _salida.WriteLine(JsonSerializer.Serialize(linea, Json));
        RenglonesEscritos++;
    }

    /// <summary>
    /// Arma la línea de una llamada desde lo que se mandó y lo que volvió. Los tokens y duraciones
    /// salen de <c>Usage</c> y, con OllamaSharp, de la respuesta cruda del servidor.
    /// </summary>
    public RegistroDeLlamada Llamada(
        string entradaId, int llamadaN, IReadOnlyList<ChatMessage> enviados, ChatOptions? opciones, ChatResponse respuesta,
        TimeSpan reloj, string? paso = null, IReadOnlyList<HerramientaInvocada>? herramientas = null,
        IReadOnlyList<Recuperado>? recuperados = null, string? contexto = null, string? estadoCacheForzado = null)
    {
        var metricas = MetricasDeOllama.De(respuesta);
        var numCtx = OpcionesDelProveedor.NumCtxDe(opciones);
        var modelo = _contexto.Modelo ?? respuesta.ModelId;
        // M-F: la guarda estima con la calibración del modelo de esta llamada.
        var estimacion = GuardaDeTruncado.Estimar(enviados, opciones, modelo);
        var promptEval = metricas.PromptEvalCount ?? respuesta.Usage?.InputTokenCount;
        // REC-22: lo que el servidor informó (o el simulador de --fake), no una deducción.
        var cacheados = LecturaDelServidorEnLaRespuesta.CacheadosDe(respuesta);
        return new RegistroDeLlamada
        {
            Ejemplo = _contexto.Ejemplo,
            Paso = paso,
            EntradaId = entradaId,
            LlamadaN = llamadaN,
            Entrada = enviados.LastOrDefault(m => m.Role == ChatRole.User)?.Text,
            Salida = respuesta.Text,
            Modelo = modelo,
            Digest = _contexto.Digest,
            Adaptador = _contexto.Adaptador == Adaptador.OllamaSharp ? "ollamasharp" : "openai-v1",
            Servidor = _contexto.Configuracion.ServidorTexto,
            OllamaVersion = _contexto.OllamaVersion,
            PaquetesHash = _contexto.PaquetesHash,
            HashInstrucciones = Hashes.Sha256(opciones?.Instructions ?? string.Empty),
            HashHerramientas = Hashes.DeHerramientas(opciones?.Tools),
            EntradasVersion = _contexto.EntradasVersion,
            EntradasHash = _contexto.EntradasHash,
            Temperature = opciones?.Temperature,
            Seed = opciones?.Seed,
            NumCtx = numCtx,
            NumCtxOrigen = numCtx is null ? null : _contexto.Configuracion.NumCtxOrigen,
            Razonamiento = opciones?.Reasoning?.Effort?.ToString(),
            Herramientas = herramientas ?? HerramientasDe(respuesta),
            Recuperados = recuperados ?? [],
            Contexto = contexto,
            PromptEvalCount = promptEval,
            EvalCount = metricas.EvalCount ?? respuesta.Usage?.OutputTokenCount,
            LoadDurationMs = metricas.LoadDurationMs,
            PromptEvalDurationMs = metricas.PromptEvalDurationMs,
            EvalDurationMs = metricas.EvalDurationMs,
            TotalDurationMs = metricas.TotalDurationMs,
            DuracionRelojMs = Math.Round(reloj.TotalMilliseconds, 1),
            TurnoId = _contexto.TurnoId,
            // «frio» forzado: antes de la entrada, /api/ps ya no listaba el modelo (otra sesión lo descargó).
            EstadoCache = estadoCacheForzado ?? EstadoDeCache.Observado(cacheados, promptEval)
                ?? EstadoDeCache.Deducir(_contexto.Configuracion.Servidor, metricas.PromptEvalDurationMs, promptEval),
            PromptEvalCachedCount = cacheados,
            TokensEstimados = estimacion,
            TruncadoPosible = GuardaDeTruncado.Evaluar(promptEval, estimacion, numCtx, _contexto.Adaptador),
            Maquina = _contexto.Maquina
        };
    }

    private static List<HerramientaInvocada> HerramientasDe(ChatResponse respuesta)
    {
        var resultados = respuesta.Messages.SelectMany(m => m.Contents).OfType<FunctionResultContent>()
            .ToDictionary(r => r.CallId, r => r);
        return respuesta.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>()
            .Select(c => resultados.TryGetValue(c.CallId, out var r)
                ? new HerramientaInvocada(c.Name, c.Arguments, r.Result, r.Exception?.Message)
                : new HerramientaInvocada(c.Name, c.Arguments, null, null))
            .ToList();
    }

    public void Dispose() => _salida.Dispose();

    /// <summary>Cronómetro de reloj para <c>duracion_reloj_ms</c> (no comparable entre máquinas: H3-021).</summary>
    public static Stopwatch Cronometro() => Stopwatch.StartNew();
}

/// <summary>Duraciones y conteos que informa Ollama en la respuesta final (en ns; se pasan a ms).</summary>
public sealed record MetricasDeOllama(long? PromptEvalCount, long? EvalCount, double? LoadDurationMs, double? PromptEvalDurationMs, double? EvalDurationMs, double? TotalDurationMs)
{
    public static readonly MetricasDeOllama Vacias = new(null, null, null, null, null, null);

    public static MetricasDeOllama De(ChatResponse respuesta)
    {
        if (respuesta.RawRepresentation is OllamaSharp.Models.Chat.ChatDoneResponseStream fin)
        {
            static double Ms(long ns) => Math.Round(ns / 1_000_000.0, 1);
            return new(fin.PromptEvalCount, fin.EvalCount, Ms(fin.LoadDuration), Ms(fin.PromptEvalDuration), Ms(fin.EvalDuration), Ms(fin.TotalDuration));
        }
        return Vacias;
    }
}

public static class EstadoDeCache
{
    /// <summary>
    /// Umbral en ms por token de prompt: por encima, «frio»; por debajo, «cacheado». Lo calibra
    /// DEP-03 en el Ollama propio (RLLM_UMBRAL_MS_POR_TOKEN); sin calibrar, todo es «desconocido».
    /// </summary>
    public static double? UmbralMsPorToken { get; set; } =
        double.TryParse(Environment.GetEnvironmentVariable("RLLM_UMBRAL_MS_POR_TOKEN"),
            System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var u) && u > 0 ? u : null;

    /// <summary>
    /// Fracción del prompt reutilizada por encima de la cual la llamada es «cacheado»: la misma mitad
    /// con la que el oráculo par-prefijo de 05 decide que una llamada «salió cacheada» (§E.2).
    /// </summary>
    public const double FraccionCacheada = 0.5;

    /// <summary>
    /// REC-22: con el valor que informó el servidor no hace falta deducir. «cacheado» si reutilizó más
    /// de <see cref="FraccionCacheada" /> del prompt; «frio» si no (unos pocos tokens del encabezado de
    /// la plantilla se reutilizan casi siempre). El número exacto va en <c>prompt_eval_cached_count</c>.
    /// null si el servidor no lo informó o no hay conteo del prompt: entonces decide <see cref="Deducir" />.
    /// </summary>
    public static string? Observado(long? tokensCacheados, long? promptEvalCount) =>
        tokensCacheados is null || promptEvalCount is null or 0 ? null
            : tokensCacheados > FraccionCacheada * promptEvalCount ? "cacheado" : "frio";

    public static string Deducir(TipoDeServidor servidor, double? promptEvalDurationMs, long? promptEvalCount)
    {
        // En el compartido la carga ajena mueve la duración: siempre «desconocido» (H2-024).
        if (servidor == TipoDeServidor.Compartido || UmbralMsPorToken is null ||
            promptEvalDurationMs is null || promptEvalCount is null or 0)
        {
            return "desconocido";
        }
        return promptEvalDurationMs / promptEvalCount > UmbralMsPorToken ? "frio" : "cacheado";
    }
}

public static class Hashes
{
    public static string Sha256(string texto) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));

    public static string Sha256DeArchivo(string ruta)
    {
        using var flujo = File.OpenRead(ruta);
        return Convert.ToHexStringLower(SHA256.HashData(flujo));
    }

    /// <summary>SHA-256 de los esquemas JSON de las herramientas montadas, en orden de nombre.</summary>
    public static string DeHerramientas(IList<AITool>? herramientas)
    {
        if (herramientas is null || herramientas.Count == 0) return Sha256(string.Empty);
        var texto = string.Join("\n", herramientas.OfType<AIFunctionDeclaration>()
            .OrderBy(f => f.Name, StringComparer.Ordinal)
            .Select(f => $"{f.Name}\t{f.Description}\t{f.JsonSchema.GetRawText()}"));
        return Sha256(texto);
    }

    /// <summary>
    /// SHA-256 de Directory.Packages.props: el que grabó la compilación (Directory.Build.targets)
    /// o, si no está, el del archivo buscado hacia arriba desde <paramref name="desde" />.
    /// </summary>
    public static string? DePaquetes(string? desde = null)
    {
        var grabado = typeof(Hashes).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyMetadataAttribute), false)
            .OfType<System.Reflection.AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "PaquetesHash")?.Value;
        if (!string.IsNullOrEmpty(grabado)) return grabado;

        for (var dir = new DirectoryInfo(desde ?? Directory.GetCurrentDirectory()); dir is not null; dir = dir.Parent)
        {
            var ruta = Path.Combine(dir.FullName, "Directory.Packages.props");
            if (File.Exists(ruta)) return Sha256DeArchivo(ruta);
        }
        return null;
    }
}
