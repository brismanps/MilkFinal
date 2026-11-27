using Granja.Services.Mappings;
using Granja.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Granja.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        services.AddScoped<GranjaService>();
        services.AddScoped<ProduccionService>();
        services.AddScoped<InventarioService>();
        services.AddScoped<VentaService>();
        services.AddScoped<ReporteService>();
        return services;
    }
}
