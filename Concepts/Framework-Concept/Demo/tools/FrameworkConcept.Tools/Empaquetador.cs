using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Contenido;
using FrameworkConcept.Core.Contracts.Puente;

namespace FrameworkConcept.Tools;

/// <summary>
/// «empaquetar»: convierte la salida de <c>dotnet publish</c> del contenido Blazor en un paquete de contenido:
/// un zip PLANO con <c>manifiesto.json</c> en la raíz, que «firmar» agrega después al índice del canal.
///
/// Qué comprueba antes de escribir nada (si algo falla, no se escribe el paquete):
///   — la versión es SemVer sin pre-release, y estrictamente mayor que las ya publicadas en el canal (P7, P2);
///   — el proyecto no compila con AOT (P1);
///   — index.html trae &lt;base href="/"&gt; (condición C-2 de E01);
///   — el código propio del contenido no usa API web que pidan permisos (P12: todo pasa por el puente).
/// Qué deja afuera: los .br y .gz precomprimidos (C-4 de E01: el servidor de contenido responde el archivo tal
/// cual, sin negociar Content-Encoding).
/// </summary>
internal static class Empaquetador
{
    // Fecha fija para todas las entradas: el mismo contenido da el mismo zip, byte a byte, y el mismo sha256.
    private static readonly DateTimeOffset FechaFija = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static int Empaquetar(Argumentos a)
    {
        var carpeta = CarpetaDelCanal.Desde(a);
        var publicado = RaizPublicada(a.Requerida("publicado"));
        var version = LeerVersion(a.Requerida("version"), a.Opcion("commit"));

        ComprobarSinAot(a.Opcion("csproj", "src/FrameworkConcept.Content/FrameworkConcept.Content.csproj"));
        ComprobarIndexHtml(publicado);
        ComprobarSinApiConPermisos(publicado);

        var declaracion = LeerDeclaracion(a);
        var manifiesto = new ManifiestoContenido(version, declaracion.CompatibleConPuente, declaracion.Capabilities, MotorMinimo.DelProducto);
        AvisarCapacidadesFueraDelCatalogo(manifiesto.Capabilities);

        var archivos = ArchivosPlanos(publicado);
        var bytesDelManifiesto = EscribirManifiesto(manifiesto, archivos);
        var zip = ArmarZip(bytesDelManifiesto, archivos);

        var destino = carpeta.RutaDelPaquete(version);
        ComprobarIdentidadYMonotonia(carpeta, version, destino, zip);

        CarpetaDelCanal.EscribirAtomico(destino, zip);
        var sha = CarpetaDelCanal.Sha256(zip);
        carpeta.Anotar("empaquetar", new { version = version.ToString(), ruta = carpeta.Relativa(destino), size = zip.Length, sha256 = sha });

        Console.WriteLine($"empaquetar: contenido {version} → {destino}");
        Console.WriteLine($"  archivos: {archivos.Count} ({archivos.Sum(f => f.Bytes.Length)} B sin comprimir), zip: {zip.Length} B");
        Console.WriteLine($"  sha256: {sha}");
        Console.WriteLine($"  compatibleConPuente: {manifiesto.CompatibleConPuente} (compatible en teoría con: ningún ensayo lo revalidó, P3)");
        Console.WriteLine($"  requires: [{string.Join(", ", manifiesto.Capabilities.Requires.Select(r => $"{r.Id} {r.Rango}"))}]" +
                          $"  optional: [{string.Join(", ", manifiesto.Capabilities.Optional.Select(r => $"{r.Id} {r.Rango}"))}]");
        Console.WriteLine("  Sigue: «firmar» para que el índice del canal lo ofrezca.");
        return 0;
    }

    /// <summary><c>dotnet publish -o X</c> deja el sitio en X/wwwroot; se aceptan las dos formas.</summary>
    private static string RaizPublicada(string ruta)
    {
        var wwwroot = Path.Combine(ruta, "wwwroot");
        var raiz = Directory.Exists(wwwroot) ? wwwroot : ruta;
        if (!File.Exists(Path.Combine(raiz, "index.html")) || !Directory.Exists(Path.Combine(raiz, "_framework")))
            throw new ErrorDeUso($"«{ruta}» no parece la salida de dotnet publish de un Blazor WebAssembly (falta index.html o _framework/)");
        return raiz;
    }

    /// <summary>P7 — SemVer 2.0.0 con +commit y SIN pre-release: la madurez la da el canal, no un sufijo.</summary>
    private static VersionSemantica LeerVersion(string texto, string? commit)
    {
        if (!VersionSemantica.TryParse(texto, out var version))
            throw new ErrorDeUso($"«{texto}» no es una versión SemVer (MAYOR.MENOR.PARCHE)");
        if (version.EsPreRelease)
            throw new PublicacionRechazada($"P7: el contenido no lleva pre-release en ningún canal («{version}»).");

        if (commit is null || version.Build is not null) return version;
        if (!Regex.IsMatch(commit, "^[0-9A-Za-z-]+$"))
            throw new ErrorDeUso($"commit «{commit}» inválido para metadatos SemVer");
        return VersionSemantica.Parse($"{version}+{commit}");
    }

