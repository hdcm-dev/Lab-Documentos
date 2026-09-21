using MyProject.Domain.Common;

namespace MyProject.Domain.Productos;

// ESTRATEGIA B — el MISMO producto, con las mismas dos reglas, devolviendo el rechazo en lugar de lanzarlo.
// El constructor privado y la Factory Function no cambian: lo que cambia es el tipo de retorno.
public class ProductoQueDevuelve
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }
    public EstadoProducto Estado { get; private set; }
    public Moneda Moneda { get; private set; } = Moneda.Materializar("ARS");

    // Testigo de la demostración, no parte del patrón: cuenta las veces que se ejecutó el constructor.
    public static int VecesQueCorrioElConstructorPrivado { get; private set; }

    // La costura del ORM: EF Core llama a este constructor al leer cada fila y después asigna las
    // propiedades por su cuenta, aunque los setters sean privados. No valida nada, a propósito.
    private ProductoQueDevuelve() => VecesQueCorrioElConstructorPrivado++;

    public static DomainResult<ProductoQueDevuelve> Create(string? nombre, decimal precio, string? moneda)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return DomainResult<ProductoQueDevuelve>.Rechazar("NOMBRE_REQUERIDO");
        if (precio <= 0)
            return DomainResult<ProductoQueDevuelve>.Rechazar("PRECIO_NO_POSITIVO");

        var monedaElegida = Moneda.Create(moneda);
        if (!monedaElegida.TryGetValue(out var valor, out var codigo))
            return DomainResult<ProductoQueDevuelve>.Rechazar(codigo);

        return DomainResult<ProductoQueDevuelve>.Aplicar(new ProductoQueDevuelve
        {
            Id = Guid.NewGuid(),
            Nombre = nombre.Trim(),
            Precio = precio,
            Estado = EstadoProducto.Borrador,
            Moneda = valor
        });
    }

    public DomainResult Publicar()
    {
        if (Estado != EstadoProducto.Borrador)
            return DomainResult.Rechazar("PUBLICACION_FUERA_DE_BORRADOR");

        Estado = EstadoProducto.Publicado;
        return DomainResult.Aplicar();
    }
}
