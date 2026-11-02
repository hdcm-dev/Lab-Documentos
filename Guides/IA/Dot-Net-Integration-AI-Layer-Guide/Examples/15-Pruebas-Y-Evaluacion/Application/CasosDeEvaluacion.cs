// Nuevo (plan §E.2, fila 15; guía §10.5–§10.7). Lo que se evalúa: las 16 salidas de 07 de la última
// corrida válida (congeladas desde su registro R-LLM) y los 10 casos de calibración, armados como
// estímulos desde salidas propias de 07 y anotados ANTES de juzgar. Los dos archivos se congelan con
// su SHA-256 en ejemplo.json (plan §E.1, H2-023).
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>Una salida de 07 tal como quedó en su registro: pregunta, contexto, recuperados, respuesta y veredicto.</summary>
public sealed record SalidaDe07
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("pregunta")] public required string Pregunta { get; init; }
    [JsonPropertyName("contexto")] public required string Contexto { get; init; }
    [JsonPropertyName("recuperados")] public required IReadOnlyList<string> Recuperados { get; init; }
    [JsonPropertyName("salida")] public required string Salida { get; init; }
    /// <summary>Lo que dijo el oráculo «rag-con-citas» en 07 (pasa | falla).</summary>
    [JsonPropertyName("resultado_07")] public required string Resultado07 { get; init; }
    [JsonPropertyName("motivo_07")] public string? Motivo07 { get; init; }
}

public sealed record SalidasDe07
{
    public const string Archivo = "entradas/salidas-07.v2.json";

    [JsonPropertyName("rotulo")] public required string Rotulo { get; init; }
    [JsonPropertyName("version")] public required string Version { get; init; }
    [JsonPropertyName("procedencia")] public required ProcedenciaDe07 Procedencia { get; init; }
    [JsonPropertyName("salidas")] public required IReadOnlyList<SalidaDe07> Salidas { get; init; }

    public static string RutaPorDefecto => Path.Combine(AppContext.BaseDirectory, Archivo);

    public static SalidasDe07 Leer(string? ruta = null) =>
        JsonSerializer.Deserialize<SalidasDe07>(File.ReadAllText(ruta ?? RutaPorDefecto))
        ?? throw new InvalidDataException($"{Archivo} vacío");
}

/// <summary>De qué corrida de 07 salen las salidas (rótulos, nunca direcciones).</summary>
public sealed record ProcedenciaDe07
{
    [JsonPropertyName("ejemplo")] public required string Ejemplo { get; init; }
    [JsonPropertyName("modelo")] public string? Modelo { get; init; }
    [JsonPropertyName("digest")] public string? Digest { get; init; }
    [JsonPropertyName("ollama_version")] public string? OllamaVersion { get; init; }
    [JsonPropertyName("servidor")] public string? Servidor { get; init; }
    [JsonPropertyName("turno_id")] public string? TurnoId { get; init; }
    [JsonPropertyName("fecha_primera_llamada")] public DateTimeOffset? Fecha { get; init; }
    [JsonPropertyName("entradas_hash_07")] public string? EntradasHash07 { get; init; }
    [JsonPropertyName("registro")] public string? Registro { get; init; }
}

/// <summary>Tipos de caso de calibración (plan §E.2): correcta tal cual o una de las tres perturbaciones controladas.</summary>
public static class TipoDeCaso
{
    public const string Correcta = "correcta";
    public const string CitaYContradice = "cita-y-contradice";
    public const string SinCita = "sin-cita";
    public const string CitaNoRecuperada = "cita-no-recuperada";
}

/// <summary>La anotación, escrita antes de juzgar: qué debería decir un juez que mide lo que dice medir.</summary>
public sealed record Anotacion
{
    /// <summary>Toda afirmación de la respuesta está en el contexto (lo que mide GroundednessEvaluator).</summary>
    [JsonPropertyName("fundamentada")] public required bool Fundamentada { get; init; }
    /// <summary>La respuesta contesta la pregunta (lo que mide RelevanceEvaluator).</summary>
    [JsonPropertyName("relevante")] public required bool Relevante { get; init; }
    /// <summary>Lo que tienen que decir los evaluadores deterministas (pasa | falla); no es del juez.</summary>
    [JsonPropertyName("determinista")] public required string Determinista { get; init; }
    [JsonPropertyName("nota")] public string? Nota { get; init; }
}

public sealed record CasoDeCalibracion
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    /// <summary>La salida de 07 de la que sale (id del oro: r03, r04…).</summary>
    [JsonPropertyName("base")] public required string Base { get; init; }
    [JsonPropertyName("tipo")] public required string Tipo { get; init; }
    [JsonPropertyName("respuesta")] public required string Respuesta { get; init; }
    [JsonPropertyName("anotacion")] public required Anotacion Anotacion { get; init; }
}

