// Nuevo: la corrida de 06 (plan §E.2, §E.5). Vectoriza los 12 fragmentos y las 16 preguntas por las
// dos variantes (con y sin la instrucción de consulta de qwen3-embedding), ordena por coseno, mide
// contra las dos líneas base y deja una línea R-LLM por llamada y una por entrada. No conoce el
// adaptador: recibe un IEmbeddingGenerator (OllamaSharp, o el generador estable de --fake / EMB-C).
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Application;

public enum Variante { SinInstruccion, ConInstruccion }

/// <summary>Una consulta hecha: la pregunta, la variante, el ordenamiento y lo que dijo el oráculo.</summary>
public sealed record Consulta(PreguntaDeOro Pregunta, Variante Variante, IReadOnlyList<Posicion>? Orden, string Resultado, string? Motivo, int Dimension);

public sealed record ResultadoDeLaCorrida(
    IReadOnlyList<Fragmento> Corpus,
    IReadOnlyList<Consulta> Consultas,
    IReadOnlyList<string> InvariantesQueFallan,
    int Llamadas)
{
    public bool InvariantesCumplidos => InvariantesQueFallan.Count == 0;
}

public sealed class CorridaDeBusqueda(
    IEmbeddingGenerator<string, Embedding<float>> generador,
    ConfiguracionDelModelo config,
    ContextoDeCorrida contexto,
    string rutaDelOro,
    Plazo plazo,
    bool sinModelo)
{
    public const string Oraculo = "recuperacion-top3";
    /// <summary>La dimensión del atributo vectorial de 07 y PD-2: se verifica contra el modelo al arrancar (N3 C2).</summary>
    public const int Dimension = 1024;

    /// <summary>
    /// Instrucción de consulta de qwen3-embedding: «Instruct: {tarea}\nQuery:{pregunta}». Sólo va en la
    /// pregunta, nunca en el fragmento. La tarea se escribe en inglés, como recomienda la ficha del
    /// modelo para corpus multilingües; la pregunta y el corpus siguen en castellano.
    /// </summary>
    public const string Tarea = "Given a question about the data-entry rules of an urban mobility survey system, retrieve the rule that answers it";

    public static string TextoDeConsulta(string pregunta, Variante variante) =>
        variante == Variante.ConInstruccion ? $"Instruct: {Tarea}\nQuery:{pregunta}" : pregunta;

    public static string Nombre(Variante v) => v == Variante.ConInstruccion ? "con-instruccion" : "sin-instruccion";

    /// <summary>Rótulo de EMB-C (plan §F.6, H2-017): va en la salida del registro y en la consola.</summary>
    public const string RotuloSinModelo = "no ejecutado contra un modelo de embeddings";

    private int _llamadas;

    public async Task<ResultadoDeLaCorrida> CorrerAsync(EscritorDeRegistro registro, TextWriter consola, CancellationToken ct = default)
    {
        var fallas = new List<string>();
        var corpus = CorpusDePolicies.Fragmentos();
        var oro = ConjuntoDeOro.Leer(rutaDelOro);
        fallas.AddRange(Validaciones.DelCorpus(corpus));
        fallas.AddRange(Validaciones.DelOro(oro, corpus));
        if (fallas.Count > 0) return new(corpus, [], fallas, 0);

        // 1. Corpus: un vector por fragmento. La primera llamada verifica la dimensión.
        var vectores = new Dictionary<string, float[]>(StringComparer.Ordinal);
        foreach (var f in corpus)
        {
            var (vector, _) = await VectorizarAsync(registro, "corpus", "fragmento", f.Texto, instruccion: string.Empty, recuperados: [], ct);
            if (vector.Length != Dimension)
            {
                fallas.Add($"dimensión del modelo {vector.Length} ≠ {Dimension} del atributo (fragmento {f.Id})");
                return new(corpus, [], fallas, _llamadas);
            }
            vectores[f.Id] = vector;
        }

        // 2. Preguntas por las dos variantes (diseño pareado: las mismas 16 por las dos).
        var consultas = new List<Consulta>();
        var vectoresDeConsulta = new List<object>();
        foreach (var variante in new[] { Variante.SinInstruccion, Variante.ConInstruccion })
        {
            foreach (var p in oro.Preguntas)
            {
                var entradaId = $"{p.Id}.{Nombre(variante)}";
                if (plazo.Vencido)
                {
                    consultas.Add(new(p, variante, null, ResultadoDeEntrada.Excluida, "plazo", 0));
                    EscribirEntrada(registro, entradaId, ResultadoDeEntrada.Excluida, "plazo");
                    continue;
                }
                var texto = TextoDeConsulta(p.Texto, variante);
                var instruccion = variante == Variante.ConInstruccion ? Tarea : string.Empty;
                // El orden se calcula antes de registrar, para que la línea lleve «recuperados».
                var (vector, linea) = await VectorizarAsync(registro, entradaId, $"consulta:{Nombre(variante)}", texto, instruccion,
                    recuperados: null, ct, vectorAOrden: v => Busqueda.PorCoseno(corpus, vectores, v));
                var orden = Busqueda.PorCoseno(corpus, vectores, vector);
                var (resultado, motivo) = Veredicto(p, orden, linea.TruncadoPosible);
                consultas.Add(new(p, variante, orden, resultado, motivo, vector.Length));
                EscribirEntrada(registro, entradaId, resultado, motivo);
                vectoresDeConsulta.Add(new { id = p.Id, variante = Nombre(variante), texto_enviado = texto, vector });
                if (vector.Length != Dimension) fallas.Add($"dimensión {vector.Length} en {entradaId}");
                if (orden.Count != corpus.Count) fallas.Add($"{entradaId}: el orden no tiene los {corpus.Count} fragmentos");
                if (!orden.Zip(orden.Skip(1)).All(x => x.First.Puntaje >= x.Second.Puntaje)) fallas.Add($"{entradaId}: orden no descendente");
            }
        }

        var hechas = consultas.Count(c => c.Orden is not null) + corpus.Count;
        if (_llamadas != hechas) fallas.Add($"llamadas registradas {_llamadas} ≠ {hechas} esperadas");

        if (!sinModelo) Persistir(corpus, vectores, vectoresDeConsulta);
        Informe.Escribir(consola, corpus, consultas, sinModelo, contexto);
        return new(corpus, consultas, fallas, _llamadas);
    }

    /// <summary>
    /// Mismo criterio que tools/oraculos/recuperacion-top3.jq: una respondible pasa si algún
    /// relevante está entre los 3 primeros; una no respondible queda fuera del hit rate.
    /// El motivo lleva los relevantes para que el oráculo se recalcule leyendo sólo el registro.
    /// </summary>
    public static (string Resultado, string? Motivo) Veredicto(PreguntaDeOro p, IReadOnlyList<Posicion> orden, bool? truncado)
    {
        if (truncado == true) return (ResultadoDeEntrada.Excluida, "truncado_posible");
        if (!p.Respondible) return (ResultadoDeEntrada.Excluida, $"no-respondible ({p.Cercania}): fuera del hit rate");
        var motivo = "relevantes=" + string.Join(',', p.Relevantes);
        return orden.Take(3).Any(x => p.Relevantes.Contains(x.Id))
            ? (ResultadoDeEntrada.Pasa, motivo)
            : (ResultadoDeEntrada.Falla, motivo);
    }

    private void EscribirEntrada(EscritorDeRegistro registro, string entradaId, string resultado, string? motivo) =>
        registro.Escribir(new RegistroDeEntrada
        {
            Ejemplo = contexto.Ejemplo, EntradaId = entradaId, Oraculo = Oraculo, Resultado = resultado, Motivo = motivo
        });

    private async Task<(float[] Vector, RegistroDeLlamada Renglon)> VectorizarAsync(
        EscritorDeRegistro registro, string entradaId, string paso, string texto, string instruccion,
        IReadOnlyList<Recuperado>? recuperados, CancellationToken ct, Func<float[], IReadOnlyList<Posicion>>? vectorAOrden = null)
    {
        var reloj = Stopwatch.StartNew();
        var generados = await generador.GenerateAsync([texto], cancellationToken: ct);
        reloj.Stop();
        var vector = generados[0].Vector.ToArray();
        _llamadas++;

        var metricas = MetricasDeEmbedding.De(generados);
        var estimacion = (int)Math.Ceiling(texto.Length / GuardaDeTruncado.CaracteresPorToken);
        var orden = vectorAOrden?.Invoke(vector);
        var linea = new RegistroDeLlamada
        {
            Ejemplo = contexto.Ejemplo,
            Paso = paso,
            EntradaId = entradaId,
            LlamadaN = _llamadas,
            Entrada = texto,
            Salida = sinModelo
                ? $"vector de {vector.Length} dimensiones (generador estable: {RotuloSinModelo})"
                : $"vector de {vector.Length} dimensiones",
            Modelo = contexto.Modelo,
            Digest = contexto.Digest,
            Adaptador = sinModelo ? "generador-estable" : "ollamasharp",
            Servidor = config.ServidorTexto,
            OllamaVersion = contexto.OllamaVersion,
            PaquetesHash = contexto.PaquetesHash,
            HashInstrucciones = Hashes.Sha256(instruccion),
            HashHerramientas = Hashes.Sha256(string.Empty),
            EntradasVersion = contexto.EntradasVersion,
            EntradasHash = contexto.EntradasHash,
            Recuperados = recuperados ?? orden?.Select(o => new Recuperado(o.Id, o.Puntaje)).ToList() ?? [],
            PromptEvalCount = metricas.PromptEvalCount,
            LoadDurationMs = metricas.LoadDurationMs,
            TotalDurationMs = metricas.TotalDurationMs,
            DuracionRelojMs = Math.Round(reloj.Elapsed.TotalMilliseconds, 1),
            TurnoId = contexto.TurnoId,
            EstadoCache = "desconocido",
            // Ollama trunca el texto de un embedding en silencio: si informó muchos menos tokens que
            // los estimados, la entrada no cuenta. Sin conteo informado, «no verificable» (null).
            TruncadoPosible = metricas.PromptEvalCount is long n ? n < GuardaDeTruncado.FraccionMinima * estimacion : null,
            Maquina = contexto.Maquina
        };
        registro.Escribir(linea);
        return (vector, linea);
    }

    private void Persistir(IReadOnlyList<Fragmento> corpus, Dictionary<string, float[]> vectores, List<object> consultas)
    {
        // Fase de embeddings de §E.1: 07 y PD-2 reutilizan estos vectores cuando el texto es idéntico.
        var dir = Path.Combine(config.RegistroDir, "embeddings");
        Directory.CreateDirectory(dir);
        var datos = new
        {
            pieza = contexto.Ejemplo,
            modelo = contexto.Modelo,
            digest = contexto.Digest,
            ollama_version = contexto.OllamaVersion,
            plantilla = CorpusDePolicies.Plantilla,
            entradas_hash = contexto.EntradasHash,
            fecha = DateTimeOffset.UtcNow,
            fragmentos = corpus.Select(f => new { id = f.Id, texto = f.Texto, vector = vectores[f.Id] }),
            preguntas = consultas
        };
        File.WriteAllText(Path.Combine(dir, $"{contexto.Ejemplo}.v1.json"), JsonSerializer.Serialize(datos));
    }
}

/// <summary>Conteo y duraciones que Ollama informa para un embedding, si el adaptador los expone.</summary>
public sealed record MetricasDeEmbedding(long? PromptEvalCount, double? LoadDurationMs, double? TotalDurationMs)
{
    public static MetricasDeEmbedding De(GeneratedEmbeddings<Embedding<float>> generados)
    {
        static double? Ms(object? ns) => ns switch
        {
            long l => Math.Round(l / 1_000_000.0, 1),
            int i => Math.Round(i / 1_000_000.0, 1),
            double d => Math.Round(d / 1_000_000.0, 1),
            _ => null
        };
        var extra = generados.AdditionalProperties;
        return new(
            generados.Usage?.InputTokenCount,
            Ms(extra?.GetValueOrDefault("load_duration")),
            Ms(extra?.GetValueOrDefault("total_duration")));
    }
}
