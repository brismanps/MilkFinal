using Granja.Domain.Interfaces;

namespace Granja.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly GranjaDbContext _db;
    public UnitOfWork(GranjaDbContext db) => _db = db;

    // SaveChanges de EF Core corre en una sola transacción.
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
