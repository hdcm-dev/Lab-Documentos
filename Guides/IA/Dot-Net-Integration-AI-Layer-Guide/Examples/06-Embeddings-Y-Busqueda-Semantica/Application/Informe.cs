// Nuevo: las cifras de 06 y cómo se leen (plan §E.2; guía §7.5 y §10.4). Toda cifra es de un
// estímulo del mismo autor (el oro), y con M = 12 es descriptiva: una diferencia se afirma sólo
// con la regla pareada.
using System.Globalization;
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Application;

public sealed record Medicion(
    IReadOnlyList<Resumen> Resumenes,
    IReadOnlyList<ComparacionPareada> Comparaciones,
    IReadOnlyDictionary<string, double> Recall3DelNombre,
    double AzarHit1, double AzarHit3, double AzarMrr,
    IReadOnlyDictionary<string, (double MaxNoRespondible, double MinTop1Respondible)> Puntajes);

public static class Informe
{
    public const string PreguntaDelNombre = "r05";

    public static Medicion Medir(IReadOnlyList<Fragmento> corpus, IReadOnlyList<Consulta> consultas)
    {
        // Pareado: sólo las respondibles medidas por las dos variantes (sin plazo ni truncado).
        bool Valida(Consulta c) => c.Orden is not null && c.Pregunta.Respondible && c.Resultado != ResultadoDeEntrada.Excluida;
        var porVariante = consultas.Where(Valida).GroupBy(c => c.Variante).ToDictionary(g => g.Key, g => g.ToDictionary(c => c.Pregunta.Id));
        var comunes = porVariante.Count == 2
            ? porVariante.Values.Select(d => d.Keys.AsEnumerable()).Aggregate((a, b) => a.Intersect(b)).OrderBy(x => x, StringComparer.Ordinal).ToList()
            : porVariante.Values.SelectMany(d => d.Keys).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToList();
        var preguntas = consultas.Select(c => c.Pregunta).DistinctBy(p => p.Id).ToDictionary(p => p.Id);
        var indice = consultas.Select(c => c.Pregunta.Id).Distinct().Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);

        var medidas = new Dictionary<string, List<MedidaDePregunta>>();
        void Agregar(string nombre, string id, IReadOnlyList<Posicion> orden)
        {
            if (!medidas.TryGetValue(nombre, out var lista)) medidas[nombre] = lista = [];
            lista.Add(Metricas.Medir(id, orden, preguntas[id].Relevantes));
        }
        foreach (var id in comunes)
        {
            foreach (var (v, d) in porVariante) Agregar($"modelo {CorridaDeBusqueda.Nombre(v)}", id, d[id].Orden!);
            Agregar("léxica", id, ReferenciaLexica.Ordenar(corpus, preguntas[id].Texto));
            Agregar("azar (permutación con semilla)", id, ReferenciaAlAzar.Ordenar(corpus, indice[id]));
        }

        var resumenes = medidas.Select(kv => Metricas.Resumir(kv.Key, kv.Value)).ToList();
        var comparaciones = new List<ComparacionPareada>();
        var modelos = medidas.Keys.Where(k => k.StartsWith("modelo", StringComparison.Ordinal)).ToList();
        foreach (var medida in new[] { "hit@1", "hit@3" })
        {
            List<bool> Aciertos(string n) => medidas[n].Select(m => medida == "hit@1" ? m.Hit1 : m.Hit3).ToList();
            foreach (var m in modelos)
            {
                comparaciones.Add(ComparacionPareada.Comparar(m, "léxica", medida, Aciertos(m), Aciertos("léxica")));
                comparaciones.Add(ComparacionPareada.Comparar(m, "azar (permutación con semilla)", medida, Aciertos(m), Aciertos("azar (permutación con semilla)")));
            }
            if (modelos.Count == 2) comparaciones.Add(ComparacionPareada.Comparar(modelos[1], modelos[0], medida, Aciertos(modelos[1]), Aciertos(modelos[0])));
        }

        var recall = medidas.Where(kv => kv.Value.Any(m => m.PreguntaId == PreguntaDelNombre))
            .ToDictionary(kv => kv.Key, kv => kv.Value.Single(m => m.PreguntaId == PreguntaDelNombre).Recall3);

        // Esperanza analítica del azar sobre las mismas preguntas (cada una con su número de relevantes).
        var n = corpus.Count;
        var r = comunes.Select(id => preguntas[id].Relevantes.Count).ToList();
        double Prom(Func<int, double> f) => r.Count == 0 ? 0 : r.Average(f);

