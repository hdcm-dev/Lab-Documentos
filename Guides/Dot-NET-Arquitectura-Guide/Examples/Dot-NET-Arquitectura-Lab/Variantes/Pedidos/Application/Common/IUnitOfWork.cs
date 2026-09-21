namespace MyProject.Application.Common;

// Interfaz de servicio técnico: la implementa el AppDbContext en Infrastructure.
// La firma es la de DbContext.SaveChangesAsync, que devuelve la cantidad de filas afectadas,
// para que el DbContext la satisfaga sin adaptador.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
