using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Granja.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Granja.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<GranjaDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<IGranjaRepository, GranjaRepository>();
        services.AddScoped<IProduccionRepository, ProduccionRepository>();
        services.AddScoped<ILoteRepository, LoteRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
