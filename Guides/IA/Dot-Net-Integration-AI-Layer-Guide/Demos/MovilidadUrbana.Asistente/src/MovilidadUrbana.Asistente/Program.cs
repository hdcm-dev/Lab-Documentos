// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Program.cs @ 8fb4c3a
// Delta: Agregado por la IA — AddAI(config), PrepararAIAsync, AddRazorComponents().AddInteractiveServerComponents(), UseAntiforgery, MapStaticAssets, MapRazorComponents<App>() (ADR-13), AddLimitadorDelModelo y UseRateLimiter (límite de concurrencia del modelo en la entrada, REC-13); Modificado a causa de la IA — SessionHeaderMiddleware sólo en /api (app.UseWhen) y SessionMiddleware sólo en los documentos (§F.5); Modificado por otro motivo — UseStatusCodePages acotado a /api y página de no encontrado de Blazor fuera de /api (ADR-13); namespace raíz MovilidadUrbana.Asistente (H2-008)
using System.Globalization;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Components;
using MovilidadUrbana.Asistente.Concurrencia;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Sessions;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Los textos de validación y los formatos del resumen son los mismos que en la web.
var cultura = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

// --- Infraestructura y aplicación: las mismas capas que la web ---------------------------------
var connectionString = builder.Configuration.GetConnectionString("BaseDeDatos")
    ?? MovilidadUrbana.Asistente.Infrastructure.DependencyInjection.DefaultConnectionString;
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();

// --- Capa de IA (plan §F.3) ---------------------------------------------------------------------
// Las variables canónicas (§E.1) se leen de la configuración, que incluye el entorno: así las
// pruebas con WebApplicationFactory las fijan con UseSetting sin tocar el proceso.
string? Entorno(string nombre) => builder.Configuration[nombre];
ConfiguracionDelModelo config;
try
{
    config = ConfiguracionDelModelo.DesdeEntorno(Entorno).Validar();
}
catch (ConfiguracionInvalidaException e)
{
    Console.Error.WriteLine($"Configuración rechazada: {e.Message}");
    return 1;
}
builder.Services.AddAI(config, Entorno);

// Un solo límite de pedidos simultáneos al modelo, en la entrada: lo comparten los controllers que
// llaman al modelo (503 con Retry-After) y la página de chat. Sección «Concurrencia» de la configuración.
builder.Services.AddLimitadorDelModelo(builder.Configuration);

// --- Presentation: REST y la página de chat (un host, ADR-13) ----------------------------------
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();

DatabaseInitializer.Initialize(app.Services);
await app.Services.PrepararAIAsync(Entorno);

static bool EsApi(HttpContext c) => c.Request.Path.StartsWithSegments("/api");

app.UseExceptionHandler();
// Las páginas de estado de la API no pisan las de Blazor, y al revés (ADR-13). El re-execute va en
// el pipeline principal, como en la web del caso base: dentro de una rama de UseWhen no se vuelve
// a enrutar y /inexistente quedaría en un 404 vacío (lo detectó Host_Humo_EstilosYEstados). En /api,
// el UseStatusCodePages interno escribe primero el problem+json y el externo ya no actúa.
app.UseStatusCodePagesWithReExecute("/no-encontrado", createScopeForStatusCodePages: true);
app.UseWhen(EsApi, api => api.UseStatusCodePages());

if (app.Environment.IsDevelopment())
{
    // El contrato en /openapi/v1.json y su documentación navegable en /scalar/v1, donde cada
    // ruta se puede probar desde el navegador. El encabezado de sesión se declara ahí mismo para
    // que quien lo repita en cada pedido vea sus propios datos.
    app.MapOpenApi();
    app.MapScalarApiReference(opciones => opciones
        .WithTitle("Movilidad Urbana — API")
        .WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl));
}

// La sesión por encabezado sólo en /api; por cookie, sólo en los documentos de la página (§F.5).
app.UseWhen(EsApi, api => api.UseMiddleware<SessionHeaderMiddleware>());
app.UseWhen(c => !EsApi(c), web => web.UseMiddleware<SessionMiddleware>());
app.UseAntiforgery();
app.UseRateLimiter();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
return 0;

/// <summary>Punto de entrada visible para las pruebas con <c>WebApplicationFactory</c>.</summary>
public partial class Program;
