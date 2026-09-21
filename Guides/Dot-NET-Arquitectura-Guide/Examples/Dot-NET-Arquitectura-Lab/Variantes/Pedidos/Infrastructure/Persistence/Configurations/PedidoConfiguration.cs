using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Domain.Pedidos;

namespace MyProject.Infrastructure.Persistence.Configurations;

// El Aggregate se mapea entero: ItemPedido es un tipo owned, sin DbSet ni Repository propios.
public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");
        builder.HasKey(p => p.Id);

        builder.OwnsMany(p => p.Items, items =>
        {
            items.ToTable("ItemsPedido");
            items.WithOwner().HasForeignKey("PedidoId");
            items.Property<int>("Id");                            // clave técnica: no existe en Domain
            items.HasKey("Id");
            items.Property(i => i.Descripcion).IsRequired().HasMaxLength(200);
            items.Property(i => i.PrecioUnitario).HasPrecision(18, 2);
        });
        // EF Core carga y guarda los ítems por el campo privado _items; Items sigue siendo de solo lectura.
        builder.Navigation(p => p.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
