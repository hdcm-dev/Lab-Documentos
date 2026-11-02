// Nuevo (plan §E.2, fila 05; P-12 decidida en el ciclo 3, opción B). El corpus de 05 es un padrón
// de 70 LocalidadModel ficticios, «Localidad 01…70», ESCRITO PARA EL EJEMPLO. Lo genera una
// función en C# (no hay script Python: H-025, DC-07), se serializa UNA vez y queda versionado en
// corpus/padron-de-localidades.v1.json con su SHA-256 en ejemplo.json. La corrida lee el archivo
// congelado, no vuelve a generar: así el prefijo es el mismo byte a byte entre corridas.
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Application;

public static class PadronDeLocalidades
{
    public const int Cantidad = 70;
    public const string Rotulo = "escrito para el ejemplo";

    private static readonly JsonSerializerOptions Json = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    /// <summary>
    /// Genera el padrón. Determinista: la provincia rota sobre <see cref="Catalogos.Provincias" />,
    /// el código postal es de 4 dígitos y los habitantes son ≥ 1, así que cada registro pasa
    /// <c>LocalidadRules</c> (lo comprueba una prueba).
    /// </summary>
    public static IReadOnlyList<LocalidadModel> Generar() =>
        Enumerable.Range(1, Cantidad).Select(i => new LocalidadModel
        {
            Nombre = $"Localidad {i:00}",
            Provincia = Catalogos.Provincias[(i - 1) % Catalogos.Provincias.Count],
            CodigoPostal = (1000 + (i * 271) % 9000).ToString("D4", CultureInfo.InvariantCulture),
            Habitantes = 800 + (i * 7919) % 120000
        }).ToList();

    /// <summary>Serializa el padrón con su rótulo (sólo se usa para congelar el archivo v1).</summary>
    public static string Serializar(IEnumerable<LocalidadModel> padron) =>
        JsonSerializer.Serialize(new
        {
            rotulo = Rotulo,
            version = "v1",
            generador = "PadronDeLocalidades.Generar() (C#), serializado una vez",
            registros = padron.Select(l => new { l.Nombre, l.Provincia, l.CodigoPostal, l.Habitantes })
        }, Json) + "\n";

    /// <summary>Lee el padrón congelado.</summary>
    public static IReadOnlyList<LocalidadModel> Leer(string ruta)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(ruta));
        return doc.RootElement.GetProperty("registros").EnumerateArray().Select(r => new LocalidadModel
        {
            Nombre = r.GetProperty("Nombre").GetString() ?? string.Empty,
            Provincia = r.GetProperty("Provincia").GetString() ?? string.Empty,
            CodigoPostal = r.GetProperty("CodigoPostal").GetString() ?? string.Empty,
            Habitantes = r.GetProperty("Habitantes").GetInt32()
        }).ToList();
    }

    /// <summary>Un registro por renglón, en un orden fijo: el texto que va en el prefijo.</summary>
    public static string ATexto(IEnumerable<LocalidadModel> padron)
    {
        var sb = new StringBuilder();
        sb.Append("Nombre | Provincia | Código postal | Habitantes\n");
        foreach (var l in padron)
        {
            sb.Append(CultureInfo.InvariantCulture, $"{l.Nombre} | {l.Provincia} | {l.CodigoPostal} | {l.Habitantes}\n");
        }
        return sb.ToString();
    }
}
