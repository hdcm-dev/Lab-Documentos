using FrameworkConcept.Core.Contracts.Puente;

namespace FrameworkConcept.Content.Bridge;

/// <summary>
/// El cascarón contestó un sobre con «error», el contenido corre fuera del cascarón (sin puente), o el transporte falló
/// (<see cref="EsDelTransporte"/>).
/// <see cref="Code"/> es uno de <see cref="CodigosDeError"/>; las páginas lo usan para elegir qué decir.
/// </summary>
public sealed class BridgeException(string metodo, int code, string mensaje, Exception? causa = null)
    : Exception($"{metodo}: {mensaje} ({code})", causa)
{
    /// <summary>Texto que puente.js pone en el error cuando no encuentra window.HybridWebView.</summary>
    internal const string MarcaSinPuente = "SIN_PUENTE";

    public string Metodo { get; } = metodo;

    public int Code { get; } = code;

    /// <summary>El mensaje del cascarón, sin el método ni el código: es el texto para la persona.</summary>
    public string Mensaje { get; } = mensaje;

    /// <summary>No hay cascarón: el contenido se abrió en un navegador común.</summary>
    public bool EsSinPuente { get; private init; }

    public bool EsConflicto => Code == CodigosDeError.ConflictoDeVersion;

    /// <summary>La llamada no llegó a tener respuesta del cascarón (el transporte del puente falló). Código ErrorInterno.</summary>
    public bool EsDelTransporte { get; private init; }

    internal static BridgeException DelTransporte(string metodo, Exception causa) =>
        new(metodo, CodigosDeError.ErrorInterno, "El cascarón no respondió a la llamada. Probá de nuevo.", causa) { EsDelTransporte = true };

    internal static BridgeException SinPuente() =>
        new("bridge", 0, "No hay puente: este contenido sólo corre dentro del cascarón.") { EsSinPuente = true };
}
