using System.Globalization;
using FrameworkConcept.Content;
using FrameworkConcept.Content.Auth;
using FrameworkConcept.Content.Bridge;
using FrameworkConcept.Content.Red;
using FrameworkConcept.Core.Application.Abstractions;
using FrameworkConcept.Core.Application.Localidades;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

// Punto de entrada del contenido. Corre dentro del HybridWebView del cascarón, como Blazor WebAssembly.

// La cultura de la demo es es-AR, como el caso base: números y fechas se muestran así en todas las pantallas.
var cultura = CultureInfo.GetCultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// El puente: un cliente (arma los sobres) y una sesión (handshake, capacidades, eventos). En WebAssembly
// hay un solo usuario por página, así que «scoped» equivale a una instancia por arranque del contenido.
builder.Services.AddScoped<BridgeClient>();
builder.Services.AddScoped<BridgeSession>();

// Folio 038 — Estado de red y política de acciones (Red/), sesión del usuario (Auth/). Los dos escuchan los eventos
// del cascarón por BridgeSession.EventoRecibido y nunca ven tokens (P3).
builder.Services.AddScoped<EstadoDeRedDelContenido>();
builder.Services.AddScoped<SesionDelContenido>();

// Casos de uso del caso base, con su repositorio ahora sobre el puente (P5: documentos JSON del cascarón).
// La encuesta no usa EncuestaService: la valida DefinicionValidator, guiado por la definición (P1).
builder.Services.AddScoped<ILocalidadRepository, LocalidadBridgeRepository>();
builder.Services.AddScoped<LocalidadService>();

await builder.Build().RunAsync();
