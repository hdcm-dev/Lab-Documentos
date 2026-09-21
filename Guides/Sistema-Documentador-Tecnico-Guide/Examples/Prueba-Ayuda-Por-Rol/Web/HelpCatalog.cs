using System.Reflection;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AyudaDemo.Web;

public sealed class FrontMatter
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Roles { get; set; } = [];
    public int Order { get; set; }
    public List<string> Traces { get; set; } = [];
    public string? Topic { get; set; }
    public string? TranslationOf { get; set; }
}

public sealed record HelpPage(string Culture, string Slug, FrontMatter Meta, string Source);

/// <summary>Resultado de pedir una página: la que se sirve y si es respaldo del idioma fuente.</summary>
public sealed record HelpResult(HelpPage Page, FrontMatter Access, bool IsFallback);

/// <summary>
/// Catálogo de la ayuda. Se arma una vez al arrancar desde los recursos incrustados
/// <c>Ayuda/{cultura}/{slug}.md</c> y es inmutable: el contenido cambia sólo con un despliegue.
/// El acceso lo decide SIEMPRE la página del idioma fuente: una traducción no puede ampliarlo.
/// </summary>
public sealed class HelpCatalog
{
    public const string SourceCulture = "es";

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions().UseYamlFrontMatter().DisableHtml().Build();

    private static readonly IDeserializer Yaml = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

    private readonly Dictionary<(string Culture, string Slug), HelpPage> _pages = [];

    public HelpCatalog()
    {
        var asm = Assembly.GetExecutingAssembly();
        foreach (var name in asm.GetManifestResourceNames().Where(n => n.StartsWith("Ayuda/") && n.EndsWith(".md")))
        {
            using var reader = new StreamReader(asm.GetManifestResourceStream(name)!);
            var source = reader.ReadToEnd();
            var doc = Markdown.Parse(source, Pipeline);
            var block = doc.Descendants<YamlFrontMatterBlock>().FirstOrDefault()
                ?? throw new InvalidOperationException($"{name}: falta el frontmatter");
            var yaml = string.Join('\n', source.Substring(block.Span.Start, block.Span.Length).Split('\n').Where(l => l.TrimEnd() != "---"));
            var meta = Yaml.Deserialize<FrontMatter>(yaml);
            var rest = name["Ayuda/".Length..^".md".Length];
            var culture = rest[..rest.IndexOf('/')];
            _pages[(culture, rest[(culture.Length + 1)..])] = new HelpPage(culture, rest[(culture.Length + 1)..], meta, source);
        }
    }

    public int Count => _pages.Count;

    public HelpResult? Get(string slug, string role, string culture)
    {
        if (!_pages.TryGetValue((SourceCulture, slug), out var source) || !source.Meta.Roles.Contains(role))
            return null;                                   // no existe para este rol: 404
        if (culture != SourceCulture && _pages.TryGetValue((culture, slug), out var translated))
            return new HelpResult(translated, source.Meta, false);
        return new HelpResult(source, source.Meta, culture != SourceCulture);
    }

    public IReadOnlyList<HelpResult> Navigation(string role, string culture) =>
        _pages.Values.Where(p => p.Culture == SourceCulture)
            .OrderBy(p => p.Meta.Order).ThenBy(p => p.Slug)
            .Select(p => Get(p.Slug, role, culture)).OfType<HelpResult>().ToList();

    public HelpResult? ByTopic(string topic, string role, string culture) =>
        _pages.Values.Where(p => p.Culture == SourceCulture && p.Meta.Topic == topic)
            .Select(p => Get(p.Slug, role, culture)).OfType<HelpResult>().FirstOrDefault();

    /// <summary>Renderiza a HTML reescribiendo los enlaces <c>../x/y.md</c> a rutas del panel.</summary>
    public static string ToHtml(HelpPage page)
    {
        var doc = Markdown.Parse(page.Source, Pipeline);
        foreach (var link in doc.Descendants<LinkInline>())
            if (Resolve(page.Slug, link.Url) is { } target)
                link.Url = "/ayuda/" + target;
        using var writer = new StringWriter();
        var renderer = new Markdig.Renderers.HtmlRenderer(writer);
        Pipeline.Setup(renderer);
        renderer.Render(doc);
        return writer.ToString();
    }

    private static string? Resolve(string fromSlug, string? url)
    {
        if (url is null || !url.EndsWith(".md") || url.Contains("://")) return null;
        var dir = Path.GetDirectoryName(fromSlug) ?? "";
        return Path.GetFullPath(Path.Combine("/" + dir, url))[1..^".md".Length];
    }

    /// <summary>Lo que CI corre antes de publicar: enlaces rotos, cruces de rol y traducciones huérfanas.</summary>
    public IReadOnlyList<string> Validate()
    {
        var failures = new List<string>();
        foreach (var page in _pages.Values)
        {
            if (page.Culture != SourceCulture && !_pages.ContainsKey((SourceCulture, page.Slug)))
                failures.Add($"{page.Culture}/{page.Slug}: traducción sin página fuente");
            if (page.Culture == SourceCulture && page.Meta.Roles.Count == 0)
                failures.Add($"{page.Slug}: sin roles");
            var readers = _pages.TryGetValue((SourceCulture, page.Slug), out var src) ? src.Meta.Roles : [];
            foreach (var link in Markdown.Parse(page.Source, Pipeline).Descendants<LinkInline>())
            {
                if (Resolve(page.Slug, link.Url) is not { } target) continue;
                if (!_pages.TryGetValue((SourceCulture, target), out var t))
                    failures.Add($"{page.Culture}/{page.Slug} → {link.Url}: enlace roto");
                else if (readers.Except(t.Meta.Roles).ToList() is { Count: > 0 } orphans)
                    failures.Add($"{page.Culture}/{page.Slug} → {target}: {string.Join(",", orphans)} no puede verla");
            }
        }
        return failures;
    }
}
