using MyProject.Application.Productos.Commands.CrearProducto;
using MyProject.Domain.Common;

namespace MyProject.Application.Tests;

public class CrearProductoHandlerTests
{
    [Fact]
    public async Task Handle_guarda_el_producto_y_devuelve_su_id()
    {
        var repository = new FakeProductoRepository();
        var handler = new CrearProductoHandler(repository);

        var id = await handler.Handle(new CrearProductoCommand("Yerba 1 kg", 4500m));

        Assert.Single(repository.Guardados);
        Assert.Equal(id, repository.Guardados[0].Id);
    }

    [Fact]
    public async Task Handle_con_precio_negativo_no_guarda_nada()
    {
        var repository = new FakeProductoRepository();
        var handler = new CrearProductoHandler(repository);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new CrearProductoCommand("Yerba 1 kg", -5m)));
        Assert.Empty(repository.Guardados);
    }
}
