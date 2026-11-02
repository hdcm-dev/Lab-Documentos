// Nuevo: la cuenta determinista que verifica un resumen (escalón −1: una función, ningún modelo).
// La usan el VERIFICADOR del workflow y el oráculo del ejemplo. tools/oraculos/porcentajes-del-servicio.jq
// repite exactamente estas reglas leyendo sólo el registro, sin compartir código con este archivo.
//
// Reglas (las mismas en C# y en jq):
//   1. Texto normalizado: minúsculas y vocales sin tilde (á→a … ü→u). Mide lo mismo que el original.
//   2. Un medio se «nombra» si aparece su clave o su etiqueta de Catalogos al comienzo de una palabra;
//      vale la PRIMERA mención (la de menor posición y, a igual posición, la más larga).
//   3. Un porcentaje es un número (con decimales por punto o coma) seguido de «%» o «por ciento».
//   4. El porcentaje de un medio se busca en la CLÁUSULA de su mención: primero la cláusula corta
//      (cortes en «.», «;», salto de línea, «y», «e» y «,» que no está entre dígitos); si ahí no hay
//      porcentaje, la larga (los mismos cortes sin la coma). Dentro de la cláusula vale el más cercano a la
//      mención (distancia en caracteres entre los dos tramos) y, a igual distancia, el que viene después.
//      Así «El 20 % de las respuestas usa colectivo y el 35 % usa bicicleta» asocia 20 con colectivo,
//      aunque el 35 esté más cerca. Límite declarado: «colectivo y auto: 20 % cada uno» no se reconoce.
//   5. Coincide si difiere en 1 punto o menos del que da ResultadosDeEncuestaService.
//   6. Un resumen sin ningún porcentaje es falla «sin cifra».
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Application.Resumenes;

/// <summary>Lo que el comparador encontró para un medio pedido.</summary>
/// <param name="Afirmado">El porcentaje asociado a la mención, o null si no lo hay.</param>
public sealed record ComparacionDeMedio(string Clave, double Esperado, bool Nombrado, double? Afirmado)
{
    public bool Coincide => Nombrado && Afirmado is double a && Math.Abs(a - Esperado) <= ComparadorDePorcentajes.Tolerancia;

    /// <summary>El error tal como se le devuelve al modelo en la vuelta.</summary>
    public string? Error => Coincide ? null
        : !Nombrado ? $"{Clave}: no aparece en el resumen (el servicio da {ComparadorDePorcentajes.Formato(Esperado)} %)"
        : Afirmado is null ? $"{Clave}: no tiene porcentaje (el servicio da {ComparadorDePorcentajes.Formato(Esperado)} %)"
        : $"{Clave}: dice {ComparadorDePorcentajes.Formato(Afirmado.Value)} %, el servicio da {ComparadorDePorcentajes.Formato(Esperado)} %";
}

/// <summary>Veredicto sobre un resumen entero.</summary>
public sealed record VerificacionDeResumen(bool SinCifra, IReadOnlyList<ComparacionDeMedio> Medios)
{
    public bool Correcto => !SinCifra && Medios.All(m => m.Coincide);

    public IReadOnlyList<string> Errores => SinCifra
        ? ["sin cifra: el resumen no tiene ningún porcentaje"]
        : [.. Medios.Select(m => m.Error).OfType<string>()];
}

public static partial class ComparadorDePorcentajes
{
    public const double Tolerancia = 1.0;

    public static VerificacionDeResumen Verificar(string? resumen, IReadOnlyList<string> mediosPedidos, ResultadosDeEncuesta resultados) =>
        Verificar(resumen, mediosPedidos.Select(c => (c, resultados.Medios.Single(m => m.Clave == c).Porcentaje)).ToList());

