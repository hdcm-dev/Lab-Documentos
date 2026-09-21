using MyProject.Domain.Pedidos;

namespace MyProject.Infrastructure.Persistence.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _db;

    public PedidoRepository(AppDbContext db) => _db = db;

    public void Add(Pedido pedido) => _db.Pedidos.Add(pedido);     // marca para insertar (con sus ítems); confirma IUnitOfWork
}
