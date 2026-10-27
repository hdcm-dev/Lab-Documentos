using System.Xml.Linq;
using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Gate.Tests.Infraestructura;

namespace FrameworkConcept.Gate.Tests.Catalogo;

/// <summary>
/// P12 — El manifiesto de la app (el que sólo cambia por tienda) declara exactamente lo que el catálogo de
/// capacidades implementa: ni una capacidad sin su permiso, ni un permiso sin una capacidad que lo use.
/// Las tiendas rechazan permisos para funciones que el binario no ejerce (Play: «unimplemented»; Apple 2.5.19).
///
/// Se lee el AndroidManifest.xml FUENTE del cascarón como XML. El combinado que arma el build de Android no
/// se mira: necesitaría compilar el APK, y la compuerta rápida no lo hace.
/// </summary>
[TestFixture]
public class CatalogoFrenteAlManifiestoTests
{
    private const string RutaManifiesto = "src/FrameworkConcept.Shell/Platforms/Android/AndroidManifest.xml";
    private const string RutaInfoPlist = "src/FrameworkConcept.Shell/Platforms/iOS/Info.plist";

    private static readonly XNamespace Android = "http://schemas.android.com/apk/res/android";

    /// <summary>
    /// Permisos que el CASCARÓN usa para sí y que no son capacidades del contenido. Cada uno con su motivo:
    /// agregar uno acá es una decisión que se revisa en el PR, igual que una entrada del catálogo.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> PermisosPropiosDelCascaron = new Dictionary<string, string>
    {
        ["android.permission.INTERNET"] = "el actualizador baja el canal (P2) y la sincronización envía el outbox (P5)"
    };

    private static XDocument LeerManifiesto()
    {
        var ruta = Repositorio.Ruta(RutaManifiesto);
        if (!File.Exists(ruta))
            Assert.Fail($"No existe {RutaManifiesto}. El cascarón tiene que traer su manifiesto fuente para que la compuerta lo compare con el catálogo (P12).");
        return XDocument.Load(ruta);
    }

    private static HashSet<string> PermisosDeclarados(XDocument manifiesto) =>
        [.. manifiesto.Root!.Elements()
            .Where(e => e.Name.LocalName is "uses-permission" or "uses-permission-sdk-23")
            .Select(e => (string?)e.Attribute(Android + "name") ?? "")];

    [Test]
    [Description("P12 — Cada permiso que pide una entrada del catálogo está declarado en el AndroidManifest")]
    public void CadaCapacidadTieneSuPermiso()
    {
        var declarados = PermisosDeclarados(LeerManifiesto());

        var faltan = CatalogoDeCapacidades.Entradas
            .SelectMany(e => e.PermisosAndroid.Select(p => (e.Id, Permiso: p)))
            .Where(x => !declarados.Contains(x.Permiso))
            .Select(x => $"«{x.Id}» necesita {x.Permiso}")
            .ToList();

        Assert.That(faltan, Is.Empty, $"Capacidades del catálogo sin su permiso en {RutaManifiesto}:\n  - " + string.Join("\n  - ", faltan));
    }

    [Test]
    [Description("P12 — El AndroidManifest no declara permisos que ninguna capacidad del catálogo implementa")]
    public void NoHayPermisosSinCapacidad()
    {
        var usados = CatalogoDeCapacidades.Entradas.SelectMany(e => e.PermisosAndroid).ToHashSet(StringComparer.Ordinal);

        var sobran = PermisosDeclarados(LeerManifiesto())
            .Where(p => !usados.Contains(p) && !PermisosPropiosDelCascaron.ContainsKey(p))
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.That(sobran, Is.Empty,
            $"{RutaManifiesto} declara permisos que ninguna entrada del catálogo implementa (P12: «declara sólo lo que el catálogo implementa»). " +
            "O se agrega la entrada al catálogo con su uso visible en la semilla, o se quita el permiso:\n  - " + string.Join("\n  - ", sobran));
    }

