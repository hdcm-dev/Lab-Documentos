using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Contenido;

namespace FrameworkConcept.Core.Update;

/// <summary>
/// P3 — El predicado único de compatibilidad, evaluado antes de activar un paquete:
///
///   puente ∈ compatibleConPuente
///   ∧ ∀ r ∈ requires: r.id ∈ catálogo ∧ versión(catálogo[r.id]) ∈ r.rango
///   ∧ motor ≥ mínimo
///
/// Evalúa las tres dimensiones siempre, aunque la primera ya falle, para que el diagnóstico diga TODO lo que falta.
/// <c>optional</c> no entra: lo resuelve el contenido con el handshake (P12).
/// </summary>
public sealed class PredicadoDeCompatibilidad : IPredicadoDeCompatibilidad
{
    public ResultadoDeCompatibilidad Evaluar(ManifiestoContenido manifiesto, IEntornoDelCascaron entorno)
    {
        var motivos = new List<string>();

        if (!manifiesto.CompatibleConPuente.Contiene(entorno.VersionDelPuente))
            motivos.Add($"puente {entorno.VersionDelPuente} fuera de {manifiesto.CompatibleConPuente}");

        foreach (var requisito in manifiesto.Capabilities.Requires)
        {
            var entrada = entorno.Catalogo.FirstOrDefault(e => string.Equals(e.Id, requisito.Id, StringComparison.Ordinal));
            if (entrada is null)
                motivos.Add($"requires «{requisito.Id}»: el catálogo del cascarón no la tiene");
            else if (!requisito.Rango.Contiene(entrada.Version))
                motivos.Add($"requires «{requisito.Id}»: versión {entrada.Version} fuera de {requisito.Rango}");
        }

        // El mínimo es el mayor entre lo que pide el paquete y el piso del producto (C-3 de E01): un paquete no puede
        // pedir MENOS que 94. Si no se pudo leer la versión del WebView, no se activa: no se sabe si alcanza.
        var minimo = Math.Max(manifiesto.Motor.AndroidWebView, ParametrosDeProducto.WebViewMinimo);
        if (entorno.VersionMayorDelWebView is not int webView)
            motivos.Add($"motor: no se pudo leer la versión del WebView (mínimo {minimo})");
        else if (webView < minimo)
            motivos.Add($"motor: WebView {webView} < mínimo {minimo}");

        return new ResultadoDeCompatibilidad(motivos.Count == 0, motivos);
    }
}
