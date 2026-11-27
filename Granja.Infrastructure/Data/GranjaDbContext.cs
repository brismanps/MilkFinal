using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Data;

public class GranjaDbContext : DbContext
{
    public GranjaDbContext(DbContextOptions<GranjaDbContext> options) : base(options) { }

    public DbSet<GranjaLechera> Granjas => Set<GranjaLechera>();
    public DbSet<Vaca> Vacas => Set<Vaca>();
    public DbSet<ParametroProducto> Parametros => Set<ParametroProducto>();
    public DbSet<ProduccionLeche> Producciones => Set<ProduccionLeche>();
    public DbSet<LoteProducto> Lotes => Set<LoteProducto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(GranjaDbContext).Assembly);
}
