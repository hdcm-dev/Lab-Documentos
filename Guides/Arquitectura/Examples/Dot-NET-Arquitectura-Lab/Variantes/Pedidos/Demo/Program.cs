using MyProject.Application.Common;
using MyProject.Application.Pedidos.Commands.RegistrarPedido;
using MyProject.Domain.Common;
using MyProject.Domain.Pedidos;
using MyProject.Domain.Productos;

// Composition root de la demostración: dobles en memoria en lugar de EF Core.
var yerba = Producto.Create("Yerba 1 kg", 4500m);
var mate = Producto.Create("Mate", 12000m);
var bombilla = Producto.Create("Bombilla");                    // llegó sin manifiesto de precios

var productos = new ProductosEnMemoria(yerba, mate, bombilla);
var pedidos = new PedidosEnMemoria();
var unitOfWork = new UnitOfWorkContador();
var handler = new RegistrarPedidoHandler(productos, pedidos, unitOfWork, TimeProvider.System);

await Registrar("Escenario principal", [new(yerba.Id, 2), new(mate.Id, 1)]);
await Registrar("2a producto inexistente", [new(yerba.Id, 1), new(Guid.Empty, 1)]);
await Registrar("3a producto sin precio", [new(yerba.Id, 1), new(bombilla.Id, 1)]);
await Registrar("3b cantidad cero", [new(mate.Id, 0)]);
await Registrar("4a pedido sin ítems", []);

bombilla.AsignarPrecio(900m);                                  // llegó el manifiesto
await Registrar("3a después de asignar precio", [new(bombilla.Id, 3)]);

async Task Registrar(string caso, RegistrarPedidoItem[] items)
{
    try
    {
        var r = await handler.Handle(new RegistrarPedidoCommand(items));
        Console.WriteLine($"{caso}: registrado, total {r.Total}");
    }
    catch (DomainException ex)
    {
        Console.WriteLine($"{caso}: DomainException: {ex.Message}");
    }
    Console.WriteLine($"   pedidos guardados: {pedidos.Guardados.Count}, confirmaciones: {unitOfWork.Confirmaciones}");
}

class ProductosEnMemoria(params Producto[] productos) : IProductoRepository
{
    public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(productos.FirstOrDefault(p => p.Id == id));
    public Task AddAsync(Producto producto, CancellationToken ct = default) => Task.CompletedTask;
}

class PedidosEnMemoria : IPedidoRepository
{
    public List<Pedido> Guardados { get; } = new();
    public void Add(Pedido pedido) => Guardados.Add(pedido);
}

class UnitOfWorkContador : IUnitOfWork
{
    public int Confirmaciones { get; private set; }
    // Cuenta confirmaciones; el doble no escribe filas, así que devuelve 0.
    public Task<int> SaveChangesAsync(CancellationToken ct = default) { Confirmaciones++; return Task.FromResult(0); }
}
