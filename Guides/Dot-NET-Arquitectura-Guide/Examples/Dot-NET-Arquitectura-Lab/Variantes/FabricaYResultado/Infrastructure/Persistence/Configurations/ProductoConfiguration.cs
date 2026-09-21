using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Domain.Productos;

namespace MyProject.Infrastructure.Persistence.Configurations;

// Mapeo desde afuera: ProductoQueDevuelve no sabe que existe una tabla, ni un enum guardado como texto,
// ni un conversor. Las tres decisiones de almacenamiento viven acá.
public class ProductoConfiguration : IEntityTypeConfiguration<ProductoQueDevuelve>
{
    public void Configure(EntityTypeBuilder<ProductoQueDevuelve> builder)
    {
        builder.ToTable("Productos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Precio).HasPrecision(18, 2);

        // El enum como TEXTO y no como entero: un valor que este binario no declara falla al leerlo
        // en lugar de entrar como número desconocido.
        builder.Property(p => p.Estado).HasConversion<string>().IsRequired();

        // El Value Object viaja como una columna de texto. De ida, su código; de vuelta, el camino de
        // materialización, que no valida: lo que ya está guardado no se rechaza al leerlo.
        builder.Property(p => p.Moneda)
            .HasConversion(moneda => moneda.Codigo, codigo => Moneda.Materializar(codigo))
            .HasMaxLength(3)
            .IsRequired();
    }
}
