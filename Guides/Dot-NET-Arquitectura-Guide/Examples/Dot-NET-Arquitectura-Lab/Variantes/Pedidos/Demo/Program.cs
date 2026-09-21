using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using MyProject.Application.Common;
using MyProject.Application.Pedidos.Commands.RegistrarPedido;
using MyProject.Application.Productos.Commands.CambiarPrecioProducto;
using MyProject.Domain.Common;
using MyProject.Domain.Pedidos;
using MyProject.Domain.Productos;
using MyProject.Infrastructure.Persistence;
using MyProject.Infrastructure.Persistence.Repositories;

// Composition root de la demostración: EF Core sobre SQLite en memoria; la base vive mientras la conexión esté abierta.
await using var conexion = new SqliteConnection("Data Source=:memory:");
await conexion.OpenAsync();
var contador = new ContadorDeConfirmaciones();                // observa cada SaveChanges del DbContext

var services = new ServiceCollection();
services.AddDbContext<AppDbContext>(o => o.UseSqlite(conexion).AddInterceptors(contador)); // Scoped: un DbContext por scope (por request en la API)
services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());              // Scoped: el mismo DbContext del scope, visto como Unit of Work
services.AddScoped<IProductoRepository, ProductoRepository>();                              // Scoped: comparte el DbContext del scope
services.AddScoped<IPedidoRepository, PedidoRepository>();                                  // Scoped: ídem
services.AddScoped<RegistrarPedidoHandler>();                                               // Scoped: vive lo que dura el request
services.AddScoped<CambiarPrecioProductoHandler>();                                         // Scoped: ídem
services.AddSingleton(TimeProvider.System);                                                 // Singleton: el reloj no tiene estado
await using var provider = services.BuildServiceProvider();

// Carga inicial: crea el esquema y guarda los productos del enunciado.
Guid yerba, mate, bombilla;
using (var scope = provider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    var productos = scope.ServiceProvider.GetRequiredService<IProductoRepository>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var y = Producto.Create("Yerba 1 kg", 4500m);
    var m = Producto.Create("Mate", 12000m);
    var b = Producto.Create("Bombilla");                       // llegó sin manifiesto de precios
    await productos.AddAsync(y); await productos.AddAsync(m); await productos.AddAsync(b);
    await unitOfWork.SaveChangesAsync();
    (yerba, mate, bombilla) = (y.Id, m.Id, b.Id);
}

var confirmacionesDePedidos = 0;                               // confirmaciones hechas por RegistrarPedidoHandler

if (args is not ["cambiar-precio"])                            // V02: los escenarios del enunciado (§4.10)
{
    await Registrar("Escenario principal", [new(yerba, 2), new(mate, 1)]);
    await Registrar("2a producto inexistente", [new(yerba, 1), new(Guid.Empty, 1)]);
    await Registrar("3a producto sin precio", [new(yerba, 1), new(bombilla, 1)]);
    await Registrar("3b cantidad cero", [new(mate, 0)]);
    await Registrar("4a pedido sin ítems", []);

    await CambiarPrecio(bombilla, 900m, conSeguimiento: true, mostrar: false);   // llegó el manifiesto: mismo Use Case que se mide abajo
    await Registrar("3a después de asignar precio", [new(bombilla, 3)]);
    Console.WriteLine();
}

// V03: Use Case de modificación, obtener → cambiar → SaveChangesAsync. Con seguimiento el cambio llega; sin seguimiento, no (§5.5).
await CambiarPrecio(mate, 13000m, conSeguimiento: true);
await CambiarPrecio(mate, 14000m, conSeguimiento: false);
using (var scope = provider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var precio = await db.Productos.Where(p => p.Id == mate).Select(p => p.Precio).SingleAsync();
    Console.WriteLine($"Precio de 'Mate' en la base: {precio:G29}");
}

async Task Registrar(string caso, RegistrarPedidoItem[] items)
{
    using var scope = provider.CreateScope();                  // un scope por Use Case, como un request
    var handler = scope.ServiceProvider.GetRequiredService<RegistrarPedidoHandler>();
    var antes = contador.Total;
    try
    {
        var r = await handler.Handle(new RegistrarPedidoCommand(items));
        Console.WriteLine($"{caso}: registrado, total {r.Total:G29}");   // G29: SQLite devuelve 21000.0; se muestra sin ceros
    }
    catch (DomainException ex)
    {
        Console.WriteLine($"{caso}: DomainException: {ex.Message}");
    }
    confirmacionesDePedidos += contador.Total - antes;
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Console.WriteLine($"   pedidos guardados: {await db.Pedidos.CountAsync()}, confirmaciones: {confirmacionesDePedidos}");
}

async Task CambiarPrecio(Guid productoId, decimal nuevoPrecio, bool conSeguimiento, bool mostrar = true)
{
    using var scope = provider.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Con seguimiento: el Handler tal como lo registra el composition root.
    // Sin seguimiento: el mismo Handler y el mismo DbContext, con el Repository de contraste armado a mano.
    var handler = conSeguimiento
        ? scope.ServiceProvider.GetRequiredService<CambiarPrecioProductoHandler>()
        : new CambiarPrecioProductoHandler(new ProductoRepositorySinSeguimiento(db), db);
    var nombre = await db.Productos.Where(p => p.Id == productoId).Select(p => p.Nombre).SingleAsync();
    var filas = await handler.Handle(new CambiarPrecioProductoCommand(productoId, nuevoPrecio));
    if (mostrar)
        Console.WriteLine($"Cambiar precio de '{nombre}' a {nuevoPrecio} {(conSeguimiento ? "con seguimiento" : "sin seguimiento (AsNoTracking)")}: filas afectadas: {filas}");
}

// Interceptor de EF Core: cuenta las confirmaciones sin tocar el DbContext ni el Handler.
class ContadorDeConfirmaciones : SaveChangesInterceptor
{
    public int Total { get; private set; }
    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        Total++;
        return base.SavedChangesAsync(eventData, result, ct);
    }
}
