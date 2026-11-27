using Granja.Domain.Entities;
using Granja.Domain.Enums;

namespace Granja.Domain.Interfaces;

public interface IClock
{
    DateOnly Today { get; }
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IRepository<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task AgregarAsync(T entidad);
}

public interface IGranjaRepository : IRepository<GranjaLechera>
{
    /// <summary>Devuelve la granja con sus vacas y parámetros.</summary>
    Task<GranjaLechera?> ObtenerAsync();
}

public interface IProduccionRepository : IRepository<ProduccionLeche>
{
    Task<bool> ExisteEnFechaAsync(DateOnly fecha);
    Task<List<ProduccionLeche>> ListarAsync(DateOnly? desde, DateOnly? hasta);
}

public interface ILoteRepository : IRepository<LoteProducto>
{
    /// <summary>Lotes con existencias, no vencidos a la fecha, ordenados por FechaProduccion ASC.</summary>
    Task<List<LoteProducto>> ObtenerVendiblesAsync(TipoProducto tipo, DateOnly fecha);
    Task<List<LoteProducto>> ListarAsync();
    Task<List<LoteProducto>> ListarPorFechaProduccionAsync(DateOnly fecha);
}

public interface IVentaRepository : IRepository<Venta>
{
    Task<List<Venta>> ListarAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo);
}