        // Insumo para el «no sé» de 07: ¿el mejor puntaje de una no respondible queda por debajo del
        // peor top-1 de una respondible? Es una observación, no un umbral.
        var puntajes = consultas.Where(c => c.Orden is not null).GroupBy(c => CorridaDeBusqueda.Nombre(c.Variante))
            .ToDictionary(g => g.Key, g => (
                g.Where(c => !c.Pregunta.Respondible).Select(c => c.Orden![0].Puntaje).DefaultIfEmpty(double.NaN).Max(),
                g.Where(c => c.Pregunta.Respondible).Select(c => c.Orden![0].Puntaje).DefaultIfEmpty(double.NaN).Min()));

        return new(resumenes, comparaciones, recall,
            Prom(x => ReferenciaAlAzar.HitEsperado(n, x, 1)), Prom(x => ReferenciaAlAzar.HitEsperado(n, x, 3)), Prom(x => ReferenciaAlAzar.RrEsperado(n, x)),
            puntajes);
    }

    public static void Escribir(TextWriter o, IReadOnlyList<Fragmento> corpus, IReadOnlyList<Consulta> consultas, bool sinModelo, ContextoDeCorrida ctx)
    {
        var ci = CultureInfo.InvariantCulture;
        o.WriteLine($"Corpus: {corpus.Count} fragmentos ({CorpusDePolicies.Plantilla}), p. ej. «{corpus[0].Texto}»");
        o.WriteLine(sinModelo
            ? $"Generador estable FNV-1a: {CorridaDeBusqueda.RotuloSinModelo}. Las cifras prueban cableado y métricas, NO calidad."
            : $"Observado en el laboratorio: {ctx.Modelo} ({ctx.Digest}), Ollama {ctx.OllamaVersion}, Ollama propio, {DateTimeOffset.UtcNow:yyyy-MM-dd}. Estímulo del mismo autor.");
        o.WriteLine();
        o.WriteLine("pregunta  variante          rango  top-1                                    puntaje  oráculo");
        foreach (var c in consultas)
        {
            var rango = c.Orden is null || !c.Pregunta.Respondible ? "-" : Metricas.Rango(c.Orden, c.Pregunta.Relevantes).ToString(ci);
            var top = c.Orden?[0];
            o.WriteLine($"{c.Pregunta.Id,-9} {CorridaDeBusqueda.Nombre(c.Variante),-17} {rango,5}  {top?.Id ?? "-",-40} {top?.Puntaje.ToString("0.000", ci) ?? "-",7}  {c.Resultado}");
        }

        var m = Medir(corpus, consultas);
        o.WriteLine();
        o.WriteLine($"Métricas sobre las respondibles medidas por las dos variantes (M = {m.Resumenes.FirstOrDefault()?.M ?? 0}; descriptivas, guía §10.4):");
        foreach (var r in m.Resumenes)
            o.WriteLine($"  {r.Nombre,-32} hit@1 {r.Hits1,2}/{r.M} = {r.HitRate1.ToString("0.000", ci)}   hit@3 {r.Hits3,2}/{r.M} = {r.HitRate3.ToString("0.000", ci)}   MRR {r.Mrr.ToString("0.000", ci)}");
        o.WriteLine($"  {"azar (esperanza analítica)",-32} hit@1 {m.AzarHit1.ToString("0.000", ci)}         hit@3 {m.AzarHit3.ToString("0.000", ci)}         MRR {m.AzarMrr.ToString("0.000", ci)}");
        foreach (var (k, v) in m.Recall3DelNombre) o.WriteLine($"  recall@3 de la pregunta del nombre ({PreguntaDelNombre}), {k}: {v.ToString("0.00", ci)}");
        o.WriteLine();
        o.WriteLine("Comparaciones pareadas (b = acierta sólo A, c = acierta sólo B; binomial exacta bilateral):");
        foreach (var c in m.Comparaciones)
            o.WriteLine($"  {c.Medida} {c.A} vs {c.B}: b = {c.SoloA}, c = {c.SoloB}, p = {c.P.ToString("0.000", ci)} → {(c.SeAfirmaDiferencia ? "se afirma una diferencia" : "no se afirma diferencia")}");
        o.WriteLine();
        foreach (var (v, (maxNo, minSi)) in m.Puntajes)
            o.WriteLine($"  {v}: mejor puntaje de una no respondible {maxNo.ToString("0.000", ci)} · peor top-1 de una respondible {minSi.ToString("0.000", ci)} (observación para el «no sé» de 07, no un umbral)");
    }
}