public sealed record CalibracionDelJuez
{
    public const string Archivo = "entradas/calibracion.v2.json";

    [JsonPropertyName("rotulo")] public required string Rotulo { get; init; }
    [JsonPropertyName("version")] public required string Version { get; init; }
    [JsonPropertyName("anotador")] public required string Anotador { get; init; }
    [JsonPropertyName("umbral")] public required int Umbral { get; init; }
    [JsonPropertyName("aciertos_minimos")] public required int AciertosMinimos { get; init; }
    [JsonPropertyName("casos")] public required IReadOnlyList<CasoDeCalibracion> Casos { get; init; }

    public static string RutaPorDefecto => Path.Combine(AppContext.BaseDirectory, Archivo);

    public static CalibracionDelJuez Leer(string? ruta = null) =>
        JsonSerializer.Deserialize<CalibracionDelJuez>(File.ReadAllText(ruta ?? RutaPorDefecto))
        ?? throw new InvalidDataException($"{Archivo} vacío");
}

/// <summary>Grupo de una entrada de 15: las salidas de 07 (se informan) o la calibración (decide si se interpretan).</summary>
public static class GrupoDeEntrada
{
    public const string Salidas07 = "salidas-07";
    public const string Calibracion = "calibracion";
}

/// <summary>
/// Un caso listo para evaluar, venga de donde venga: la pregunta, el contexto de grounding, la
/// respuesta a juzgar y lo que se espera (del oráculo de 07 o de la anotación).
/// </summary>
public sealed record CasoDeEvaluacion(
    string EntradaId,
    string Grupo,
    PreguntaDeOro Pregunta,
    string Contexto,
    IReadOnlyList<Recuperacion> Recuperados,
    string Respuesta,
    string? Tipo,
    Anotacion? Anotacion,
    string? Resultado07,
    string EntradasHash)
{
    /// <summary>Lo que el juez «debería» decir, en el formato que lee el oráculo jq (va en el campo paso).</summary>
    public string Esperado(string evaluador) => Grupo == GrupoDeEntrada.Calibracion
        ? (evaluador == NombresDeMetricas.Relevance ? Anotacion!.Relevante : Anotacion!.Fundamentada) ? "alta" : "baja"
        : Resultado07!;

    /// <summary>Arma los 26 casos: 16 salidas de 07 (en el orden del oro) y 10 de calibración.</summary>
    public static IReadOnlyList<CasoDeEvaluacion> Armar(
        ConjuntoDeOro oro, SalidasDe07 salidas, string shaSalidas, CalibracionDelJuez calibracion, string shaCalibracion)
    {
        var porId = salidas.Salidas.ToDictionary(s => s.Id, StringComparer.Ordinal);
        var casos = new List<CasoDeEvaluacion>();
        foreach (var p in oro.Preguntas)
        {
            var s = porId[p.Id];
            casos.Add(new($"s07-{p.Id}", GrupoDeEntrada.Salidas07, p, s.Contexto, Recuperaciones(s), s.Salida, null, null, s.Resultado07, shaSalidas));
        }
        foreach (var c in calibracion.Casos)
        {
            var s = porId[c.Base];
            var p = oro.Preguntas.Single(x => x.Id == c.Base);
            casos.Add(new(c.Id, GrupoDeEntrada.Calibracion, p, s.Contexto, Recuperaciones(s), c.Respuesta, c.Tipo, c.Anotacion, null, shaCalibracion));
        }
        return casos;
    }

    /// <summary>Los fragmentos recuperados en 07, reconstruidos desde su contexto («[Fuente: id] texto»).</summary>
    public static IReadOnlyList<Recuperacion> Recuperaciones(SalidaDe07 s)
    {
        var textos = s.Contexto.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.TrimEnd('\r'))
            .Where(l => l.StartsWith("[Fuente: ", StringComparison.Ordinal))
            .ToDictionary(l => l[9..l.IndexOf(']')], l => l[(l.IndexOf(']') + 2)..], StringComparer.Ordinal);
        return s.Recuperados.Select(id => new Recuperacion(id, textos[id], 0)).ToList();
    }
}

/// <summary>Nombres de las métricas de M.E.AI.Evaluation.Quality que usa el ejemplo.</summary>
public static class NombresDeMetricas
{
    public const string Relevance = "Relevance";
    public const string Groundedness = "Groundedness";
    /// <summary>Descriptiva, contra la referencia del oro (REC-18); no entra en el oráculo «acuerdo-del-juez».</summary>
    public const string Equivalence = "Equivalence";
}