    /// <summary>
    /// P1 — Sin AOT. Se lee el .csproj del contenido: si alguna propiedad RunAOTCompilation vale true, se rechaza.
    /// (Un -p:RunAOTCompilation=true en la línea de dotnet publish no se ve acá; por eso la compuerta del PR
    /// también lo prueba, P8.)
    /// </summary>
    private static void ComprobarSinAot(string csproj)
    {
        if (!File.Exists(csproj))
            throw new ErrorDeUso($"no existe {csproj} (opción --csproj)");

        var conAot = XDocument.Load(csproj).Descendants()
            .Where(e => e.Name.LocalName == "RunAOTCompilation")
            .Any(e => string.Equals(e.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase));
        if (conAot)
            throw new PublicacionRechazada($"P1: {csproj} activa RunAOTCompilation. El contenido se publica SIN AOT; habilitarlo es una decisión con su propia revisión.");
    }

    /// <summary>C-2 de E01: sin &lt;base href="/"&gt; las rutas del contenido no resuelven contra el servidor local.</summary>
    private static void ComprobarIndexHtml(string raiz)
    {
        var html = File.ReadAllText(Path.Combine(raiz, "index.html"));
        if (!Regex.IsMatch(html, @"<base\s+href\s*=\s*""/""", RegexOptions.IgnoreCase))
            throw new PublicacionRechazada("C-2 (E01): index.html no trae <base href=\"/\">.");
        if (!html.Contains("hybridwebview.js", StringComparison.Ordinal))
            Console.WriteLine("  aviso C-2 (E01): index.html no nombra _framework/hybridwebview.js; sin él no hay puente.");
    }

    /// <summary>
    /// P12 — Toda capacidad del dispositivo pasa por el puente. Se revisa el código PROPIO del contenido
    /// (lo que no está en _framework/, que es el runtime de .NET): si llama a API web que piden permisos, se rechaza.
    /// </summary>
    private static void ComprobarSinApiConPermisos(string raiz)
    {
        string[] prohibidas = ["navigator.geolocation", "getUserMedia", "capture="];
        foreach (var archivo in Directory.EnumerateFiles(raiz, "*", SearchOption.AllDirectories))
        {
            var relativa = Path.GetRelativePath(raiz, archivo).Replace('\\', '/');
            if (relativa.StartsWith("_framework/", StringComparison.Ordinal)) continue;
            if (!relativa.EndsWith(".js", StringComparison.Ordinal) && !relativa.EndsWith(".html", StringComparison.Ordinal)) continue;

            var texto = File.ReadAllText(archivo);
            var usada = prohibidas.FirstOrDefault(p => texto.Contains(p, StringComparison.Ordinal));
            if (usada is not null)
                throw new PublicacionRechazada($"P12: {relativa} usa «{usada}». Las capacidades del dispositivo se piden por el puente, no por la API web del WebView.");
        }
    }

    /// <summary>
    /// P12 — Lo que el contenido declara (rango del puente y capabilities) sale de UNA fuente: el archivo
    /// declaracion-del-contenido.json del proyecto del contenido, que también lee el contenido para decidir qué
    /// funciones opcionales muestra. Las opciones --puente-piso, --requires y --optional lo reemplazan (para
    /// armar paquetes de prueba, por ejemplo uno que pide una capacidad que el cascarón no tiene).
    /// </summary>
    private sealed record Declaracion(RangoDeVersion CompatibleConPuente, CapacidadesDelContenido Capabilities);

    private static Declaracion LeerDeclaracion(Argumentos a)
    {
        var ruta = a.Opcion("declaracion", "src/FrameworkConcept.Content/declaracion-del-contenido.json");
        var desdeArchivo = File.Exists(ruta)
            ? JsonDelContrato.Leer<Declaracion>(File.ReadAllText(ruta))
            : new Declaracion(RangoDeVersion.Desde(VersionSemantica.Crear(ContratoDelPuente.Version.Mayor, 0, 0)), new CapacidadesDelContenido([], []));

        var rango = a.Opcion("puente-piso") is { } piso ? RangoDeVersion.Desde(VersionSemantica.Parse(piso)) : desdeArchivo.CompatibleConPuente;
        var requires = a.Todas("requires").Count > 0 ? LeerCapacidades(a.Todas("requires")) : desdeArchivo.Capabilities.Requires;
        var optional = a.Todas("optional").Count > 0 ? LeerCapacidades(a.Todas("optional")) : desdeArchivo.Capabilities.Optional;
        return new Declaracion(rango, new CapacidadesDelContenido(requires, optional));
    }

