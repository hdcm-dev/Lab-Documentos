using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<IAlmacenDeProductos, ArchivoDeTexto>();   // «cuando pidan la interfaz, entregá esto»
services.AddTransient<AltaDeProducto>();

using var provider = services.BuildServiceProvider();
var alta = provider.GetRequiredService<AltaDeProducto>();       // el contenedor hace los new
alta.Ejecutar("Mate", 3500m);
Console.WriteLine(File.ReadAllText("productos.txt"));

interface IAlmacenDeProductos
{
    void Guardar(string nombre, decimal precio);
}

class AltaDeProducto
{
    private readonly IAlmacenDeProductos _almacen;
    public AltaDeProducto(IAlmacenDeProductos almacen) => _almacen = almacen;
    public void Ejecutar(string nombre, decimal precio)
    {
        if (precio <= 0)
            throw new InvalidOperationException("El precio debe ser mayor a cero.");
        _almacen.Guardar(nombre, precio);
    }
}

class ArchivoDeTexto : IAlmacenDeProductos
{
    public void Guardar(string nombre, decimal precio) =>
        File.AppendAllText("productos.txt", $"{nombre};{precio}\n");
}
