using System.Reflection;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseYamlFrontMatter().DisableHtml().Build();
var yaml = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
var asm = Assembly.GetExecutingAssembly();

// Catálogo: una entrada por recurso incrustado Ayuda/**.md
var pages = new Dictionary<string, (FrontMatter Meta, MarkdownDocument Doc, string Source)>();
foreach (var name in asm.GetManifestResourceNames().Where(n => n.EndsWith(".md")))
{
    using var r = new StreamReader(asm.GetManifestResourceStream(name)!);
    var src = r.ReadToEnd();
    var doc = Markdown.Parse(src, pipeline);
    var fm = doc.Descendants<YamlFrontMatterBlock>().First();
    var meta = yaml.Deserialize<FrontMatter>(src.Substring(fm.Span.Start, fm.Span.Length).Trim('-', '\n', '\r'));
    var slug = name["Ayuda/".Length..^3];
    pages[slug] = (meta, doc, src);
}
Console.WriteLine($"Catálogo: {pages.Count} páginas → {string.Join(", ", pages.Keys.Order())}");

// Acceso por rol: si no alcanza, la página NO EXISTE (404), igual que groups en Mintlify con authentication
string? Render(string slug, string role) =>
    pages.TryGetValue(slug, out var p) && p.Meta.Roles.Contains(role) ? p.Doc.ToHtml(pipeline) : null;

foreach (var role in new[] { "Student", "Administrator" })
{
    Console.WriteLine($"\n== Rol {role}: navegación");
    foreach (var (slug, p) in pages.Where(x => x.Value.Meta.Roles.Contains(role)).OrderBy(x => x.Value.Meta.Order))
        Console.WriteLine($"   {p.Meta.Order,3}  /ayuda/{slug}  «{p.Meta.Title}»");
    Console.WriteLine($"   GET administrador/habilitar-cuentas → {(Render("administrador/habilitar-cuentas", role) is null ? "404" : "200")}");
    Console.WriteLine($"-- llms.txt ({role})");
    Console.WriteLine($"# Ayuda de Fábrica de Geometría ({role})");
    foreach (var (slug, p) in pages.Where(x => x.Value.Meta.Roles.Contains(role)).OrderBy(x => x.Value.Meta.Order))
        Console.WriteLine($"- [{p.Meta.Title}](/ayuda/{slug}.md): {p.Meta.Description}");
}

Console.WriteLine("\n== HTML de comun/ingresar (Student) — ¿se escapa el <script>?");
Console.WriteLine(Render("comun/ingresar", "Student"));
Console.WriteLine("== HTML de la tabla (Administrator)");
Console.WriteLine(Render("administrador/habilitar-cuentas", "Administrator"));

// Validador de CI: enlaces rotos y enlaces que cruzan hacia una página que el lector no puede ver
int fallas = 0;
foreach (var (slug, p) in pages)
foreach (var link in p.Doc.Descendants<LinkInline>().Where(l => l.Url is { } u && u.EndsWith(".md")))
{
    var target = Path.GetFullPath(Path.Combine("/" + Path.GetDirectoryName(slug)!, link.Url!))[1..^3];
    if (!pages.TryGetValue(target, out var t)) { Console.WriteLine($"FALLA enlace roto: {slug} → {link.Url}"); fallas++; continue; }
    var huerfanos = p.Meta.Roles.Except(t.Meta.Roles).ToList();
    if (huerfanos.Count > 0) { Console.WriteLine($"FALLA cruce de rol: {slug} → {target}; {string.Join(",", huerfanos)} no puede verla"); fallas++; }
}
Console.WriteLine($"\nValidador: {fallas} fallas");
return fallas == 0 ? 0 : 1;

class FrontMatter { public string Title { get; set; } = ""; public string Description { get; set; } = ""; public List<string> Roles { get; set; } = []; public int Order { get; set; } }