    public static VerificacionDeResumen Verificar(string? resumen, IReadOnlyList<(string Clave, double Esperado)> esperados)
    {
        var texto = Normalizar(resumen ?? string.Empty);
        var porcentajes = Porcentaje().Matches(texto)
            .Select(m => (Inicio: m.Index, Fin: m.Index + m.Length,
                          Valor: double.Parse(m.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture)))
            .ToList();
        if (porcentajes.Count == 0)
        {
            return new VerificacionDeResumen(true, [.. esperados.Select(e => new ComparacionDeMedio(e.Clave, e.Esperado, false, null))]);
        }

        var medios = new List<ComparacionDeMedio>();
        foreach (var (clave, esperado) in esperados)
        {
            var mencion = PrimeraMencion(texto, clave);
            if (mencion is not { } m)
            {
                medios.Add(new ComparacionDeMedio(clave, esperado, false, null));
                continue;
            }
            var candidatos = EnLaClausula(porcentajes, Clausula(texto, CorteCorto(), m.Inicio));
            if (candidatos.Count == 0) candidatos = EnLaClausula(porcentajes, Clausula(texto, CorteLargo(), m.Inicio));
            double? afirmado = candidatos.Count == 0 ? null : candidatos
                .Select(p => (p.Valor,
                              Distancia: p.Inicio >= m.Fin ? p.Inicio - m.Fin : p.Fin <= m.Inicio ? m.Inicio - p.Fin : 0,
                              Despues: p.Inicio >= m.Fin ? 0 : 1))
                .OrderBy(p => p.Distancia).ThenBy(p => p.Despues)
                .First().Valor;
            medios.Add(new ComparacionDeMedio(clave, esperado, true, afirmado));
        }
        return new VerificacionDeResumen(false, medios);
    }

    /// <summary>Términos con que se reconoce un medio: su clave y su etiqueta, normalizadas.</summary>
    public static IReadOnlyList<string> Terminos(string clave)
    {
        var etiqueta = Catalogos.Medios.Single(m => m.Clave == clave).Etiqueta;
        return [.. new[] { Normalizar(clave), Normalizar(etiqueta) }.Distinct()];
    }

    public static string Normalizar(string texto)
    {
        var sb = new StringBuilder(texto.Length);
        foreach (var c in texto.ToLowerInvariant())
        {
            sb.Append(c switch { 'á' => 'a', 'é' => 'e', 'í' => 'i', 'ó' => 'o', 'ú' => 'u', 'ü' => 'u', _ => c });
        }
        return sb.ToString();
    }

    public static string Formato(double valor) => valor.ToString("0.#", CultureInfo.InvariantCulture);

    private static List<(int Inicio, int Fin, double Valor)> EnLaClausula(
        List<(int Inicio, int Fin, double Valor)> porcentajes, (int Inicio, int Fin) clausula) =>
        [.. porcentajes.Where(p => p.Inicio >= clausula.Inicio && p.Fin <= clausula.Fin)];

    /// <summary>La cláusula (tramo entre cortes) que contiene la posición <paramref name="posicion" />.</summary>
    private static (int Inicio, int Fin) Clausula(string texto, Regex corte, int posicion)
    {
        var inicio = 0;
        foreach (Match c in corte.Matches(texto))
        {
            if (c.Index > posicion) return (inicio, c.Index);
            inicio = c.Index + c.Length;
        }
        return (inicio, texto.Length);
    }

    private static (int Inicio, int Fin)? PrimeraMencion(string texto, string clave) =>
        Terminos(clave)
            .SelectMany(t => Regex.Matches(texto, $@"\b{Regex.Escape(t)}").Select(m => (Inicio: m.Index, Fin: m.Index + m.Length)))
            .OrderBy(m => m.Inicio).ThenByDescending(m => m.Fin - m.Inicio)
            .Select(m => ((int, int)?)m)
            .FirstOrDefault();

    [GeneratedRegex(@"([0-9]+(?:[.,][0-9]+)?)[ \t\u00a0]*(?:%|por ?ciento)")]
    private static partial Regex Porcentaje();

    [GeneratedRegex(@"\.(?![0-9])|[;\n]|\by\b|\be\b|,(?![0-9])")]
    private static partial Regex CorteCorto();

    [GeneratedRegex(@"\.(?![0-9])|[;\n]|\by\b|\be\b")]
    private static partial Regex CorteLargo();
}
