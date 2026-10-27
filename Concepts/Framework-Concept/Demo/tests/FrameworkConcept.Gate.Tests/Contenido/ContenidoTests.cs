using System.Text.RegularExpressions;
using System.Xml.Linq;
using FrameworkConcept.Gate.Tests.Infraestructura;

namespace FrameworkConcept.Gate.Tests.Contenido;

/// <summary>
/// P1, P8, P12 — El contenido descargable:
///   - se compila SIN AOT. Con AOT el código bajado deja de ser IL interpretado y cambia de clase ante las
///     tiendas; habilitarlo es una decisión con su propia revisión, no una optimización (P1);
///   - no usa API web del WebView que piden permisos (geolocalización, cámara o micrófono): toda capacidad
///     del dispositivo pasa por el puente, donde la declara el catálogo (P12).
/// Se leen los archivos fuente: lo que se escribe en el repositorio es lo que llega al paquete.
/// </summary>
[TestFixture]
public class ContenidoTests
{
    private const string CarpetaDelContenido = "src/FrameworkConcept.Content";
    private const string ProyectoDelContenido = CarpetaDelContenido + "/FrameworkConcept.Content.csproj";

    /// <summary>
    /// Archivos de MSBuild que pueden fijar RunAOTCompilation para el contenido: su .csproj, los
    /// Directory.Build.props/.targets desde su carpeta hasta la raíz de la demo y sus perfiles de publicación.
    /// </summary>
    private static IEnumerable<string> ArchivosDeMsBuild()
    {
        var proyecto = Repositorio.Ruta(ProyectoDelContenido);
        if (!File.Exists(proyecto))
            Assert.Fail($"No existe {ProyectoDelContenido}.");
        yield return proyecto;

        var raiz = new DirectoryInfo(Repositorio.Raiz).FullName;
        for (var dir = new DirectoryInfo(Repositorio.Ruta(CarpetaDelContenido)); dir is not null; dir = dir.Parent)
        {
            foreach (var nombre in new[] { "Directory.Build.props", "Directory.Build.targets" })
            {
                var archivo = Path.Combine(dir.FullName, nombre);
                if (File.Exists(archivo)) yield return archivo;
            }
            if (string.Equals(dir.FullName.TrimEnd(Path.DirectorySeparatorChar), raiz.TrimEnd(Path.DirectorySeparatorChar), StringComparison.Ordinal))
                break;
        }

        var perfiles = Repositorio.Ruta(CarpetaDelContenido + "/Properties/PublishProfiles");
        if (Directory.Exists(perfiles))
        {
            foreach (var perfil in Directory.EnumerateFiles(perfiles, "*.pubxml")) yield return perfil;
        }
    }

    [Test]
    [Description("P1 — RunAOTCompilation falso o ausente en todo archivo de MSBuild que alcanza al contenido")]
    public void ElContenidoNoUsaAot()
    {
        var problemas = new List<string>();

        foreach (var archivo in ArchivosDeMsBuild())
        {
            foreach (var propiedad in XDocument.Load(archivo).Descendants().Where(e => e.Name.LocalName == "RunAOTCompilation"))
            {
                var valor = propiedad.Value.Trim();
                if (!string.Equals(valor, "false", StringComparison.OrdinalIgnoreCase))
                {
                    var condicion = (string?)propiedad.Attribute("Condition") ?? (string?)propiedad.Parent?.Attribute("Condition");
                    problemas.Add($"{Path.GetRelativePath(Repositorio.Raiz, archivo)}: RunAOTCompilation = «{valor}»{(condicion is null ? "" : $" (Condition: {condicion})")}");
                }
            }
        }

        Assert.That(problemas, Is.Empty,
            "El contenido tiene que compilarse SIN AOT (P1). Habilitarlo es una decisión con su propia revisión de conformidad ante las tiendas:\n  - "
            + string.Join("\n  - ", problemas));
    }

    /// <summary>API web que piden permisos al usuario desde el WebView. P8 nombra geolocalización, getUserMedia y capture.</summary>
    private static readonly (string Nombre, Regex Patron, string[] Extensiones)[] ApiQuePidenPermisos =
    [
        ("navigator.geolocation", new Regex(@"navigator\s*\.\s*geolocation", RegexOptions.Compiled), [".js", ".mjs", ".razor", ".cs", ".html", ".cshtml"]),
        ("getUserMedia", new Regex(@"\bgetUserMedia\b", RegexOptions.Compiled), [".js", ".mjs", ".razor", ".cs", ".html", ".cshtml"]),
        ("navigator.mediaDevices", new Regex(@"navigator\s*\.\s*mediaDevices", RegexOptions.Compiled), [".js", ".mjs", ".razor", ".cs", ".html", ".cshtml"]),
        // El atributo capture de <input type="file"> abre la cámara o el micrófono.
        ("<input capture>", new Regex(@"<input\b[^>]*\scapture\b", RegexOptions.Compiled | RegexOptions.IgnoreCase), [".razor", ".html", ".cshtml"])
    ];

    [Test]
    [Description("P8, P12 — El contenido no llama API web del WebView que piden permisos: eso va por el puente")]
    public void ElContenidoNoUsaApiWebQuePidenPermisos()
    {
        var carpeta = Repositorio.Ruta(CarpetaDelContenido);
        var excluidas = new[] { "bin", "obj" };
        var hallazgos = new List<string>();

        foreach (var archivo in Directory.EnumerateFiles(carpeta, "*", SearchOption.AllDirectories))
        {
            var relativa = Path.GetRelativePath(carpeta, archivo);
            if (excluidas.Contains(relativa.Split(Path.DirectorySeparatorChar)[0])) continue;

            var extension = Path.GetExtension(archivo).ToLowerInvariant();
            var aplicables = ApiQuePidenPermisos.Where(a => a.Extensiones.Contains(extension)).ToList();
            if (aplicables.Count == 0) continue;

            var lineas = File.ReadAllLines(archivo);
            for (var i = 0; i < lineas.Length; i++)
            {
                foreach (var api in aplicables.Where(a => a.Patron.IsMatch(lineas[i])))
                    hallazgos.Add($"{CarpetaDelContenido}/{relativa.Replace(Path.DirectorySeparatorChar, '/')}:{i + 1}: {api.Nombre}");
            }
        }

        Assert.That(hallazgos, Is.Empty,
            "El contenido usa API web que piden permisos. Toda capacidad del dispositivo pasa por el puente y el catálogo (P12):\n  - "
            + string.Join("\n  - ", hallazgos));
    }
}
