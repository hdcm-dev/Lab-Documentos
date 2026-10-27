using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrameworkConcept.Core.Contracts;

/// <summary>
/// Un rango de compatibilidad con piso y techo: <c>&gt;=A.B.C &lt;(A+1).0.0</c>.
///
/// P3 — Es la forma canónica de Versionado-Producto-Integral §2.6: «nunca un piso a secas». El techo es
/// siempre el próximo mayor, porque un mayor nuevo es, por definición, un contrato distinto.
/// Se usa en el manifiesto del contenido (compatible con puente, <c>requires</c>, <c>optional</c>) y en
/// la definición de encuesta (compatible con contenido).
/// </summary>
[JsonConverter(typeof(RangoDeVersionJsonConverter))]
public sealed class RangoDeVersion
{
    private RangoDeVersion(VersionSemantica piso, VersionSemantica techo)
    {
        Piso = piso;
        Techo = techo;
    }

    /// <summary>Incluido: <c>&gt;=</c>.</summary>
    public VersionSemantica Piso { get; }

    /// <summary>Excluido: <c>&lt;</c>. Siempre <c>(Piso.Mayor + 1).0.0</c>.</summary>
    public VersionSemantica Techo { get; }

    /// <summary>El rango canónico que empieza en <paramref name="piso"/>.</summary>
    public static RangoDeVersion Desde(VersionSemantica piso) =>
        new(piso.SinBuild(), VersionSemantica.Crear(piso.Mayor + 1, 0, 0));

    public static RangoDeVersion Parse(string texto) =>
        TryParse(texto, out var rango, out var error) ? rango : throw new FormatException(error);

    public static bool TryParse(string? texto, out RangoDeVersion rango, out string error)
    {
        rango = null!;
        error = string.Empty;
        var partes = (texto ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length != 2 || !partes[0].StartsWith(">=", StringComparison.Ordinal) || !partes[1].StartsWith('<'))
        {
            error = $"«{texto}» no tiene la forma «>=A.B.C <D.0.0» (piso y techo, en ese orden).";
            return false;
        }
        if (!VersionSemantica.TryParse(partes[0][2..], out var piso) || !VersionSemantica.TryParse(partes[1][1..], out var techo))
        {
            error = $"«{texto}»: el piso o el techo no son versiones SemVer.";
            return false;
        }
        if (techo.CompareTo(VersionSemantica.Crear(piso.Mayor + 1, 0, 0)) != 0 || techo.Build is not null)
        {
            error = $"«{texto}»: el techo tiene que ser el próximo mayor del piso, <{piso.Mayor + 1}.0.0.";
            return false;
        }

        rango = new RangoDeVersion(piso, techo);
        return true;
    }

    /// <summary>¿La versión está dentro? Se compara por precedencia: el build no cuenta.</summary>
    public bool Contiene(VersionSemantica version) => version >= Piso && version < Techo;

    public override string ToString() => $">={Piso} <{Techo}";
}

public sealed class RangoDeVersionJsonConverter : JsonConverter<RangoDeVersion>
{
    public override RangoDeVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        RangoDeVersion.TryParse(reader.GetString(), out var rango, out var error) ? rango : throw new JsonException(error);

    public override void Write(Utf8JsonWriter writer, RangoDeVersion value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
