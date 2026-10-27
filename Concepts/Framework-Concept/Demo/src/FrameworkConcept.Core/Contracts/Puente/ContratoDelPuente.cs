using System.Security.Cryptography;
using System.Text;

namespace FrameworkConcept.Core.Contracts.Puente;

/// <summary>
/// El contrato del puente = tabla de métodos + catálogo (id y versión) + transporte (P3).
///
/// Detección de cambios (P3, P8): <see cref="SerializarInstantanea"/> escribe esas tres piezas en un orden
/// fijo; la prueba de la compuerta la compara con <c>Contracts/contrato.json</c>, que está versionado.
///   — si difieren y la versión no subió → rojo;
///   — si la versión subió y no difieren → rojo.
/// Para regenerar contrato.json después de un cambio intencional: <c>FrameworkConcept.Tools contrato</c>.
/// </summary>
public static class ContratoDelPuente
{
    /// <summary>P7 — SemVer del puente. El mayor es la identidad del contrato. Coincide con versionado/puente.json.</summary>
    public static VersionSemantica Version { get; } = VersionSemantica.Parse("1.1.0");

    /// <summary>Lo que se compara: sin descripciones (cambiar un comentario no cambia el contrato).</summary>
    public sealed record Instantanea(
        VersionSemantica Version,
        TransporteDelPuente Transporte,
        IReadOnlyList<FilaDeMetodo> Metodos,
        IReadOnlyList<FilaDeEvento> Eventos,
        IReadOnlyList<EntradaVersionada> Catalogo);

    public sealed record FilaDeMetodo(string Nombre, IReadOnlyList<CampoDelSobre> Parametros, IReadOnlyList<CampoDelSobre> Respuesta, string? Capacidad);

    public sealed record FilaDeEvento(string Nombre, IReadOnlyList<CampoDelSobre> Parametros);

    /// <summary>La instantánea de ESTE código, ordenada por nombre para que el orden de declaración no cuente.</summary>
    public static Instantanea Actual() => new(
        Version,
        TransporteDelPuente.Actual,
        [.. TablaDeMetodos.Metodos.OrderBy(m => m.Nombre, StringComparer.Ordinal)
            .Select(m => new FilaDeMetodo(m.Nombre, m.Parametros, m.Respuesta, m.Capacidad))],
        [.. TablaDeMetodos.Eventos.OrderBy(e => e.Nombre, StringComparer.Ordinal)
            .Select(e => new FilaDeEvento(e.Nombre, e.Parametros))],
        [.. CatalogoDeCapacidades.Versionadas().OrderBy(e => e.Id, StringComparer.Ordinal)]);

    /// <summary>El texto de contrato.json que corresponde a este código (fin de línea «\n», con salto final).</summary>
    public static string SerializarInstantanea() => JsonDelContrato.Escribir(Actual()) + "\n";

    /// <summary>contrato.json tal como quedó versionado y embebido en el ensamblado.</summary>
    public static string LeerInstantaneaVersionada()
    {
        using var stream = typeof(ContratoDelPuente).Assembly.GetManifestResourceStream("contrato.json")
            ?? throw new InvalidOperationException("contrato.json no está embebido en FrameworkConcept.Core.");
        using var lector = new StreamReader(stream, Encoding.UTF8);
        return lector.ReadToEnd().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    /// <summary>Huella (sha256 en hex) de contrato.json. Dato de diagnóstico, no una línea de versión (P7).</summary>
    public static string Huella() =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(LeerInstantaneaVersionada()))).ToLowerInvariant();
}
