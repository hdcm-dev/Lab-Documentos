using FrameworkConcept.Core.Contracts.Puente;

namespace FrameworkConcept.Content.Bridge;

/// <summary>
/// El cascarón contestó un sobre con «error», o el contenido corre fuera del cascarón (sin puente).
/// <see cref="Code"/> es uno de <see cref="CodigosDeError"/>; las páginas lo usan para elegir qué decir.
/// </summary>
public sealed class BridgeException(string metodo, int code, string mensaje)
    : Exception($"{metodo}: {mensaje} ({code})")
{
    /// <summary>Texto que puente.js pone en el error cuando no encuentra window.HybridWebView.</summary>
    internal const string MarcaSinPuente = "SIN_PUENTE";

    public string Metodo { get; } = metodo;

    public int Code { get; } = code;

    /// <summary>No hay cascarón: el contenido se abrió en un navegador común.</summary>
    public bool EsSinPuente { get; private init; }

    public bool EsConflicto => Code == CodigosDeError.ConflictoDeVersion;

    internal static BridgeException SinPuente() =>
        new("bridge", 0, "No hay puente: este contenido sólo corre dentro del cascarón.") { EsSinPuente = true };
}
