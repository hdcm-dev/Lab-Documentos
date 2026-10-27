using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FrameworkConcept.Core.Contracts;

/// <summary>
/// Una versión SemVer 2.0.0: <c>MAYOR.MENOR.PARCHE[-prerelease][+build]</c>.
/// La usan el puente y el contenido (P7). El orden sigue la «precedencia» de SemVer: los metadatos
/// de build (lo que va después de <c>+</c>, en la demo el commit) no cuentan para ordenar.
/// </summary>
[JsonConverter(typeof(VersionSemanticaJsonConverter))]
public sealed partial class VersionSemantica : IComparable<VersionSemantica>, IEquatable<VersionSemantica>
{
    private VersionSemantica(int mayor, int menor, int parche, string? preRelease, string? build)
    {
        Mayor = mayor;
        Menor = menor;
        Parche = parche;
        PreRelease = preRelease;
        Build = build;
    }

    public int Mayor { get; }
    public int Menor { get; }
    public int Parche { get; }

    /// <summary>Lo que va después de <c>-</c>. El contenido nunca lo lleva (P7).</summary>
    public string? PreRelease { get; }

    /// <summary>Lo que va después de <c>+</c>. En el contenido, el commit del que se construyó.</summary>
    public string? Build { get; }

    public bool EsPreRelease => PreRelease is not null;

    public static VersionSemantica Crear(int mayor, int menor, int parche) => new(mayor, menor, parche, null, null);

    public static VersionSemantica Parse(string texto) =>
        TryParse(texto, out var version)
            ? version
            : throw new FormatException($"«{texto}» no es una versión SemVer 2.0.0 (MAYOR.MENOR.PARCHE).");

    public static bool TryParse(string? texto, out VersionSemantica version)
    {
        version = null!;
        if (texto is null) return false;

        var m = Patron().Match(texto);
        if (!m.Success) return false;

        version = new VersionSemantica(
            int.Parse(m.Groups["mayor"].Value, CultureInfo.InvariantCulture),
            int.Parse(m.Groups["menor"].Value, CultureInfo.InvariantCulture),
            int.Parse(m.Groups["parche"].Value, CultureInfo.InvariantCulture),
            m.Groups["pre"].Success ? m.Groups["pre"].Value : null,
            m.Groups["build"].Success ? m.Groups["build"].Value : null);
        return true;
    }

    /// <summary>La misma versión sin metadatos de build. Dos versiones que sólo difieren en el build tienen la misma precedencia.</summary>
    public VersionSemantica SinBuild() => new(Mayor, Menor, Parche, PreRelease, null);

    /// <summary>Compara por precedencia SemVer (§11): ignora el build; una pre-release es menor que su versión normal.</summary>
    public int CompareTo(VersionSemantica? otra)
    {
        if (otra is null) return 1;

        var c = Mayor.CompareTo(otra.Mayor);
        if (c != 0) return c;
        c = Menor.CompareTo(otra.Menor);
        if (c != 0) return c;
        c = Parche.CompareTo(otra.Parche);
        if (c != 0) return c;

        if (PreRelease is null) return otra.PreRelease is null ? 0 : 1;
        if (otra.PreRelease is null) return -1;
        return CompararPreRelease(PreRelease, otra.PreRelease);
    }

    private static int CompararPreRelease(string a, string b)
    {
        var partesA = a.Split('.');
        var partesB = b.Split('.');
        for (var i = 0; i < Math.Min(partesA.Length, partesB.Length); i++)
        {
            var esNumeroA = int.TryParse(partesA[i], NumberStyles.None, CultureInfo.InvariantCulture, out var na);
            var esNumeroB = int.TryParse(partesB[i], NumberStyles.None, CultureInfo.InvariantCulture, out var nb);
            int c = (esNumeroA, esNumeroB) switch
            {
                (true, true) => na.CompareTo(nb),
                (true, false) => -1,
                (false, true) => 1,
                _ => string.CompareOrdinal(partesA[i], partesB[i])
            };
            if (c != 0) return c;
        }
        return partesA.Length.CompareTo(partesB.Length);
    }

    /// <summary>Igualdad por texto completo, build incluido: es la identidad de un paquete, que no se reutiliza (P7).</summary>
    public bool Equals(VersionSemantica? otra) => otra is not null && ToString() == otra.ToString();

    public override bool Equals(object? obj) => Equals(obj as VersionSemantica);

    public override int GetHashCode() => ToString().GetHashCode(StringComparison.Ordinal);

    public override string ToString() =>
        $"{Mayor}.{Menor}.{Parche}"
        + (PreRelease is null ? "" : "-" + PreRelease)
        + (Build is null ? "" : "+" + Build);

    public static bool operator <(VersionSemantica a, VersionSemantica b) => a.CompareTo(b) < 0;
    public static bool operator >(VersionSemantica a, VersionSemantica b) => a.CompareTo(b) > 0;
    public static bool operator <=(VersionSemantica a, VersionSemantica b) => a.CompareTo(b) <= 0;
    public static bool operator >=(VersionSemantica a, VersionSemantica b) => a.CompareTo(b) >= 0;

    // Gramática de semver.org, sin ceros a la izquierda en los números.
    [GeneratedRegex(@"^(?<mayor>0|[1-9]\d*)\.(?<menor>0|[1-9]\d*)\.(?<parche>0|[1-9]\d*)"
        + @"(?:-(?<pre>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?"
        + @"(?:\+(?<build>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$")]
    private static partial Regex Patron();
}

/// <summary>Escribe y lee una <see cref="VersionSemantica"/> como texto JSON («1.2.3+abc1234»).</summary>
public sealed class VersionSemanticaJsonConverter : JsonConverter<VersionSemantica>
{
    public override VersionSemantica Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        VersionSemantica.TryParse(reader.GetString(), out var v)
            ? v
            : throw new JsonException($"Versión SemVer inválida: «{reader.GetString()}».");

    public override void Write(Utf8JsonWriter writer, VersionSemantica value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
