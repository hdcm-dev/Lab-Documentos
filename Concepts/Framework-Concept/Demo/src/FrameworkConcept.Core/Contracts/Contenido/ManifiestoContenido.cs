namespace FrameworkConcept.Core.Contracts.Contenido;

/// <summary>
/// El manifiesto del paquete de contenido: <c>manifiesto.json</c> en la raíz del zip. Lo escribe el
/// empaquetador; lo lee el actualizador ANTES de activar, para evaluar el predicado de compatibilidad (P3):
///
///   puente ∈ compatibleConPuente
///   ∧ ∀ r ∈ requires: r.id ∈ catálogo ∧ versión(catálogo[r.id]) ∈ r.rango
///   ∧ motor ≥ mínimo
///
/// El piso declarado se rotula «compatible en teoría con» mientras ninguna prueba lo revalide (VPI §2.6).
/// </summary>
/// <param name="Version">SemVer con +commit, sin pre-release (P7).</param>
/// <param name="CompatibleConPuente">Rango con techo: <c>&gt;=1.0.0 &lt;2.0.0</c>.</param>
/// <param name="Capabilities">P12 — Lo que el contenido necesita (requires) y lo que aprovecha si está (optional).</param>
/// <param name="Motor">C-3 de E01 — Motor web mínimo.</param>
public sealed record ManifiestoContenido(
    VersionSemantica Version,
    RangoDeVersion CompatibleConPuente,
    CapacidadesDelContenido Capabilities,
    MotorMinimo Motor)
{
    public const string NombreDeArchivo = "manifiesto.json";
}

/// <summary>P12 — <c>capabilities: { requires, optional }</c>.</summary>
public sealed record CapacidadesDelContenido(
    IReadOnlyList<RequisitoDeCapacidad> Requires,
    IReadOnlyList<RequisitoDeCapacidad> Optional);

/// <summary>Una capacidad pedida por id, con rango con techo sobre la versión de la entrada del catálogo (P3, P12).</summary>
public sealed record RequisitoDeCapacidad(string Id, RangoDeVersion Rango);

/// <summary>Motor web mínimo. Android: mayor del proveedor de WebView activo. iOS: diseño, NO VERIFICADO.</summary>
public sealed record MotorMinimo(int AndroidWebView, string Ios)
{
    public static MotorMinimo DelProducto { get; } = new(ParametrosDeProducto.WebViewMinimo, ParametrosDeProducto.IosMinimo);
}
