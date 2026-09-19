using MyProject.Domain.Common;
using MyProject.Domain.Productos;

// Cada caso imprime lo que el dominio permite y lo que rechaza.
var yerba = Producto.Create("Yerba 1 kg");                      // llegó sin manifiesto de precios
Console.WriteLine($"1. Alta sin precio: Precio = {(yerba.Precio is null ? "null" : yerba.Precio)}");

Intentar("2. Vender sin precio", () => yerba.PrecioDeVenta());
Intentar("3. Asignar precio -5", () => { yerba.AsignarPrecio(-5m); return 0m; });

yerba.AsignarPrecio(4500m);
Intentar("4. Vender con precio", () => yerba.PrecioDeVenta());
Intentar("5. Alta con precio 0", () => { Producto.Create("Mate", 0m); return 0m; });

static void Intentar(string caso, Func<decimal> accion)
{
    try { Console.WriteLine($"{caso}: {accion()}"); }
    catch (DomainException ex) { Console.WriteLine($"{caso}: DomainException: {ex.Message}"); }
}
