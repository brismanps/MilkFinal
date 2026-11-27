using AutoMapper;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Granja.Infrastructure.Repositories;
using Granja.Services.Mappings;
using Granja.Services.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Granja.Tests;

public class FakeClock : IClock
{
    public DateOnly Today { get; set; } = new(2026, 12, 1);
}

/// <summary>Arma los servicios sobre SQLite en memoria (n = 10, l = 15 por defecto).</summary>
public sealed class Entorno : IDisposable
{
    private readonly SqliteConnection _conn;
    public GranjaDbContext Db { get; }
    public ProduccionService Produccion { get; }
    public InventarioService Inventario { get; }
    public VentaService Ventas { get; }
    public ReporteService Reportes { get; }
    public GranjaService Granja { get; }

    public Entorno()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        Db = new GranjaDbContext(new DbContextOptionsBuilder<GranjaDbContext>().UseSqlite(_conn).Options);
        DbInitializer.InicializarAsync(Db).GetAwaiter().GetResult();

        IMapper mapper = new MapperConfiguration(
            c => c.AddProfile<MappingProfile>(),
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance
        ).CreateMapper();
        var clock = new FakeClock();
        var uow = new UnitOfWork(Db);
        var granjas = new GranjaRepository(Db);
        var producciones = new ProduccionRepository(Db);
        var lotes = new LoteRepository(Db);
        var ventas = new VentaRepository(Db);

        Granja = new GranjaService(granjas, uow, mapper);
        Produccion = new ProduccionService(granjas, producciones, lotes, uow, clock, mapper);
        Inventario = new InventarioService(lotes, uow, clock, mapper);
        Ventas = new VentaService(granjas, lotes, ventas, uow, clock, mapper);
        Reportes = new ReporteService(granjas, producciones, lotes, ventas);
    }

    public static DateOnly D(int dia) => new(2026, 12, dia);

    /// <summary>Registra producción del día y transforma lo indicado.</summary>
    public async Task ProducirAsync(int dia, params (TipoProducto tipo, decimal litros)[] transformaciones)
    {
        await Produccion.RegistrarProduccionAsync(new(D(dia), null));
        foreach (var (tipo, litros) in transformaciones)
            await Produccion.TransformarAsync(new(tipo, litros, D(dia)));
    }

    public void Dispose()
    {
        Db.Dispose();
        _conn.Dispose();
    }
}
