namespace FrameworkConcept.Tools;

/// <summary>
/// Lee la línea de comandos de las herramientas: «comando --opcion valor --bandera».
/// Es a propósito mínimo (sin paquetes): una opción puede repetirse (<c>--requires a@1.0.0 --requires b@1.0.0</c>).
/// </summary>
internal sealed class Argumentos
{
    private readonly Dictionary<string, List<string>> _opciones = new(StringComparer.Ordinal);
    private readonly HashSet<string> _banderas = new(StringComparer.Ordinal);

    private Argumentos()
    {
    }

    /// <param name="args">Lo que viene después del nombre del comando.</param>
    /// <param name="banderas">Las opciones que no llevan valor (por ejemplo «forzar»).</param>
    public static Argumentos Leer(IEnumerable<string> args, params string[] banderas)
    {
        var resultado = new Argumentos();
        using var e = args.GetEnumerator();
        while (e.MoveNext())
        {
            var actual = e.Current;
            if (!actual.StartsWith("--", StringComparison.Ordinal))
                throw new ErrorDeUso($"argumento suelto «{actual}»: las opciones van como --nombre valor");

            var nombre = actual[2..];
            if (banderas.Contains(nombre))
            {
                resultado._banderas.Add(nombre);
                continue;
            }
            if (!e.MoveNext()) throw new ErrorDeUso($"falta el valor de --{nombre}");

            if (!resultado._opciones.TryGetValue(nombre, out var valores))
                resultado._opciones[nombre] = valores = [];
            valores.Add(e.Current);
        }
        return resultado;
    }

    public string? Opcion(string nombre) =>
        _opciones.TryGetValue(nombre, out var valores) ? valores[^1] : null;

    public string Opcion(string nombre, string porDefecto) => Opcion(nombre) ?? porDefecto;

    public string Requerida(string nombre) =>
        Opcion(nombre) ?? throw new ErrorDeUso($"falta --{nombre}");

    public IReadOnlyList<string> Todas(string nombre) =>
        _opciones.TryGetValue(nombre, out var valores) ? valores : [];

    public bool Bandera(string nombre) => _banderas.Contains(nombre);
}

/// <summary>La línea de comandos está mal escrita (código de salida 2).</summary>
internal sealed class ErrorDeUso(string mensaje) : Exception(mensaje);

/// <summary>
/// Lo que se quiso publicar viola una regla del dictamen (código de salida 1). El mensaje dice cuál, con su
/// P-número, para que quien publica sepa qué corregir.
/// </summary>
internal sealed class PublicacionRechazada(string mensaje) : Exception(mensaje);
