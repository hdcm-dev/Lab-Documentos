// Nuevo: métricas de recuperación (plan §E.2; guía §7.5; H-028). Con un fragmento relevante por
// pregunta, recall@k es hit rate@k; para la pregunta del nombre (dos relevantes) se informa además
// recall@3. Sólo cuentan las preguntas respondibles.
namespace MovilidadUrbana.Asistente.Application;

/// <summary>Lo que se mide de una pregunta en un ordenamiento.</summary>
public sealed record MedidaDePregunta(string PreguntaId, int Rango, bool Hit1, bool Hit3, double Rr, double Recall3);

public sealed record Resumen(string Nombre, int M, int Hits1, int Hits3, double Mrr)
{
    public double HitRate1 => M == 0 ? 0 : (double)Hits1 / M;
    public double HitRate3 => M == 0 ? 0 : (double)Hits3 / M;
}

public static class Metricas
{
    /// <summary>Rango (desde 1) del primer relevante en el ordenamiento; 0 si no aparece.</summary>
    public static int Rango(IReadOnlyList<Posicion> orden, IReadOnlyCollection<string> relevantes)
    {
        for (var i = 0; i < orden.Count; i++)
        {
            if (relevantes.Contains(orden[i].Id)) return i + 1;
        }
        return 0;
    }

    public static MedidaDePregunta Medir(string preguntaId, IReadOnlyList<Posicion> orden, IReadOnlyCollection<string> relevantes)
    {
        var rango = Rango(orden, relevantes);
        var recall3 = relevantes.Count == 0 ? 0 : (double)orden.Take(3).Count(p => relevantes.Contains(p.Id)) / relevantes.Count;
        return new(preguntaId, rango, rango == 1, rango is >= 1 and <= 3, rango == 0 ? 0 : 1.0 / rango, recall3);
    }

    public static Resumen Resumir(string nombre, IReadOnlyList<MedidaDePregunta> medidas) =>
        new(nombre, medidas.Count, medidas.Count(m => m.Hit1), medidas.Count(m => m.Hit3),
            medidas.Count == 0 ? 0 : medidas.Average(m => m.Rr));
}
