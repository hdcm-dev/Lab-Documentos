using Microsoft.EntityFrameworkCore;

namespace FrameworkConcept.Shell.Data;

/// <summary>El DbContext de EF Core para las tablas del cascarón. Sólo lo usa <see cref="DbSqlite"/>.</summary>
public sealed class ContextoDelCascaron(DbContextOptions<ContextoDelCascaron> opciones) : DbContext(opciones)
{
    public DbSet<FilaDeOutbox> Outbox => Set<FilaDeOutbox>();
    public DbSet<FilaDeDocumento> Documentos => Set<FilaDeDocumento>();
    public DbSet<FilaDeDefinicion> Definiciones => Set<FilaDeDefinicion>();
    public DbSet<FilaDeVersionDeContenido> VersionesDeContenido => Set<FilaDeVersionDeContenido>();
    public DbSet<FilaDeConfiguracion> Configuracion => Set<FilaDeConfiguracion>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.Entity<FilaDeOutbox>(t =>
        {
            t.ToTable("outbox");
            t.HasKey(f => f.Id);
            t.HasIndex(f => f.ClaveIdempotencia).IsUnique();     // P5 — clave de idempotencia única
            t.HasIndex(f => new { f.Estado, f.ProximoIntentoEn });
            t.Property(f => f.Estado).HasConversion<int>();
        });

        modelo.Entity<FilaDeDocumento>(t =>
        {
            t.ToTable("documentos");
            t.HasKey(d => new { d.Coleccion, d.Id });
        });

        modelo.Entity<FilaDeDefinicion>(t =>
        {
            t.ToTable("definiciones");
            t.HasKey(d => new { d.EncuestaId, d.Version });
        });

        modelo.Entity<FilaDeVersionDeContenido>(t =>
        {
            t.ToTable("version_contenido");
            t.HasKey(v => v.Id);
        });

        modelo.Entity<FilaDeConfiguracion>(t =>
        {
            t.ToTable("configuracion");
            t.HasKey(c => c.Clave);
        });
    }
}
