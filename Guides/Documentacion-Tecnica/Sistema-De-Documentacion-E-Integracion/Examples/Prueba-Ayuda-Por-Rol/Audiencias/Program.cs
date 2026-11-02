using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using Audiencias;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// Prueba extendida de ayuda por rol (resolución 399 del expediente 0003, §5.2.3, E-1 a E-6). Contenido ficticio.
// Cada par corre la mitad verde (como está) y la roja (con un defecto plantado en memoria, nunca en el repositorio).
// Sale con 0 si cada control da verde sobre lo verde y rojo sobre lo plantado.
var cat = Catalogo.Cargar();
int pares = 0, bien = 0;
void Par(string id, string control, List<string> verde, List<string> rojo, string plantado)
{
    pares++;
    var ok = verde.Count == 0 && rojo.Count > 0;
    if (ok) bien++;
    Console.WriteLine($"-- {id} {control}");
    Console.WriteLine($"   verde (tal cual): {verde.Count} fallas");
    foreach (var f in verde) Console.WriteLine($"      {f}");
    Console.WriteLine($"   rojo  ({plantado}): {rojo.Count} fallas");
    foreach (var f in rojo) Console.WriteLine($"      {f}");
    Console.WriteLine($"   par: {(ok ? "OK" : "NO")}");
}

Console.WriteLine($"Registro: {cat.Audiencias.Count} audiencias, repositorio «{cat.Visibilidad}»");
foreach (var a in cat.Audiencias.Values)
    Console.WriteLine($"   {a.Id,-11} rol={a.Rol,-10} alcance={a.Alcance,-14} superficie={a.Superficie,-14} nivel={a.Nivel,-11} expande={string.Join(",", cat.Expandir(a.Id).Order())}");
Console.WriteLine($"Catálogo: {cat.Paginas.Count} páginas");
foreach (var p in cat.Paginas.Values.OrderBy(p => p.Meta.Order))
    Console.WriteLine($"   {p.Slug,-26} delivery=[{string.Join(",", p.Meta.Delivery)}] nivel={cat.NivelDe(p)} reciben={string.Join(",", cat.Receptores(p).Order())}");

// E-1: jerarquía y cruce de audiencia.
Console.WriteLine("\n== E-1 Tres roles en jerarquía y el integrador");
var e1 = cat.Copia();
e1.Agregar("comun/plantada-sube", "---\ntitle: Plantada\ndescription: Defecto plantado.\nroles: [User]\ndelivery: [usuario]\n---\n# Plantada\n\nVer [alta](../raiz/alta-de-aplicacion.md).\n");
Par("E-1", "cruce-de-audiencia", Controles.CruceDeAudiencia(cat), Controles.CruceDeAudiencia(e1),
    "plantada: comun/plantada-sube, de usuario, enlaza a raiz/alta-de-aplicacion");

// E-2: dos alcances resueltos en el servidor; prueba pareada del producto.
Console.WriteLine("\n== E-2 Dos alcances (app-a, app-b) resueltos en el servidor");
async Task<List<string>> PruebaAlcance(Opciones op, bool imprimir)
{
    await using var app = await Servidor.Levantar(cat, op);
    var fallas = new List<string>();
    var casos = new (string Rol, string App, string Slug, bool Recibe, string Marca)[]
    {
        ("AppAdmin", "app-a", "app-a/configurar-avisos", true, "aplicación A"),
        ("AppAdmin", "app-a", "app-b/configurar-avisos", false, "aplicación B"),
        ("RootAdmin", "", "app-b/configurar-avisos", true, "aplicación B"),
        ("User", "app-a", "app-a/configurar-avisos", false, "aplicación A"),
        ("User", "app-a", "comun/primeros-pasos", true, "Primeros pasos"),
    };
    foreach (var c in casos)
    {
        var (cod, cuerpo) = await Servidor.Get(app, "/ayuda/" + c.Slug, c.Rol, c.App);
        var conContenido = cuerpo.Contains(c.Marca);
        if (imprimir) Console.WriteLine($"   {c.Rol,-9} {c.App,-5} GET /ayuda/{c.Slug,-24} → {cod}, contenido={(conContenido ? "sí" : "no")}");
        if (c.Recibe != (cod == 200 && conContenido) || (!c.Recibe && conContenido))
            fallas.Add($"entrega: {c.Rol} de «{c.App}» {(c.Recibe ? "no recibe" : "recibe")} {c.Slug} ({cod})");
    }
    return fallas;
}
Par("E-2", "prueba pareada de alcance", await PruebaAlcance(new(), true), await PruebaAlcance(new(IgnorarAlcance: true), false),
    "servidor que ignora el alcance");

