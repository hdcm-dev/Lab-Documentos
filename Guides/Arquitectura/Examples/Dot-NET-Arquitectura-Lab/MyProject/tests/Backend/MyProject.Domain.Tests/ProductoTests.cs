using MyProject.Domain.Common;
using MyProject.Domain.Productos;

namespace MyProject.Domain.Tests;

public class ProductoTests
{
    [Fact]
    public void Create_con_precio_negativo_lanza_DomainException() =>
        Assert.Throws<DomainException>(() => Producto.Create("Mate", -5m));

    [Fact]
    public void Create_con_datos_validos_devuelve_un_producto_activo()
    {
        var producto = Producto.Create("Mate", 3500m);

        Assert.True(producto.Activo);
        Assert.Equal(3500m, producto.Precio);
    }

    [Fact]
    public void Dos_Dinero_con_los_mismos_datos_son_iguales() =>
        Assert.Equal(new Dinero(10m, "ARS"), new Dinero(10m, "ARS"));
}
