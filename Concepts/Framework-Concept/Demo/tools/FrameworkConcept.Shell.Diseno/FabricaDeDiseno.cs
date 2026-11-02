using FrameworkConcept.Shell.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FrameworkConcept.Shell.Diseno;

/// <summary>
/// P5 — Cómo arma «dotnet ef» el contexto en tiempo de diseño: ContextoDelCascaron sólo tiene el constructor con
/// opciones, y en la app las arma DbSqlite. Acá se usa el mismo proveedor (SQLite) con un archivo que nunca se abre:
/// optimize lee el modelo, no la base.
/// </summary>
public sealed class FabricaDeDiseno : IDesignTimeDbContextFactory<ContextoDelCascaron>
{
    public ContextoDelCascaron CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ContextoDelCascaron>()
            .UseSqlite($"Data Source={EsquemaDelCascaron.NombreDelArchivo}")
            .Options);
}
