var alta = new AltaDeProducto();
alta.Ejecutar("Mate", 3500m);
Console.WriteLine(File.ReadAllText("productos.txt"));

// Adentro: la regla del negocio.
class AltaDeProducto
{
    public void Ejecutar(string nombre, decimal precio)
    {
        if (precio <= 0)
            throw new InvalidOperationException("El precio debe ser mayor a cero.");

        var archivo = new ArchivoDeTexto();   // la regla nombra un detalle de afuera
        archivo.Guardar(nombre, precio);
    }
}

// Afuera: un detalle técnico.
class ArchivoDeTexto
{
    public void Guardar(string nombre, decimal precio) =>
        File.AppendAllText("productos.txt", $"{nombre};{precio}\n");
}
