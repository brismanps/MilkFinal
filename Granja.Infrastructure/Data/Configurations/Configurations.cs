using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class GranjaLecheraConfiguration : IEntityTypeConfiguration<GranjaLechera>
{
    public void Configure(EntityTypeBuilder<GranjaLechera> b)
    {
        b.HasKey(g => g.Id);
        b.HasMany(g => g.Vacas).WithOne().HasForeignKey("GranjaLecheraId");
        b.HasMany(g => g.Parametros).WithOne().HasForeignKey("GranjaLecheraId");
        b.Navigation(g => g.Vacas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(g => g.Parametros).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class VacaConfiguration : IEntityTypeConfiguration<Vaca>
{
    public void Configure(EntityTypeBuilder<Vaca> b)
    {
        b.HasKey(v => v.Id);
        b.Property(v => v.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class ParametroProductoConfiguration : IEntityTypeConfiguration<ParametroProducto>
{
    public void Configure(EntityTypeBuilder<ParametroProducto> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
    }
}

public class ProduccionLecheConfiguration : IEntityTypeConfiguration<ProduccionLeche>
{
    public void Configure(EntityTypeBuilder<ProduccionLeche> b)
    {
        b.HasKey(p => p.Id);
        b.HasOne(p => p.Vaca).WithMany().HasForeignKey(p => p.VacaId);
        // RN-01: una sola producción por fecha y por vaca
        b.HasIndex(p => new { p.VacaId, p.Fecha }).IsUnique();
    }
}

public class LoteProductoConfiguration : IEntityTypeConfiguration<LoteProducto>
{
    public void Configure(EntityTypeBuilder<LoteProducto> b)
    {
        b.HasKey(l => l.Id);
        b.Property(l => l.Tipo).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(l => new { l.Tipo, l.FechaProduccion });
    }
}

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> b)
    {
        b.HasKey(v => v.Id);
        b.Property(v => v.Tipo).HasConversion<string>().HasMaxLength(20);
        b.HasMany(v => v.Detalles).WithOne().HasForeignKey("VentaId");
        b.Navigation(v => v.Detalles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> b)
    {
        b.HasKey(d => d.Id);
        b.HasOne(d => d.Lote).WithMany().HasForeignKey(d => d.LoteId);
    }
}
