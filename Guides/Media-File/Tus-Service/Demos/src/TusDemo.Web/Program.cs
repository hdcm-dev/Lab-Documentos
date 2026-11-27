using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using tusdotnet;
using tusdotnet.Stores;
using TusDemo.Web.Archivos;
using TusDemo.Web.Components;
using TusDemo.Web.Seguridad;

var builder = WebApplication.CreateBuilder(args);

// --- Configuración y servicios de archivos ---
builder.Services.Configure<OpcionesArchivos>(builder.Configuration.GetSection(OpcionesArchivos.Seccion));
builder.Services.AddSingleton(sp =>
{
    var raiz = sp.GetRequiredService<IOptions<OpcionesArchivos>>().Value.Raiz;
    Directory.CreateDirectory(raiz);
    return new TusDiskStore(raiz);          // una sola instancia para tus y para las descargas
});
builder.Services.AddSingleton<RegistroDeArchivos>();
builder.Services.AddSingleton<AvisoDeArchivos>();
builder.Services.AddSingleton<ServicioDeArchivos>();

// --- Autenticación de la demo (cabecera X-Clave-Demo) ---
builder.Services.AddAuthentication(ClaveDemoHandler.Esquema)
    .AddScheme<AuthenticationSchemeOptions, ClaveDemoHandler>(ClaveDemoHandler.Esquema, null);
builder.Services.AddAuthorization();

// --- API con controllers y documento OpenAPI ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- Interfaz Blazor Interactive Server ---
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapOpenApi();                    // /openapi/v1.json
app.MapScalarApiReference();         // interfaz de Scalar sobre ese documento
app.MapControllers();                // /api/archivos y /descargas
app.MapTus("/archivos", ConfiguracionTus.CrearAsync);   // endpoint del protocolo tus
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

/// <summary>Visible para pruebas de integración.</summary>
public partial class Program;