    [Test]
    [Description("P12 — Features: el catálogo de la demo no declara ninguna, así que el manifiesto tampoco")]
    public void NoHayFeaturesSinCatalogo()
    {
        var extra = LeerManifiesto().Root!.Elements()
            .Where(e => e.Name.LocalName == "uses-feature")
            .Select(e => $"uses-feature {(string?)e.Attribute(Android + "name")}")
            .ToList();

        Assert.That(extra, Is.Empty,
            $"{RutaManifiesto} declara {string.Join(", ", extra)}, y EntradaDelCatalogo no tiene dónde declararlo. " +
            "Primero el catálogo tiene que poder nombrarlo (P12: «lo que declara en cada plataforma»).");
    }

    [Test]
    [Description("P12 — <queries>: las acciones de intent del manifiesto son exactamente las que declaran las entradas del catálogo")]
    public void QueriesCoincidenConElCatalogo()
    {
        var root = LeerManifiesto().Root!;
        var queries = root.Elements().Where(e => e.Name.LocalName == "queries").ToList();

        // Dentro de <queries> la demo sólo admite <intent><action/></intent>: <package> o <provider> no tienen
        // dónde declararse en EntradaDelCatalogo.
        var otros = queries.SelectMany(q => q.Elements())
            .Where(e => e.Name.LocalName != "intent")
            .Select(e => $"<{e.Name.LocalName}>")
            .ToList();
        var declaradas = queries.SelectMany(q => q.Elements().Where(e => e.Name.LocalName == "intent"))
            .SelectMany(i => i.Elements().Where(e => e.Name.LocalName == "action"))
            .Select(a => (string?)a.Attribute(Android + "name") ?? "")
            .ToHashSet(StringComparer.Ordinal);
        var esperadas = CatalogoDeCapacidades.Entradas.SelectMany(e => e.ConsultasAndroidDeclaradas).ToHashSet(StringComparer.Ordinal);

        Assert.Multiple(() =>
        {
            Assert.That(otros, Is.Empty, $"{RutaManifiesto}: <queries> con elementos que el catálogo no puede nombrar");
            Assert.That(esperadas.Except(declaradas), Is.Empty, $"Consultas que el catálogo pide y {RutaManifiesto} no declara en <queries>");
            Assert.That(declaradas.Except(esperadas), Is.Empty, $"Consultas de {RutaManifiesto} que ninguna entrada del catálogo implementa");
        });
    }

    [Test]
    [Description("P12, P9 — Info.plist fuente: si existe, sus claves NS*UsageDescription coinciden con el catálogo")]
    public void InfoPlistFuenteSiExiste()
    {
        var ruta = Repositorio.Ruta(RutaInfoPlist);
        var esperadas = CatalogoDeCapacidades.Entradas.SelectMany(e => e.ClavesDeUsoIos).ToHashSet(StringComparer.Ordinal);

        if (!File.Exists(ruta))
        {
            // P9: iOS es diseño sin ensayo y no se compila. Sin Info.plist no hay nada que comparar; si el
            // catálogo llegara a pedir claves de uso, el día que exista el archivo esta prueba las exige.
            Assert.Pass($"No hay {RutaInfoPlist}: rama iOS sin compilar (P9, NO VERIFICADO). Claves que el catálogo pediría: {(esperadas.Count == 0 ? "ninguna" : string.Join(", ", esperadas))}.");
        }

        var claves = XDocument.Load(ruta).Descendants("key")
            .Select(k => k.Value.Trim())
            .Where(k => k.StartsWith("NS", StringComparison.Ordinal) && k.EndsWith("UsageDescription", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);

        Assert.Multiple(() =>
        {
            Assert.That(esperadas.Except(claves), Is.Empty, $"Claves de uso que el catálogo pide y {RutaInfoPlist} no trae");
            Assert.That(claves.Except(esperadas), Is.Empty, $"Claves de uso de {RutaInfoPlist} que ninguna entrada del catálogo implementa");
        });
    }
}
