// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Domain/Rules/LocalidadRules.cs @ 8fb4c3a
// Delta: Modificado a causa de la IA — endurecimiento T2 (provincia contra Catalogos); endurecimiento REC-17 (código postal sólo con dígitos 0-9; largo máximo del nombre aplicado); Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using System.Text.RegularExpressions;

using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Domain.Rules;

/// <summary>
/// Reglas de negocio del ABM de localidades. Viven en el dominio, no en atributos del modelo
/// de pantalla, para que la validación no dependa de la interfaz que la invoque.
/// </summary>
public static partial class LocalidadRules
{
    public const int LargoMinimoDelNombre = 3;
    public const int LargoMaximoDelNombre = 60;
    public const int HabitantesMinimos = 1;

    /// <summary>Dígitos exactos del código postal. El patrón de <see cref="CodigoPostal" /> lo espeja.</summary>
    public const int DigitosDelCodigoPostal = 4;

    // Endurecimiento REC-17: el caso base declaraba el máximo y no lo aplicaba; un nombre de 61
    // caracteres que propone el modelo pasaba. Ahora el largo va de LargoMinimo a LargoMaximo.
    public static bool NombreValido(string? nombre) =>
        (nombre ?? string.Empty).Trim().Length is >= LargoMinimoDelNombre and <= LargoMaximoDelNombre;

    // Endurecimiento T2 (hueco H1): el caso base sólo exigía «no vacío». Con un modelo que propone
    // texto, «Corrientes Capital» pasaba; ahora la provincia tiene que ser una clave exacta del catálogo.
    public static bool ProvinciaValida(string? provincia) =>
        provincia is not null && Catalogos.Provincias.Contains(provincia, StringComparer.Ordinal);

    public static bool CodigoPostalValido(string? codigoPostal) =>
        CodigoPostal().IsMatch((codigoPostal ?? string.Empty).Trim());

    public static bool HabitantesValidos(int? habitantes) =>
        habitantes is not null && habitantes >= HabitantesMinimos;

    /// <summary>Dos localidades son la misma si comparten nombre —sin distinguir mayúsculas— y provincia.</summary>
    public static bool MismaLocalidad(string nombreA, string provinciaA, string nombreB, string provinciaB) =>
        string.Equals(nombreA.Trim(), nombreB.Trim(), StringComparison.OrdinalIgnoreCase) &&
        string.Equals(provinciaA, provinciaB, StringComparison.Ordinal);

    // Endurecimiento REC-17: \d de .NET acepta cualquier dígito Unicode («٣٤٥٠» pasaba); sólo 0-9.
    [GeneratedRegex(@"^[0-9]{4}$")]
    private static partial Regex CodigoPostal();
}
