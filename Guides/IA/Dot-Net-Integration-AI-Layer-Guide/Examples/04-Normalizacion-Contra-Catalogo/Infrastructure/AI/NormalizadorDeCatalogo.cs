// Nuevo: el adaptador que normaliza texto a claves de Catalogos. La tabla de sinónimos y el plegado
// son «escrito para el ejemplo»; los dos están copiados en tools/oraculos/normalizacion-comun.jq para
// que los oráculos se recalculen desde el registro (si cambia uno, cambia el otro, y las entradas
// pasan a v2). Pruebas/paridad-del-oraculo.v1.json fija los casos en que tienen que coincidir.
using System.Text;
using MovilidadUrbana.Asistente.Application.Normalizacion;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

public sealed class NormalizadorDeCatalogo : INormalizadorDeCatalogo
{
    // Plegado explícito y finito (escrito para el ejemplo), a propósito: sin ToLowerInvariant ni
    // Normalize, que dependen de las tablas Unicode de la plataforma y que jq no puede reproducir
    // (y Normalize tira ArgumentException con U+FFFE). Fuera de estas reglas, el carácter queda igual:
    // un texto raro no se vuelve clave, se queda como está y ValidarPaso lo rechaza.
    private static readonly Dictionary<char, char> SinTilde = new()
    {
        ['á'] = 'a', ['à'] = 'a', ['â'] = 'a', ['ä'] = 'a', ['Á'] = 'a', ['À'] = 'a', ['Â'] = 'a', ['Ä'] = 'a',
        ['é'] = 'e', ['è'] = 'e', ['ê'] = 'e', ['ë'] = 'e', ['É'] = 'e', ['È'] = 'e', ['Ê'] = 'e', ['Ë'] = 'e',
        ['í'] = 'i', ['ì'] = 'i', ['î'] = 'i', ['ï'] = 'i', ['Í'] = 'i', ['Ì'] = 'i', ['Î'] = 'i', ['Ï'] = 'i',
        ['ó'] = 'o', ['ò'] = 'o', ['ô'] = 'o', ['ö'] = 'o', ['Ó'] = 'o', ['Ò'] = 'o', ['Ô'] = 'o', ['Ö'] = 'o',
        ['ú'] = 'u', ['ù'] = 'u', ['û'] = 'u', ['ü'] = 'u', ['Ú'] = 'u', ['Ù'] = 'u', ['Û'] = 'u', ['Ü'] = 'u',
        ['ñ'] = 'n', ['Ñ'] = 'n', ['ç'] = 'c', ['Ç'] = 'c'
    };

    /// <summary>Lo que se quita de los bordes: espacios ASCII y signos de puntuación.</summary>
    private static readonly char[] Bordes = [' ', '\t', '\n', '\r', '\f', '\v', '.', ',', ';', ':', '!', '?', '¡', '¿', '"', '\''];

    // Sinónimos rioplatenses → clave (escrito para el ejemplo). Las claves y etiquetas de Catalogos
    // se agregan solas abajo, así que la tabla sólo lleva lo que el catálogo no dice.
    public static readonly IReadOnlyDictionary<string, string> SinonimosDeMedios = new Dictionary<string, string>
    {
        ["bondi"] = "colectivo", ["micro"] = "colectivo", ["omnibus"] = "colectivo", ["bus"] = "colectivo", ["colectivos"] = "colectivo",
        ["coche"] = "auto", ["automovil"] = "auto", ["el auto"] = "auto",
        ["bici"] = "bicicleta", ["la bici"] = "bicicleta",
        ["motocicleta"] = "moto",
        ["a pie"] = "caminata", ["caminando"] = "caminata", ["camino"] = "caminata", ["caminar"] = "caminata", ["pie"] = "caminata",
        ["el tren"] = "tren", ["ferrocarril"] = "tren"
    };

    public static readonly IReadOnlyDictionary<string, string> SinonimosDeFrecuencias = new Dictionary<string, string>
    {
        ["todos los dias"] = "diaria", ["casi todos los dias"] = "diaria", ["a diario"] = "diaria", ["cada dia"] = "diaria", ["de lunes a domingo"] = "diaria",
        ["algunos dias"] = "semanal", ["varias veces por semana"] = "semanal", ["un par de veces por semana"] = "semanal", ["una vez por semana"] = "semanal",
        ["ocasionalmente"] = "ocasional", ["a veces"] = "ocasional", ["de vez en cuando"] = "ocasional", ["rara vez"] = "ocasional"
    };

    private static readonly Dictionary<string, string> Medios = Tabla(Catalogos.Medios, SinonimosDeMedios);
    private static readonly Dictionary<string, string> Frecuencias = Tabla(Catalogos.Frecuencias, SinonimosDeFrecuencias);

    public string NormalizarMedio(string texto) => Buscar(Medios, texto);

    public string NormalizarFrecuencia(string texto) => Buscar(Frecuencias, texto);

    /// <summary>
    /// Sin clave, devuelve el texto plegado (minúsculas, sin tildes): «Avión» → «avion». No lo
    /// descarta: así ValidarPaso lo ve y lo rechaza con su nombre.
    /// </summary>
    private static string Buscar(Dictionary<string, string> tabla, string texto)
    {
        var plegado = Plegar(texto);
        return tabla.TryGetValue(plegado, out var clave) ? clave : plegado;
    }

    private static Dictionary<string, string> Tabla(IReadOnlyList<(string Clave, string Etiqueta)> catalogo, IReadOnlyDictionary<string, string> sinonimos)
    {
        var tabla = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (clave, etiqueta) in catalogo)
        {
            tabla[Plegar(clave)] = clave;
            tabla[Plegar(etiqueta)] = clave;
        }
        foreach (var (texto, clave) in sinonimos)
        {
            tabla[Plegar(texto)] = clave;
        }
        return tabla;
    }

    /// <summary>
    /// Minúsculas ASCII, sin las tildes de la tabla, sin marcas combinantes (U+0300–U+036F), sin
    /// <see cref="Bordes" /> en las puntas y con espacios simples. Un texto que queda vacío («  »,
    /// «¿?») no es un medio: <see cref="NormalizacionDeMovilidad" /> lo saltea.
    /// </summary>
    public static string Plegar(string texto)
    {
        var sb = new StringBuilder(texto.Length);
        foreach (var c in texto)
        {
            if (c is >= 'A' and <= 'Z') sb.Append((char)(c + ('a' - 'A')));
            else if (SinTilde.TryGetValue(c, out var sinTilde)) sb.Append(sinTilde);
            else if (c is >= '\u0300' and <= '\u036F') continue;
            else sb.Append(c);
        }
        var limpio = sb.ToString().Trim(Bordes);
        return string.Join(' ', limpio.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
