using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly GranjaDbContext Db;
    public Repository(GranjaDbContext db) => Db = db;

    public virtual async Task<T?> ObtenerPorIdAsync(int id) => await Db.Set<T>().FindAsync(id);
    public async Task AgregarAsync(T entidad) => await Db.Set<T>().AddAsync(entidad);
}

public class GranjaRepository : Repository<GranjaLechera>, IGranjaRepository
{
    public GranjaRepository(GranjaDbContext db) : base(db) { }

    public Task<GranjaLechera?> ObtenerAsync() =>
        Db.Granjas.Include(g => g.Vacas).Include(g => g.Parametros).FirstOrDefaultAsync();
}

public class ProduccionRepository : Repository<ProduccionLeche>, IProduccionRepository
{
    public ProduccionRepository(GranjaDbContext db) : base(db) { }

    public Task<bool> ExisteEnFechaAsync(DateOnly fecha) => Db.Producciones.AnyAsync(p => p.Fecha == fecha);

    public Task<List<ProduccionLeche>> ListarAsync(DateOnly? desde, DateOnly? hasta)
    {
        var q = Db.Producciones.Include(p => p.Vaca).AsQueryable();
        if (desde.HasValue) q = q.Where(p => p.Fecha >= desde.Value);
        if (hasta.HasValue) q = q.Where(p => p.Fecha <= hasta.Value);
        return q.OrderBy(p => p.Fecha).ThenBy(p => p.VacaId).ToListAsync();
    }
}

public class LoteRepository : Repository<LoteProducto>, ILoteRepository
{
    public LoteRepository(GranjaDbContext db) : base(db) { }

    public async Task<List<LoteProducto>> ObtenerVendiblesAsync(TipoProducto tipo, DateOnly fecha)
    {
        // FIFO: más antiguo primero. El filtro de existencias se hace en memoria (SQLite no compara decimal bien).
        var lotes = await Db.Lotes
            .Where(l => l.Tipo == tipo && l.FechaProduccion <= fecha && l.FechaVencimiento >= fecha)
            .OrderBy(l => l.FechaProduccion).ThenBy(l => l.Id)
            .ToListAsync();
        return lotes.Where(l => l.CantidadDisponible > 0 && !l.EstaVencido(fecha)).ToList();
    }

    public Task<List<LoteProducto>> ListarAsync() =>
        Db.Lotes.OrderBy(l => l.FechaProduccion).ThenBy(l => l.Id).ToListAsync();

    public Task<List<LoteProducto>> ListarPorFechaProduccionAsync(DateOnly fecha) =>
        Db.Lotes.Where(l => l.FechaProduccion == fecha).ToListAsync();
}

public class VentaRepository : Repository<Venta>, IVentaRepository
{
    public VentaRepository(GranjaDbContext db) : base(db) { }

    private IQueryable<Venta> ConDetalles() => Db.Ventas.Include(v => v.Detalles).ThenInclude(d => d.Lote);

    public override async Task<Venta?> ObtenerPorIdAsync(int id) =>
        await ConDetalles().FirstOrDefaultAsync(v => v.Id == id);

    public Task<List<Venta>> ListarAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo)
    {
        var q = ConDetalles();
        if (desde.HasValue) q = q.Where(v => v.Fecha >= desde.Value);
        if (hasta.HasValue) q = q.Where(v => v.Fecha <= hasta.Value);
        if (tipo.HasValue) q = q.Where(v => v.Tipo == tipo.Value);
        return q.OrderBy(v => v.Fecha).ThenBy(v => v.Id).ToListAsync();
    }
}
