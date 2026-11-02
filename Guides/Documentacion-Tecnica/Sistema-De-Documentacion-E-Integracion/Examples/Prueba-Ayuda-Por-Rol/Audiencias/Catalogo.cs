using System.Reflection;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Audiencias;

// Una fila del registro AUD-: las cuatro dimensiones y la jerarquía con «Incluye».
public record Audiencia(string Id, string Rol, string Alcance, string Superficie, string Nivel, List<string> Incluye);

public class Meta
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Roles { get; set; } = [];
    public List<string> Delivery { get; set; } = [];
    public string? Contexto { get; set; }
    public int Order { get; set; }
}

public record Pagina(string Slug, Meta Meta, string Fuente, MarkdownDocument Doc)
{
    // El espacio de nombres de «contexto: <app>/<id>» es el valor de alcance que el producto compara con la sesión.
    public string? App => Meta.Contexto?.Split('/')[0];
}

public class Catalogo
{
    public static readonly string[] Niveles = ["publico", "producto", "restringido", "interno"];
    static readonly MarkdownPipeline Pipe = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseYamlFrontMatter().DisableHtml().Build();
    static readonly IDeserializer Yaml = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

    public Dictionary<string, Audiencia> Audiencias { get; } = new();
    public Dictionary<string, Pagina> Paginas { get; } = new();
    public string Visibilidad { get; private set; } = "";

    public static string Recurso(string nombre)
    {
        using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(nombre) ?? throw new FileNotFoundException(nombre);
        return new StreamReader(s).ReadToEnd();
    }

    public static Catalogo Cargar()
    {
        var c = new Catalogo();
        var reg = Recurso("_audiencias.md");
        c.Visibilidad = Regex.Match(reg, @"Visibilidad del repositorio: `(\w+)`").Groups[1].Value;
        string[]? cab = null;
        foreach (var l in reg.Split('\n').Select(x => x.Trim()))
        {
            if (!l.StartsWith('|')) { cab = null; continue; }
            var celdas = l.Trim('|').Split('|').Select(x => x.Trim()).ToArray();
            if (cab is null) { cab = celdas; continue; }
            if (celdas.All(x => x.Trim('-', ':', ' ') == "") || cab[0] != "Audiencia") continue;
            var f = cab.Zip(celdas).ToDictionary(x => x.First, x => x.Second);
            c.Audiencias[f["Audiencia"]] = new(f["Audiencia"], f["Rol"], f["Alcance"], f["Superficie"], f["Nivel"],
                f["Incluye"].Split(',').Select(x => x.Trim()).Where(x => x is not ("—" or "")).ToList());
        }
        foreach (var n in Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(n => n.StartsWith("Ayuda/")))
            c.Agregar(n["Ayuda/".Length..^3], Recurso(n));
        return c;
    }

    public void Agregar(string slug, string fuente)
    {
        var doc = Markdown.Parse(fuente, Pipe);
        var fm = doc.Descendants<YamlFrontMatterBlock>().First();
        var meta = Yaml.Deserialize<Meta>(fuente.Substring(fm.Span.Start, fm.Span.Length).Trim('-', '\n', '\r'));
        Paginas[slug] = new(slug, meta, fuente, doc);
    }

    public Catalogo Copia()
    {
        var c = new Catalogo { Visibilidad = Visibilidad };
        foreach (var (k, v) in Audiencias) c.Audiencias[k] = v;
        foreach (var (k, v) in Paginas) c.Paginas[k] = v;
        return c;
    }

    // Jerarquía expandida: la audiencia y todas las que incluye, transitivamente.
    public HashSet<string> Expandir(string aud)
    {
        var r = new HashSet<string>();
        var pila = new Stack<string>([aud]);
        while (pila.TryPop(out var a))
            if (r.Add(a) && Audiencias.TryGetValue(a, out var x)) foreach (var i in x.Incluye) pila.Push(i);
        return r;
    }

    public Audiencia? DelRol(string? rol) => Audiencias.Values.FirstOrDefault(a => a.Rol == rol);

    // Quiénes reciben la página: toda audiencia cuya jerarquía expandida toca su «delivery».
    public HashSet<string> Receptores(Pagina p) => Audiencias.Keys.Where(a => Expandir(a).Overlaps(p.Meta.Delivery)).ToHashSet();

    // Nivel de la página: el más alto de las audiencias a las que se entrega.
    public string NivelDe(Pagina p) => p.Meta.Delivery.Select(a => Audiencias.TryGetValue(a, out var x) ? x.Nivel : "interno")
        .MaxBy(n => Array.IndexOf(Niveles, n)) ?? "interno";

    public IEnumerable<(string Origen, string Destino)> Enlaces()
    {
        foreach (var (slug, p) in Paginas)
        foreach (var l in p.Doc.Descendants<LinkInline>().Where(l => l.Url is { } u && u.EndsWith(".md")))
            yield return (slug, Path.GetFullPath(Path.Combine("/" + Path.GetDirectoryName(slug)!, l.Url!))[1..^3]);
    }

    public string Html(Pagina p) => p.Doc.ToHtml(Pipe);
}
