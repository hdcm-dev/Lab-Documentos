using System.Globalization;
using FrameworkConcept.Core.Update;
using FrameworkConcept.Receptor;

// «Receptor didáctico» (P11): un doble de prueba del lado servidor de la demo. Hace dos cosas:
//   1. sirve el canal estático /{canal}/puente-{mayor}/… desde una carpeta (CanalEstatico.cs);
//   2. recibe las respuestas del outbox en POST /respuestas y las muestra en GET /respuestas
//      (RecepcionDeRespuestas.cs).
// NO es un servidor de producción: un archivo como base. HTTPS con la CA de demostración («guion.sh tls»);
// autenticación opcional con un emisor OIDC de ensayo (--oidc-emisor, folio 038).
//
// Uso (desde la raíz de la demo):
//   dotnet tools/FrameworkConcept.Receptor/bin/Release/net10.0/FrameworkConcept.Receptor.dll \
//     --urls http://0.0.0.0:8099 --canal salida/canal --datos salida/receptor [--ahora 2026-12-01T00:00:00Z]
//
// --ahora fija el reloj del receptor: sirve para ver qué pasa con una respuesta DESPUÉS de la fecha de retiro
// de su definición, sin esperar los 30 días de aviso (P13.5).

var builder = WebApplication.CreateBuilder(args);
var raizDelCanal = builder.Configuration["canal"] ?? "salida/canal";
var carpetaDeDatos = builder.Configuration["datos"] ?? "salida/receptor";
IReloj reloj = builder.Configuration["ahora"] is { } ahora
    ? new RelojFijo(DateTimeOffset.Parse(ahora, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal))
    : new RelojDelSistema();

// Folio 038 — Con --oidc-emisor, las respuestas exigen un access token válido del emisor (AutenticacionDelReceptor.cs).
var conAutenticacion = AutenticacionDelReceptor.Configurar(builder);

var app = builder.Build();
AutenticacionDelReceptor.Usar(app, conAutenticacion);

app.MapGet("/salud", () => Results.Ok(new { estado = "ok", canal = Path.GetFullPath(raizDelCanal), ahora = reloj.Ahora }));
CanalEstatico.Mapear(app, raizDelCanal);
RecepcionDeRespuestas.Mapear(app, new AlmacenDeRespuestas(carpetaDeDatos), new RegistroDeDefiniciones(raizDelCanal, reloj), reloj, conAutenticacion);

app.Logger.LogInformation("receptor didáctico: canal en {Canal}, respuestas en {Datos}, reloj {Ahora:yyyy-MM-dd HH:mm}Z",
    Path.GetFullPath(raizDelCanal), Path.GetFullPath(carpetaDeDatos), reloj.Ahora);
app.Run();

internal sealed class RelojDelSistema : IReloj
{
    public DateTimeOffset Ahora => DateTimeOffset.UtcNow;
}

internal sealed class RelojFijo(DateTimeOffset ahora) : IReloj
{
    public DateTimeOffset Ahora { get; } = ahora;
}

// Hace visible el tipo Program para ILogger<Program>.
public partial class Program;