    /// <summary>«device.info@1.0.0» → id device.info, rango canónico &gt;=1.0.0 &lt;2.0.0 (P3, P12).</summary>
    private static List<RequisitoDeCapacidad> LeerCapacidades(IReadOnlyList<string> textos)
    {
        var lista = new List<RequisitoDeCapacidad>();
        foreach (var texto in textos)
        {
            var partes = texto.Split('@');
            if (partes.Length != 2 || !VersionSemantica.TryParse(partes[1], out var piso))
                throw new ErrorDeUso($"capacidad «{texto}»: se escribe id@piso, por ejemplo device.info@1.0.0");
            lista.Add(new RequisitoDeCapacidad(partes[0], RangoDeVersion.Desde(piso)));
        }
        return lista;
    }

    private static void AvisarCapacidadesFueraDelCatalogo(CapacidadesDelContenido capacidades)
    {
        foreach (var r in capacidades.Requires.Where(r => CatalogoDeCapacidades.Buscar(r.Id) is null))
            Console.WriteLine($"  aviso P12: requires «{r.Id}» no está en el catálogo de este cascarón: el actualizador no activará el paquete en él.");
        foreach (var r in capacidades.Optional.Where(r => CatalogoDeCapacidades.Buscar(r.Id) is null))
            Console.WriteLine($"  aviso P12: optional «{r.Id}» no está en el catálogo de este cascarón: el contenido oculta esa función.");
    }

    private sealed record ArchivoPlano(string Ruta, byte[] Bytes);

    /// <summary>C-4 de E01 — Sólo los archivos planos, ordenados por ruta para que el zip sea reproducible.</summary>
    private static List<ArchivoPlano> ArchivosPlanos(string raiz)
    {
        var archivos = Directory.EnumerateFiles(raiz, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".br", StringComparison.Ordinal) && !f.EndsWith(".gz", StringComparison.Ordinal))
            .Select(f => new ArchivoPlano(Path.GetRelativePath(raiz, f).Replace('\\', '/'), File.ReadAllBytes(f)))
            .OrderBy(f => f.Ruta, StringComparer.Ordinal)
            .ToList();

        if (archivos.Any(f => f.Ruta == ManifiestoContenido.NombreDeArchivo))
            throw new PublicacionRechazada($"el contenido publicado ya trae un {ManifiestoContenido.NombreDeArchivo}: ese nombre es del empaquetador.");
        return archivos;
    }

    /// <summary>
    /// El manifiesto del paquete: los campos de <see cref="ManifiestoContenido"/> más «archivos» (ruta, size y
    /// sha256 de cada uno). Quien lee el manifiesto como <see cref="ManifiestoContenido"/> ignora «archivos».
    /// </summary>
    private static byte[] EscribirManifiesto(ManifiestoContenido manifiesto, List<ArchivoPlano> archivos)
    {
        var json = JsonNode.Parse(JsonDelContrato.Escribir(manifiesto))!.AsObject();
        var lista = new JsonArray();
        foreach (var archivo in archivos)
        {
            lista.Add(new JsonObject
            {
                ["ruta"] = archivo.Ruta,
                ["size"] = archivo.Bytes.LongLength,
                ["sha256"] = CarpetaDelCanal.Sha256(archivo.Bytes)
            });
        }
        json["archivos"] = lista;
        return Encoding.UTF8.GetBytes(json.ToJsonString(JsonDelContrato.Opciones) + "\n");
    }

    private static byte[] ArmarZip(byte[] manifiesto, List<ArchivoPlano> archivos)
    {
        using var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            Agregar(zip, ManifiestoContenido.NombreDeArchivo, manifiesto);
            foreach (var archivo in archivos) Agregar(zip, archivo.Ruta, archivo.Bytes);
        }
        return memoria.ToArray();

        static void Agregar(ZipArchive zip, string ruta, byte[] bytes)
        {
            var entrada = zip.CreateEntry(ruta, CompressionLevel.Optimal);
            entrada.LastWriteTime = FechaFija;
            using var flujo = entrada.Open();
            flujo.Write(bytes);
        }
    }

    /// <summary>
    /// P7 — La identidad de un paquete no se reutiliza: si ya hay un contenido-X.Y.Z.zip, tiene que ser idéntico.
    /// P2 — Anti-rollback: una versión nueva tiene que ser estrictamente mayor que todas las publicadas.
    /// </summary>
    private static void ComprobarIdentidadYMonotonia(CarpetaDelCanal carpeta, VersionSemantica version, string destino, byte[] zip)
    {
        if (File.Exists(destino))
        {
            if (CarpetaDelCanal.Sha256DeArchivo(destino) == CarpetaDelCanal.Sha256(zip)) return;
            throw new PublicacionRechazada($"P7: ya hay un paquete {version.SinBuild()} distinto en el canal. La identidad no se reutiliza: publicá una versión mayor.");
        }

        var mayor = Indexador.LeerPaquetes(carpeta).Select(p => p.Manifiesto.Version).DefaultIfEmpty().Max();
        if (mayor is not null && version <= mayor)
            throw new PublicacionRechazada($"P2: {version} no es mayor que {mayor}, la última publicada en {carpeta.Canal}/puente-{carpeta.PuenteMayor}. El canal sólo avanza.");
    }
}
