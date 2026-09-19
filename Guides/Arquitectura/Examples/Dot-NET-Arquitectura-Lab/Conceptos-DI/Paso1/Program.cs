var alta = new AltaDeProducto(new ArchivoDeTexto());   // composition root: el único que conoce las dos piezas
alta.Ejecutar("Mate", 3500m);

var prueba = new AltaDeProducto(new AlmacenEnMemoria()); // en una prueba: sin disco
prueba.Ejecutar("Yerba", 4200m);
Console.WriteLine(File.ReadAllText("productos.txt"));

// Adentro: la regla y la interfaz que declara lo que la regla necesita.
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

        _almacen.Guardar(nombre, precio);   // llama hacia afuera sin nombrar nada de afuera
    }
}

// Afuera: implementa la interfaz de adentro.
class ArchivoDeTexto : IAlmacenDeProductos
{
    public void Guardar(string nombre, decimal precio) =>
        File.AppendAllText("productos.txt", $"{nombre};{precio}\n");
}

class AlmacenEnMemoria : IAlmacenDeProductos
{
    public List<string> Guardados { get; } = new();
    public void Guardar(string nombre, decimal precio) => Guardados.Add(nombre);
}
