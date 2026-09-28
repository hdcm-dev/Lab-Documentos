using Scalar.AspNetCore;

// LaEstancia.WebAPI — punto de entrada (composition root) de la API.
// Este archivo es lo único que existe todavía: acá se registran los servicios
// y se arma el pipeline HTTP. Las capas (Domain, Application, Infrastructure)
// se agregan como proyectos hermanos bajo src/Backend/ (ver §8.1 de la guía).

var builder = WebApplication.CreateBuilder(args);

// Documento OpenAPI: lo genera el propio ASP.NET Core a partir de los endpoints.
// Es el contrato que después consume cualquier cliente, sea .NET o no (§8.5).
builder.Services.AddOpenApi();

// --- Registro de las capas ---------------------------------------------------
// builder.Services.AddApplication();
// builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // /openapi/v1.json — el documento crudo.
    app.MapOpenApi();

    // /scalar — la interfaz de lectura y prueba sobre ese mismo documento.
    app.MapScalarApiReference(options => options.WithTitle("LaEstancia — API"));
}

app.UseHttpsRedirection();

// --- Endpoints ---------------------------------------------------------------
// app.MapProductosEndpoints();

app.Run();
