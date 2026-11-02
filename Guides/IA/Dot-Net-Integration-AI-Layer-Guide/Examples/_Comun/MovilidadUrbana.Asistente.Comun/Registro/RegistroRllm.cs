// Nuevo (plan §E.1, «Esquema» del registro R-LLM; H2-004, H2-022, H2-024, H3-021). Una línea JSON
// por llamada al modelo (tipo "llamada") y una por entrada (tipo "entrada"). No se capturan
// endpoint, host, puerto ni encabezados (H-053): ningún campo del esquema los admite.
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente.Comun.Registro;

/// <summary>Una herramienta invocada en la llamada.</summary>
public sealed record HerramientaInvocada(
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("argumentos")] IDictionary<string, object?>? Argumentos,
    [property: JsonPropertyName("resultado")] object? Resultado,
    [property: JsonPropertyName("error")] string? Error);

/// <summary>Un fragmento recuperado, con su puntaje.</summary>
public sealed record Recuperado(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("puntaje")] double Puntaje);

/// <summary>Línea <c>tipo: "llamada"</c>. Los campos son los de §E.1, con los mismos nombres.</summary>
public sealed record RegistroDeLlamada
{
    [JsonPropertyName("tipo")] public string Tipo => "llamada";
    [JsonPropertyName("fecha")] public DateTimeOffset Fecha { get; init; } = DateTimeOffset.UtcNow;
    [JsonPropertyName("ejemplo")] public required string Ejemplo { get; init; }
    [JsonPropertyName("paso")] public string? Paso { get; init; }
    [JsonPropertyName("entrada_id")] public required string EntradaId { get; init; }
    [JsonPropertyName("llamada_n")] public int LlamadaN { get; init; }
    [JsonPropertyName("entrada")] public string? Entrada { get; init; }
    [JsonPropertyName("salida")] public string? Salida { get; init; }
    [JsonPropertyName("modelo")] public string? Modelo { get; init; }
    [JsonPropertyName("digest")] public string? Digest { get; init; }
    [JsonPropertyName("adaptador")] public string? Adaptador { get; init; }
    [JsonPropertyName("servidor")] public string? Servidor { get; init; }
    [JsonPropertyName("ollama_version")] public string? OllamaVersion { get; init; }
    [JsonPropertyName("paquetes_hash")] public string? PaquetesHash { get; init; }
    [JsonPropertyName("hash_instrucciones")] public string? HashInstrucciones { get; init; }
    [JsonPropertyName("hash_herramientas")] public string? HashHerramientas { get; init; }
    [JsonPropertyName("entradas_version")] public string? EntradasVersion { get; init; }
    [JsonPropertyName("entradas_hash")] public string? EntradasHash { get; init; }
    [JsonPropertyName("temperature")] public float? Temperature { get; init; }
    [JsonPropertyName("seed")] public long? Seed { get; init; }
    [JsonPropertyName("num_ctx")] public int? NumCtx { get; init; }
    [JsonPropertyName("num_ctx_origen")] public string? NumCtxOrigen { get; init; }
    [JsonPropertyName("razonamiento")] public string? Razonamiento { get; init; }
    [JsonPropertyName("herramientas")] public IReadOnlyList<HerramientaInvocada> Herramientas { get; init; } = [];
    [JsonPropertyName("recuperados")] public IReadOnlyList<Recuperado> Recuperados { get; init; } = [];
    [JsonPropertyName("contexto")] public string? Contexto { get; init; }
    [JsonPropertyName("prompt_eval_count")] public long? PromptEvalCount { get; init; }
    [JsonPropertyName("eval_count")] public long? EvalCount { get; init; }
    [JsonPropertyName("load_duration_ms")] public double? LoadDurationMs { get; init; }
    [JsonPropertyName("prompt_eval_duration_ms")] public double? PromptEvalDurationMs { get; init; }
    [JsonPropertyName("eval_duration_ms")] public double? EvalDurationMs { get; init; }
    [JsonPropertyName("total_duration_ms")] public double? TotalDurationMs { get; init; }
    [JsonPropertyName("duracion_reloj_ms")] public double? DuracionRelojMs { get; init; }
    [JsonPropertyName("turno_id")] public string? TurnoId { get; init; }
    /// <summary><c>frio</c> | <c>cacheado</c> | <c>desconocido</c>.</summary>
    [JsonPropertyName("estado_cache")] public string EstadoCache { get; init; } = "desconocido";
    /// <summary>
    /// REC-22: tokens del prompt que el servidor informó como reutilizados (<c>prompt_eval_cached_count</c>
    /// de /api/chat, leído por <c>LectorDePrefijoCacheado</c>); null si no lo informó (vía /v1, o sin él).
    /// </summary>
    [JsonPropertyName("prompt_eval_cached_count")] public long? PromptEvalCachedCount { get; init; }
    /// <summary>M-F: la estimación de la guarda de truncado, con la calibración del modelo de la llamada.</summary>
    [JsonPropertyName("tokens_estimados")] public int? TokensEstimados { get; init; }
    /// <summary>true, false o null («no verificable», vía /v1).</summary>
    [JsonPropertyName("truncado_posible")] public bool? TruncadoPosible { get; init; }
    /// <summary><c>propio-remoto</c> | <c>propio-local</c> | <c>compartido</c> | <c>guion</c>: rótulo, nunca dirección.</summary>
    [JsonPropertyName("maquina")] public string? Maquina { get; init; }
    [JsonPropertyName("carga_1min")] public CargaDeMaquina? Carga1Min { get; init; }
}

/// <summary>Carga de la máquina del Ollama propio al empezar y al terminar; null si el gestor no la expone.</summary>
public sealed record CargaDeMaquina(
    [property: JsonPropertyName("inicio")] double? Inicio,
    [property: JsonPropertyName("fin")] double? Fin);

/// <summary>Línea <c>tipo: "entrada"</c>: el veredicto del oráculo sobre una entrada.</summary>
public sealed record RegistroDeEntrada
{
    [JsonPropertyName("tipo")] public string Tipo => "entrada";
    [JsonPropertyName("fecha")] public DateTimeOffset Fecha { get; init; } = DateTimeOffset.UtcNow;
    [JsonPropertyName("ejemplo")] public required string Ejemplo { get; init; }
    [JsonPropertyName("entrada_id")] public required string EntradaId { get; init; }
    [JsonPropertyName("oraculo")] public required string Oraculo { get; init; }
    /// <summary><c>pasa</c> | <c>falla</c> | <c>excluida</c>.</summary>
    [JsonPropertyName("resultado")] public required string Resultado { get; init; }
    [JsonPropertyName("motivo")] public string? Motivo { get; init; }
    [JsonPropertyName("entradas_version")] public string? EntradasVersion { get; init; }
    [JsonPropertyName("entradas_hash")] public string? EntradasHash { get; init; }
}

public static class ResultadoDeEntrada
{
    public const string Pasa = "pasa";
    public const string Falla = "falla";
    public const string Excluida = "excluida";
}
