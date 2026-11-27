using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Data;

public static class DbInitializer
{
    /// <summary>
    /// Aplica migraciones (o crea el esquema si todavía no hay migraciones) y siembra la granja por defecto.
    /// </summary>
    public static async Task InicializarAsync(GranjaDbContext db)
    {
        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        if (await db.Granjas.AnyAsync()) return;

        // Valores sugeridos del taller (editables vía PUT /api/granja)
        var granja = GranjaLechera.Crear(numeroVacas: 10, litrosPorVaca: 15);
        granja.AgregarParametro(TipoProducto.Queso, rendimiento: 10, vidaUtilDias: 30, precio: 28000);
        granja.AgregarParametro(TipoProducto.Yogurt, rendimiento: 1, vidaUtilDias: 15, precio: 8000);
        db.Granjas.Add(granja);
        await db.SaveChangesAsync();
    }
}
