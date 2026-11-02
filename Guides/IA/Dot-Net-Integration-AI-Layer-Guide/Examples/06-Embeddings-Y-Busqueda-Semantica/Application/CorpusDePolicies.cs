// Nuevo: el corpus de 06 (plan §E.2, §F.6; H-026, H2-027). Un fragmento por cada propiedad
// `public static string` de LocalidadPolicy (4) y EncuestaPolicy (8), leída por reflexión: si cambia
// una regla, cambia el texto vectorizado (N6 P3). Nada de este archivo se escribe a mano.
using System.Reflection;
using System.Text.RegularExpressions;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>Un fragmento del corpus. <see cref="Id" /> va calificado por clase: «RequisitoDelNombre» está en las dos.</summary>
public sealed record Fragmento(string Id, string Entidad, string Propiedad, string Literal, string Texto);

public static partial class CorpusDePolicies
{
    /// <summary>Versión de la plantilla; se versiona con el oro (conjunto-de-oro.v1.json la nombra).</summary>
    public const string Plantilla = "plantilla.v1";

    // Las únicas palabras de los nombres de propiedad que llevan tilde en castellano.
    private static readonly Dictionary<string, string> Tildes = new(StringComparer.Ordinal)
    {
        ["codigo"] = "código"
    };

    public static IReadOnlyList<Fragmento> Fragmentos() =>
        [.. DeLaClase(typeof(LocalidadPolicy)), .. DeLaClase(typeof(EncuestaPolicy))];

    /// <summary>plantilla.v1: «{entidad} · {propiedad en palabras}: {literal}». La línea base léxica usa el mismo texto.</summary>
    public static string Texto(string entidad, string propiedad, string literal) =>
        $"{entidad} · {EnPalabras(propiedad)}: {literal}";

    /// <summary>«RequisitoDelCodigoPostal» → «requisito del código postal».</summary>
    public static string EnPalabras(string propiedad) =>
        string.Join(' ', Mayusculas().Split(propiedad)
            .Where(p => p.Length > 0)
            .Select(p => p.ToLowerInvariant())
            .Select(p => Tildes.GetValueOrDefault(p, p)));

    private static IEnumerable<Fragmento> DeLaClase(Type clase)
    {
        var entidad = clase.Name[..^"Policy".Length];
        // Orden de declaración (MetadataToken): el de los archivos copiados de 8fb4c3a.
        return clase.GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(string))
            .OrderBy(p => p.MetadataToken)
            .Select(p =>
            {
                var literal = (string)p.GetValue(null)!;
                return new Fragmento($"{clase.Name}.{p.Name}", entidad, p.Name, literal, Texto(entidad, p.Name, literal));
            });
    }

    [GeneratedRegex("(?<!^)(?=[A-Z])")]
    private static partial Regex Mayusculas();
}
