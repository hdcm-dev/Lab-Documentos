using System.Text.Json.Nodes;
using System.Xml.Linq;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Gate.Tests.Infraestructura;
using static FrameworkConcept.Core.Contracts.Puente.ContratoDelPuente;

namespace FrameworkConcept.Gate.Tests.Contrato;

/// <summary>
/// P3 — Instantánea del contrato del puente (tabla de métodos + catálogo + transporte).
///
/// Tres comparaciones:
///   1. el código contra <c>contrato.json</c> del repositorio: alguien cambió la tabla y no regeneró la
///      instantánea, o subió la versión sin cambiar nada;
///   2. <c>contrato.json</c> contra el de la base del PR (lo publicado): la versión subió lo que P3 manda;
///   3. las tres fuentes de la versión del puente dicen lo mismo, y el transporte conoce el mayor de MAUI.
/// Esta suite BLOQUEA el PR (P6, P8): un contenido publicado contra un contrato que cambió en silencio es
/// irreversible para quien ya lo bajó.
/// </summary>
[TestFixture]
public class InstantaneaDelContratoTests
{
    private const string RutaContrato = "src/FrameworkConcept.Core/Contracts/contrato.json";
    private const string ComoRegenerar = "Después de subir ContratoDelPuente.Version, regenerá la instantánea con: dotnet run --project tools/FrameworkConcept.Tools -- contrato";

    private static Instantanea LeerInstantanea(string ruta) => JsonDelContrato.Leer<Instantanea>(File.ReadAllText(ruta));

    [Test]
    [Description("P3 — El contrato del código coincide con contrato.json; si no, dice qué cambió y qué versión corresponde")]
    public void ElCodigoCoincideConContratoJson()
    {
        var versionado = LeerInstantanea(Repositorio.Ruta(RutaContrato));
        var actual = Actual();

        var clasificacion = ClasificadorDeCambios.Clasificar(versionado, actual);
        var problemaDeVersion = ClasificadorDeCambios.ProblemaDeVersion(clasificacion.Nivel, versionado.Version, actual.Version);

        if (problemaDeVersion is not null)
            Assert.Fail(Mensaje($"Código frente a {RutaContrato}: {problemaDeVersion}.", clasificacion) + "\n" + ComoRegenerar);

        // La clasificación y la versión están bien, pero el archivo tiene que ser exactamente el que genera
        // el código (por ejemplo, alguien subió la versión y cambió la tabla, pero no regeneró el archivo).
        var texto = File.ReadAllText(Repositorio.Ruta(RutaContrato)).Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.That(texto, Is.EqualTo(SerializarInstantanea()),
            $"{RutaContrato} no es la instantánea del código actual. {ComoRegenerar}");
    }

    [Test]
    [Description("P3 — Frente a lo publicado (base del PR): la versión del puente sube exactamente lo que el cambio exige")]
    public void LaVersionSubeSegunLaClasificacionFrenteALaBase()
    {
        var baseDelPr = Repositorio.BaseObligatoria();
        var rutaBase = Path.Combine(baseDelPr, RutaContrato.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(rutaBase))
            Assert.Pass($"La base del PR no tiene {RutaContrato}: no hay contrato publicado con qué comparar (el PR que crea la demo).");

        var publicado = LeerInstantanea(rutaBase);
        var propuesto = LeerInstantanea(Repositorio.Ruta(RutaContrato));

        var clasificacion = ClasificadorDeCambios.Clasificar(publicado, propuesto);
        var problema = ClasificadorDeCambios.ProblemaDeVersion(clasificacion.Nivel, publicado.Version, propuesto.Version);

        Assert.That(problema, Is.Null, Mensaje($"Base del PR ({publicado.Version}) frente a este PR ({propuesto.Version}): {problema}.", clasificacion));
    }

    [Test]
    [Description("P7 — La versión del puente es una sola: ContratoDelPuente.Version = versionado/puente.json = contrato.json")]
    public void LasFuentesDeLaVersionDelPuenteCoinciden()
    {
        var puenteJson = JsonNode.Parse(File.ReadAllText(Repositorio.Ruta("versionado/puente.json")))!["version"]?.GetValue<string>();
        var contratoJson = LeerInstantanea(Repositorio.Ruta(RutaContrato)).Version;

        Assert.Multiple(() =>
        {
            Assert.That(puenteJson, Is.EqualTo(ContratoDelPuente.Version.ToString()), "versionado/puente.json frente a ContratoDelPuente.Version");
            Assert.That(contratoJson, Is.EqualTo(ContratoDelPuente.Version), $"{RutaContrato} frente a ContratoDelPuente.Version");
        });
    }

    [Test]
    [Description("P3 — El transporte lleva el mayor de MAUI de Directory.Build.props: subir MAUI de mayor se ve en la instantánea")]
    public void ElTransporteConoceElMayorDeMauiDelRepositorio()
    {
        var props = XDocument.Load(Repositorio.Ruta("Directory.Build.props"));
        var mauiVersion = props.Descendants("MauiVersion").Select(e => e.Value.Trim()).SingleOrDefault();

        Assert.That(mauiVersion, Is.Not.Null.And.Not.Empty, "Directory.Build.props no fija MauiVersion (P6 punto 8).");
        Assert.That(VersionSemantica.TryParse(mauiVersion, out var maui), Is.True, $"MauiVersion «{mauiVersion}» no es MAYOR.MENOR.PARCHE.");
        Assert.That(TransporteDelPuente.Actual.MauiMayor, Is.EqualTo(maui.Mayor),
            "El ensamblado de Core no grabó el mayor de MauiVersion: la instantánea no vería un cambio de MAUI.");
    }

    private static string Mensaje(string encabezado, ResultadoDeClasificacion clasificacion) =>
        clasificacion.Motivos.Count == 0
            ? encabezado + " (sin diferencias en la tabla, el catálogo ni el transporte)"
            : encabezado + " Diferencias:\n  - " + string.Join("\n  - ", clasificacion.Motivos);
}
