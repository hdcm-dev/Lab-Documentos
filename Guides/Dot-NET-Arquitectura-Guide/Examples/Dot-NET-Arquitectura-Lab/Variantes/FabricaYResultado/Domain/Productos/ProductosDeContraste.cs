namespace MyProject.Domain.Productos;

// Las dos entidades que NO hay que escribir. Existen para que el laboratorio muestre el error literal
// en lugar de describirlo; ningún otro archivo de esta variante las usa para trabajar.

// CONTRASTE 1 — sin constructor sin parámetros, y con nombres de parámetro que no coinciden con las
// propiedades mapeadas. EF Core no tiene por dónde entrar y el modelo no se construye (§5.4).
public class ProductoSinCtorPrivado
{
    public ProductoSinCtorPrivado(string textoLibre, decimal importe)
    {
        Id = Guid.NewGuid();
        Nombre = textoLibre;
        Precio = importe;
    }

    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
}

// CONTRASTE 2 — una propiedad de sólo lectura de verdad, `{ get; }`, en lugar de setter privado.
// Compila, no avisa nada y desaparece del modelo: no hay columna, no se guarda y no se relee.
public class ProductoConPropiedadSinSetter
{
    private ProductoConPropiedadSinSetter() => Nombre = string.Empty;

    public Guid Id { get; private set; }
    public string Nombre { get; }
    public decimal Precio { get; private set; }
}
