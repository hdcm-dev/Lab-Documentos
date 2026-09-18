using MyProject.Application.Productos.Commands.CrearProducto;
using MyProject.Application.Productos.Queries.ObtenerProductoPorId;
using MyProject.Application.Productos.Queries.ObtenerProductos;
using MyProject.Infrastructure;
using MyProject.WebAPI.ExceptionHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// Composition root: cambió la implementación del repositorio; Domain y Application no se tocaron.
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("MyProject") ?? "Data Source=myproject.db");
builder.Services.AddScoped<CrearProductoHandler>();
builder.Services.AddScoped<ObtenerProductosHandler>();
builder.Services.AddScoped<ObtenerProductoPorIdHandler>();

var app = builder.Build();

app.Services.EnsureDatabaseCreated();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();
