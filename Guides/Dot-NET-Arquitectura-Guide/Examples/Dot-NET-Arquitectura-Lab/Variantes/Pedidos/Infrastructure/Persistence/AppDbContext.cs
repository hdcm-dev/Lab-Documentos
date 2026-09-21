using Microsoft.EntityFrameworkCore;
using MyProject.Application.Common;
using MyProject.Domain.Pedidos;
using MyProject.Domain.Productos;

namespace MyProject.Infrastructure.Persistence;

// Unit of Work real: el DbContext ya tiene Task<int> SaveChangesAsync(CancellationToken), así que IUnitOfWork se satisface sin adaptador.
public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
