using FrameworkConcept.Core.Contracts.Puente;

namespace FrameworkConcept.Shell.Catalogo;

/// <summary>
/// P12 — Lo que el cascarón hace por cada entrada del catálogo. El catálogo (id y versión) es la constante de Core,
/// <see cref="CatalogoDeCapacidades"/>; acá está la implementación de Android. Toda capacidad del dispositivo pasa por
/// el puente: el contenido no usa API web del WebView que pidan permisos.
///
/// Catálogo de la demo: sólo lo que el contenido usa y sin permisos de tiempo de ejecución. ACCESS_NETWORK_STATE es
/// un permiso «normal» (se concede al instalar) y está declarado en Platforms/Android/AndroidManifest.xml, que no
/// declara nada más del catálogo (la compuerta lo compara, P8).
/// Una capacidad nueva entra «en dos tiempos» y en el mismo binario: entrada del catálogo + declaración del manifiesto
/// + una función visible que la use (P12).
/// </summary>
public static class ImplementacionesDelCatalogo
{
    public static IReadOnlyDictionary<string, Func<object>> Todas { get; } = new Dictionary<string, Func<object>>(StringComparer.Ordinal)
    {
        ["device.info"] = () => new
        {
            fabricante = DeviceInfo.Current.Manufacturer,
            modelo = DeviceInfo.Current.Model,
            plataforma = DeviceInfo.Current.Platform.ToString(),
            versionSistema = DeviceInfo.Current.VersionString
        },
    };

    /// <summary>
    /// Folio 038 — Las entradas que se atienden con servicios del cascarón (estado de red, sesión) y no con una función
    /// suelta: network.status → Red/AtencionDeRed.cs; auth.session → Auth/AtencionDeSesion.cs.
    /// </summary>
    public static IReadOnlySet<string> AtendidasPorServicios { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "network.status", "auth.session"
    };

    /// <summary>¿Este binario implementa la entrada?</summary>
    public static bool Implementa(string id) => Todas.ContainsKey(id) || AtendidasPorServicios.Contains(id);

    /// <summary>
    /// Que el catálogo y las implementaciones coincidan uno a uno. Si no, es un defecto de este binario: se anota
    /// en la bitácora al arrancar y el puente contesta CapacidadNoDisponible para lo que falte.
    /// </summary>
    public static IReadOnlyList<string> Diferencias()
    {
        var declaradas = CatalogoDeCapacidades.Entradas.Select(e => e.Id).ToHashSet(StringComparer.Ordinal);
        return
        [
            .. declaradas.Where(id => !Implementa(id)).Select(id => $"«{id}» está en el catálogo y no tiene implementación"),
            .. Todas.Keys.Concat(AtendidasPorServicios).Except(declaradas).Select(id => $"«{id}» tiene implementación y no está en el catálogo")
        ];
    }
}