// E-3: paquete con sólo lo público, en texto y en recursos compilados.
Console.WriteLine("\n== E-3 Dos superficies: panel y paquete");
var clavesNoPublicas = new Dictionary<string, string> { ["Integracion.Credencial"] = "restringido" };
List<string> Paquete(bool plantar)
{
    var dir = Directory.CreateTempSubdirectory("paquete-").FullName;
    try
    {
        foreach (var p in cat.Paginas.Values.Where(p => cat.NivelDe(p) == "publico"))
        {
            var f = Path.Combine(dir, "ayuda", p.Slug + ".md");
            Directory.CreateDirectory(Path.GetDirectoryName(f)!);
            File.WriteAllText(f, p.Fuente);
        }
        using (var w = new ResourceWriter(Path.Combine(dir, "Microtexto.resources")))
        {
            foreach (var e in Microtexto.Rm.GetResourceSet(System.Globalization.CultureInfo.InvariantCulture, true, false)!
                         .Cast<System.Collections.DictionaryEntry>().OrderBy(e => (string)e.Key))
                w.AddResource((string)e.Key, (string)e.Value!);
            if (plantar) w.AddResource("Integracion.Credencial", "FICTICIO-RESTRINGIDO: credencial de integrador de ejemplo");
        }
        if (!plantar)
            foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Order())
                Console.WriteLine($"   paquete/{Path.GetRelativePath(dir, f).Replace('\\', '/')}");
        var prohibidas = cat.Paginas.Values.Where(p => cat.NivelDe(p) != "publico").Select(p => p.Meta.Title).Concat(clavesNoPublicas.Keys);
        return Controles.PaqueteSinFuga(dir, prohibidas);
    }
    finally { Directory.Delete(dir, true); }
}
Par("E-3", "paquete-sin-fuga", Paquete(false), Paquete(true), "clave «restringido» plantada en Microtexto.resources");

// E-4: dos documentos OpenAPI; el de integradores detrás de autorización.
Console.WriteLine("\n== E-4 Documentación de API por exposición");
async Task<List<string>> Contrato(Opciones op, bool imprimir)
{
    await using var app = await Servidor.Levantar(cat, op);
    foreach (var doc in new[] { "publico", "integradores" })
    {
        var (cod, cuerpo) = await Servidor.Get(app, $"/openapi/{doc}.json", doc == "integradores" ? "Integrator" : null);
        var rutas = Regex.Matches(cuerpo, "\"(/api/[^\"]+)\"").Select(m => m.Groups[1].Value).Distinct().Order();
        Console.WriteLine($"   {(imprimir ? "verde" : "rojo ")} {doc,-12} → {cod}, rutas: {string.Join(", ", rutas)}");
    }
    return Servidor.SinGrupo(app);
}
Par("E-4a", "contrato-por-exposicion", await Contrato(new(), true), await Contrato(new(EndpointSinGrupo: true), false),
    "punto final /api/diagnostico sin grupo");
async Task<List<string>> Pareada(Opciones op, bool imprimir)
{
    await using var app = await Servidor.Levantar(cat, op);
    var fallas = new List<string>();
    foreach (var (rol, autorizado) in new[] { ("Integrator", true), ((string?)null, false), ("User", false) })
    {
        var (cod, cuerpo) = await Servidor.Get(app, "/openapi/integradores.json", rol);
        var con = cuerpo.Contains("/api/integracion/eventos");
        if (imprimir) Console.WriteLine($"   {rol ?? "sin sesión",-10} GET /openapi/integradores.json → {cod}, contenido={(con ? "sí" : "no")}");
        if (autorizado != (cod == 200 && con) || (!autorizado && con)) fallas.Add($"entrega: {rol ?? "sin sesión"} → {cod}, contenido={(con ? "sí" : "no")}");
    }
    return fallas;
}
Par("E-4b", "prueba pareada 200 / distinto de 200", await Pareada(new(), true), await Pareada(new(OpenApiAbierto: true), false),
    "documento de integradores sin autorización");

// E-5: formulario con [Display], regla con límite, pista y error interpolados, traza desde la ficha.
Console.WriteLine("\n== E-5 Formulario: ayuda de campo y validación trazadas a RN-DEMO-ASUNTO");
var ficha = Catalogo.Recurso("Reglas/RN-DEMO-ASUNTO.md");
var version = Regex.Match(ficha, @"\*\*Versión:\*\* (\S+)").Groups[1].Value;
var limiteFicha = int.Parse(Regex.Match(ficha, @"\| Límite \| (\d+) \|").Groups[1].Value);
var anclas = Regex.Matches(ficha, @"^\| (valida|muestra) \| `([^`]+)` → `([^`]+)` \| (\d+) \|$", RegexOptions.Multiline)
    .Select(m => (Rol: m.Groups[1].Value, Destino: m.Groups[3].Value, Contiene: m.Groups[4].Value)).ToList();
