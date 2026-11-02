// Nuevo: búsqueda semántica sin almacén vectorial (plan §E.2; guía §7.1). 06 enseña lo que hace un
// VectorStore por dentro: comparar el vector de la pregunta con el de cada fragmento y ordenar.
// El almacén (InMemoryVectorStore) entra recién en 07.
namespace MovilidadUrbana.Asistente.Application;

/// <summary>Un fragmento en el orden de recuperación, con su puntaje.</summary>
public sealed record Posicion(string Id, double Puntaje);

public static class Busqueda
{
    public static double Coseno(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length) throw new ArgumentException($"Dimensiones distintas: {a.Length} y {b.Length}.");
        double punto = 0, na = 0, nb = 0;
        for (var i = 0; i < a.Length; i++)
        {
            punto += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        return na == 0 || nb == 0 ? 0 : punto / (Math.Sqrt(na) * Math.Sqrt(nb));
    }

    /// <summary>
    /// Ordena todos los fragmentos por puntaje, de mayor a menor. Empates: gana el que aparece antes
    /// en el corpus (orden declarado; pesa sobre todo en la línea base léxica, que empata seguido).
    /// </summary>
    public static IReadOnlyList<Posicion> Ordenar(IReadOnlyList<string> idsEnOrdenDelCorpus, Func<string, double> puntaje) =>
        idsEnOrdenDelCorpus
            .Select((id, i) => (Id: id, Orden: i, Puntaje: puntaje(id)))
            .OrderByDescending(x => x.Puntaje)
            .ThenBy(x => x.Orden)
            .Select(x => new Posicion(x.Id, Math.Round(x.Puntaje, 6)))
            .ToList();

    public static IReadOnlyList<Posicion> PorCoseno(
        IReadOnlyList<Fragmento> corpus, IReadOnlyDictionary<string, float[]> vectores, float[] consulta) =>
        Ordenar(corpus.Select(f => f.Id).ToList(), id => Coseno(consulta, vectores[id]));
}
