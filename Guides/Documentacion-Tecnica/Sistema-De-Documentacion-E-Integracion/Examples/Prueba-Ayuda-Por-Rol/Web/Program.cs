using System.Globalization;
using System.Security.Claims;
using System.Text;
using AyudaDemo.Web;
using AyudaDemo.Web.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
builder.Services.AddSingleton<HelpCatalog>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => { o.LoginPath = "/ingresar"; o.Cookie.Name = "demo-sesion"; });

var app = builder.Build();

var catalog = app.Services.GetRequiredService<HelpCatalog>();
var failures = catalog.Validate();
foreach (var f in failures) app.Logger.LogError("Validador: {Falla}", f);
app.Logger.LogInformation("Catálogo de ayuda: {Paginas} páginas, validador con {Fallas} fallas", catalog.Count, failures.Count);

string[] cultures = ["es", "en"];
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("es").AddSupportedCultures(cultures).AddSupportedUICultures(cultures));
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Fuente cruda y llms.txt: el MISMO filtro por rol que la página HTML.
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? "";
    var isRaw = path.StartsWith("/ayuda/") && path.EndsWith(".md");
    if (!isRaw && path != "/ayuda/llms.txt") { await next(); return; }
    var role = ctx.User.FindFirst("gf:role")?.Value;
    if (role is null) { ctx.Response.StatusCode = 401; return; }
    var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    ctx.Response.ContentType = (isRaw ? "text/markdown" : "text/plain") + "; charset=utf-8";
    if (isRaw)
    {
        var hit = catalog.Get(path["/ayuda/".Length..^".md".Length], role, culture);
        if (hit is null) { ctx.Response.StatusCode = 404; return; }
        await ctx.Response.WriteAsync(hit.Page.Source);
        return;
    }
    var sb = new StringBuilder($"# Ayuda de la Fábrica de Geometría — {role}\n\n");
    foreach (var r in catalog.Navigation(role, culture))
        sb.Append($"- [{r.Page.Meta.Title}](/ayuda/{r.Page.Slug}.md): {r.Page.Meta.Description}\n");
    await ctx.Response.WriteAsync(sb.ToString());
});

// Ingreso simulado: en Lab-Geometria el papel lo emite el servicio en el claim gf:role.
app.MapGet("/entrar/{role}", async (string role, HttpContext ctx) =>
{
    if (role is not ("Student" or "Administrator")) return Results.BadRequest();
    var identity = new ClaimsIdentity(
        [new Claim("gf:role", role), new Claim(ClaimTypes.Name, role == "Student" ? "alumno@demo" : "admin@demo")],
        CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, "gf:role");
    await ctx.SignInAsync(new ClaimsPrincipal(identity));
    return Results.LocalRedirect("/");
});
app.MapGet("/salir", async (HttpContext ctx) => { await ctx.SignOutAsync(); return Results.LocalRedirect("/ingresar"); });
app.MapGet("/cultura/{culture}", (string culture, string? volver, HttpContext ctx) =>
{
    if (!cultures.Contains(culture)) return Results.BadRequest();
    ctx.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
    return Results.LocalRedirect(volver is { Length: > 0 } && volver.StartsWith('/') && !volver.StartsWith("//") ? volver : "/");
});

app.MapRazorComponents<App>();
app.Run();