var prop = typeof(AvisoForm).GetProperty(nameof(AvisoForm.Asunto))!;
var display = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>()!;
var largo = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.StringLengthAttribute>()!;
Console.WriteLine($"   ficha RN-DEMO-ASUNTO@{version}: límite {limiteFicha}, {anclas.Count} anclas");
Console.WriteLine($"   [Display] Name=«{display.Name}» Prompt=«{display.Prompt}» Description=«{display.Description}»");
Console.WriteLine($"   pista:      «{Microtexto.Texto("Asunto.Pista", ReglaAsunto.Maximo)}»");
Console.WriteLine($"   validacion: «{Microtexto.Texto("Asunto.Validacion", ReglaAsunto.Maximo)}»");
List<string> Anclas(int limite, int atributo, Func<string, string> texto) =>
    anclas.Select(a => (a, valor: a.Rol == "valida" ? atributo.ToString() : texto(a.Destino)))
        .Where(x => !x.valor.Contains(limite.ToString()))
        .Select(x => $"anclas: {x.a.Rol} {x.a.Destino} no contiene {limite} («{x.valor}»)").ToList();
Console.WriteLine($"   tal cual (límite {ReglaAsunto.Maximo}): {Anclas(limiteFicha, largo.MaximumLength, k => Microtexto.Texto(k, ReglaAsunto.Maximo)).Count} fallas");
// La regla pasa a 100 (ficha y atributo); el texto interpolado la sigue, el copiado a mano no.
const int nuevo = 100;
string Copiado(string k) => Microtexto.Rm.GetString(k)!.Replace("{0}", limiteFicha.ToString());
Par("E-5a", "anclas (muestra, valida, contiene)", Anclas(nuevo, nuevo, k => Microtexto.Texto(k, nuevo)), Anclas(nuevo, nuevo, Copiado),
    $"la regla pasa a {nuevo} y el texto quedó copiado con {limiteFicha}");

var servicios = new ServiceCollection().AddLogging().BuildServiceProvider();
async Task<string> Render(bool con)
{
    await using var h = new HtmlRenderer(servicios, servicios.GetRequiredService<ILoggerFactory>());
    return await h.Dispatcher.InvokeAsync(async () =>
        (await h.RenderComponentAsync<CampoAsunto>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["ConDescribedBy"] = con }))).ToHtmlString());
}
List<string> Aria(string html)
{
    var input = Regex.Match(html, "<input[^>]*>").Value;
    var ids = Regex.Match(input, "aria-describedby=\"([^\"]*)\"").Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var fallas = new List<string>();
    if (ids.Length == 0) fallas.Add("aria-describedby: el campo no la tiene");
    foreach (var id in ids) if (!html.Contains($"id=\"{id}\"")) fallas.Add($"aria-describedby: «{id}» no existe");
    if (!ids.Contains("asunto-error")) fallas.Add("aria-describedby: no apunta al error");
    return fallas;
}
var sinAria = await Render(false);
var conAria = await Render(true);
var mensaje = Regex.Match(sinAria, "<div class=\"validation-message\"[^>]*>").Value;
Console.WriteLine($"   medido: InputText sin atributo → {Regex.Match(sinAria, "<input[^>]*>").Value}");
Console.WriteLine($"   medido: ValidationMessage → {mensaje} (id propio: {(mensaje.Contains("id=") ? "sí" : "no")})");
Console.WriteLine($"   con el atributo → {Regex.Match(conAria, "<input[^>]*>").Value}");
Par("E-5b", "aria-describedby medido", Aria(conAria), Aria(sinAria), "campo sin aria-describedby");

// E-6: declaración de repo público y una página «interno» plantada.
Console.WriteLine("\n== E-6 Nivel contra visibilidad del repositorio");
var e6 = cat.Copia();
e6.Agregar("equipo/plantada-interna", "---\ntitle: Plantada\ndescription: Defecto plantado.\nroles: [User]\ndelivery: [equipo]\n---\n# Plantada\n");
Par("E-6", "nivel-vs-repo", Controles.NivelVsRepo(cat), Controles.NivelVsRepo(e6), "página de «equipo» (interno) plantada");

Console.WriteLine($"\nValidador: {bien}/{pares} pares con verde y rojo como se espera");
return bien == pares ? 0 : 1;
