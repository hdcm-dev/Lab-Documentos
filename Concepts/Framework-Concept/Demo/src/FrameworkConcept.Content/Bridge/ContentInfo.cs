using System.Reflection;
using System.Text;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Contenido;

namespace FrameworkConcept.Content.Bridge;

/// <summary>
/// Lo que este paquete de contenido sabe de sí mismo: su versión y lo que declara necesitar.
/// </summary>
public static class ContentInfo
{
    /// <summary>
    /// P7 — SemVer del contenido, con +commit si el empaquetador lo pasó. Sale del .csproj
    /// (versionado/contenido.json o -p:ContenidoVersion).
    /// </summary>
    public static VersionSemantica Version { get; } = VersionSemantica.Parse(
        typeof(ContentInfo).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(a => a.Key == "ContenidoVersion").Value!);

    /// <summary>
    /// El cambio visible de la v2 de la demo (contenido 1.1.0 o mayor): tarjeta de novedades en el Inicio y
    /// otro color de cabecera. Lo enciende el .csproj con la constante CONTENIDO_NOVEDADES. En un producto real
    /// la v2 sería simplemente otro código; acá se compilan las dos variantes desde el mismo fuente.
    /// </summary>
    public static bool ConNovedades { get; } =
#if CONTENIDO_NOVEDADES
        true;
#else
        false;
#endif

    /// <summary>P3, P12 — declaracion-del-contenido.json, embebido en el ensamblado.</summary>
    public static DeclaracionDelContenido Declaracion { get; } = LeerDeclaracion();

    private static DeclaracionDelContenido LeerDeclaracion()
    {
        using var stream = typeof(ContentInfo).Assembly.GetManifestResourceStream("declaracion-del-contenido.json")
            ?? throw new InvalidOperationException("declaracion-del-contenido.json no está embebido en el contenido.");
        using var lector = new StreamReader(stream, Encoding.UTF8);
        return JsonDelContrato.Leer<DeclaracionDelContenido>(lector.ReadToEnd());
    }
}

/// <summary>
/// La parte del manifiesto del paquete que escribe quien programa el contenido (el empaquetador le suma la
/// versión y el motor mínimo para formar <see cref="ManifiestoContenido"/>).
/// </summary>
/// <param name="CompatibleConPuente">Rango con techo de versiones del puente con las que este contenido funciona.</param>
/// <param name="Capabilities">P12 — requires: sin ellas el actualizador no activa el paquete; optional: el contenido degrada.</param>
public sealed record DeclaracionDelContenido(RangoDeVersion CompatibleConPuente, CapacidadesDelContenido Capabilities);
