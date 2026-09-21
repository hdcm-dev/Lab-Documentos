using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Productos;

namespace MyProject.Infrastructure.Persistence;

// El contexto de la variante: una sola Entity, la que tiene Factory Function con resultado.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ProductoQueDevuelve> Productos => Set<ProductoQueDevuelve>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}

// Contexto auxiliar SIN ninguna configuración: mapea una sola Entity por pura convención. Existe para que
// la demostración pueda provocar los dos errores de §5.4 —entidad sin constructor utilizable y propiedad
// `{ get; }` que desaparece del modelo— y para mostrar cómo persiste un enum cuando nadie dice nada.
public class ContextoDeUnaEntidad<TEntity> : DbContext
    where TEntity : class
{
    public ContextoDeUnaEntidad(DbContextOptions<ContextoDeUnaEntidad<TEntity>> options) : base(options) { }

    public DbSet<TEntity> Filas => Set<TEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<TEntity>();
}
