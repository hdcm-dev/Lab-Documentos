// Nuevo. Delta: Agregado por la IA — normalización contra Catalogos ANTES de validar (plan §F.3, N6 §6).
using System.Globalization;
using System.Text;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

/// <summary>
/// Lleva lo que escribió una persona (o lo que propuso el modelo) a una clave de <see cref="Catalogos" />.
/// Si no encuentra una, devuelve el texto tal como vino: la regla endurecida del dominio lo rechaza
/// (T2). Nunca inventa una clave para «salvar» un valor: «avion» sigue siendo «avion».
///
/// Los sinónimos son ESCRITOS PARA EL EJEMPLO (estímulo de prueba, plan §E.1): formas habituales de
/// nombrar los medios, frecuencias y motivos del catálogo; no salen del caso base.
/// </summary>
public sealed class NormalizadorDeCatalogo
{
    private static readonly Dictionary<string, string> SinonimosDeMedio = new()
    {
        ["bondi"] = "colectivo", ["micro"] = "colectivo", ["omnibus"] = "colectivo", ["colectivos"] = "colectivo",
        ["bici"] = "bicicleta", ["bicis"] = "bicicleta",
        ["coche"] = "auto", ["auto particular"] = "auto", ["mi auto"] = "auto",
        ["motocicleta"] = "moto",
        ["a pie"] = "caminata", ["caminando"] = "caminata", ["camino"] = "caminata",
        ["ferrocarril"] = "tren"
    };

    private static readonly Dictionary<string, string> SinonimosDeFrecuencia = new()
    {
        ["todos los dias"] = "diaria", ["a diario"] = "diaria", ["casi todos los dias"] = "diaria", ["diario"] = "diaria",
        ["algunos dias"] = "semanal", ["algunos dias por semana"] = "semanal", ["varias veces por semana"] = "semanal",
        ["a veces"] = "ocasional", ["de vez en cuando"] = "ocasional", ["ocasionalmente"] = "ocasional"
    };

    private static readonly Dictionary<string, string> SinonimosDeMotivo = new()
    {
        ["laburo"] = "trabajo", ["ir a trabajar"] = "trabajo",
        ["facultad"] = "estudio", ["escuela"] = "estudio", ["estudiar"] = "estudio", ["colegio"] = "estudio",
        ["medico"] = "salud", ["hospital"] = "salud",
        ["otro"] = "otros"
    };

    /// <summary>Minúsculas, sin tildes, sin espacios repetidos.</summary>
    public static string Plano(string? texto)
    {
        var descompuesto = (texto ?? string.Empty).Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return string.Join(' ', sb.ToString().Normalize(NormalizationForm.FormC).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public string? Provincia(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var plano = Plano(texto);
        return Catalogos.Provincias.FirstOrDefault(p => Plano(p) == plano) ?? texto.Trim();
    }

    public string Medio(string texto) => Clave(texto, Catalogos.Medios, SinonimosDeMedio);

    public string? Frecuencia(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : Clave(texto, Catalogos.Frecuencias, SinonimosDeFrecuencia);

    public string? Motivo(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : Clave(texto, Catalogos.Motivos, SinonimosDeMotivo);

    private static string Clave(string texto, IReadOnlyList<(string Clave, string Etiqueta)> catalogo, Dictionary<string, string> sinonimos)
    {
        var plano = Plano(texto);
        foreach (var (clave, etiqueta) in catalogo)
        {
            if (plano == clave || plano == Plano(etiqueta)) return clave;
        }
        return sinonimos.TryGetValue(plano, out var deSinonimo) ? deSinonimo : plano;
    }
}
