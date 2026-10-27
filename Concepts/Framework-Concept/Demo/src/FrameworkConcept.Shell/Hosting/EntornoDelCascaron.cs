using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Core.Update;

namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// P3 — Lo que el predicado de compatibilidad necesita saber de ESTE cascarón: canal, versión del puente, catálogo
/// (la constante de Core, P12), WebView y versión del cascarón (para la política de soporte de P2).
/// </summary>
public sealed class EntornoDelCascaron(ConfiguracionDelCascaron configuracion, MotorWeb motor) : IEntornoDelCascaron
{
    public string Canal => configuracion.Canal;

    public VersionSemantica VersionDelPuente => ContratoDelPuente.Version;

    public IReadOnlyList<EntradaDelCatalogo> Catalogo => CatalogoDeCapacidades.Entradas;

    public int? VersionMayorDelWebView => motor.VersionMayor;

    public VersionSemantica VersionDelCascaron =>
        VersionSemantica.TryParse(AppInfo.Current.VersionString, out var v) ? v : VersionSemantica.Crear(0, 0, 0);
}
