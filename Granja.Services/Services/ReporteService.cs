using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;

namespace Granja.Services.Services;

public class ReporteService
{
    private readonly IGranjaRepository _granjas;
    private readonly IProduccionRepository _producciones;
    private readonly ILoteRepository _lotes;
    private readonly IVentaRepository _ventas;

    public ReporteService(IGranjaRepository granjas, IProduccionRepository producciones,
        ILoteRepository lotes, IVentaRepository ventas)
    {
        _granjas = granjas;
        _producciones = producciones;
        _lotes = lotes;
        _ventas = ventas;
    }

    private static void ValidarRango(DateOnly? desde, DateOnly? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new ValidacionException("'desde' no puede ser mayor que 'hasta'.");
    }

    private static bool EnRango(DateOnly f, DateOnly? desde, DateOnly? hasta) =>
        (desde is null || f >= desde) && (hasta is null || f <= hasta);

    public async Task<ReporteVentasResponse> VentasAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo)
    {
        ValidarRango(desde, hasta);
        var items = (await _ventas.ListarAsync(desde, hasta, tipo))
            .Select(v => new VentaReporteItem(v.Id, v.Fecha, v.Tipo, v.Cantidad, v.PrecioUnitario, v.Total))
            .ToList();
        return new ReporteVentasResponse(desde, hasta, items, items.Sum(i => i.Total));
    }

    public async Task<List<LecheDiariaItem>> LecheDiariaAsync(DateOnly? desde, DateOnly? hasta)
    {
        ValidarRango(desde, hasta);
        var producciones = await _producciones.ListarAsync(desde, hasta);
        var lotes = (await _lotes.ListarAsync()).Where(l => EnRango(l.FechaProduccion, desde, hasta)).ToList();

        return producciones.GroupBy(p => p.Fecha).OrderBy(g => g.Key).Select(g =>
        {
            var total = g.Sum(p => p.Litros);
            var transformados = lotes.Where(l => l.FechaProduccion == g.Key).Sum(l => l.LitrosUsados);
            return new LecheDiariaItem(g.Key, total, transformados, total - transformados);
        }).ToList();
    }

    public async Task<List<ProductoDiarioItem>> ProductoDiarioAsync(DateOnly? desde, DateOnly? hasta)
    {
        ValidarRango(desde, hasta);
        return (await _lotes.ListarAsync())
            .Where(l => EnRango(l.FechaProduccion, desde, hasta))
            .GroupBy(l => new { l.FechaProduccion, l.Tipo })
            .OrderBy(g => g.Key.FechaProduccion).ThenBy(g => g.Key.Tipo)
            .Select(g => new ProductoDiarioItem(g.Key.FechaProduccion, g.Key.Tipo, g.Sum(l => l.CantidadInicial)))
            .ToList();
    }

    public async Task<List<ProductoTotalItem>> ProductoTotalAsync()
    {
        var lotes = await _lotes.ListarAsync();
        return Enum.GetValues<TipoProducto>().Select(t =>
        {
            var l = lotes.Where(x => x.Tipo == t).ToList();
            var producido = l.Sum(x => x.CantidadInicial);
            var disponible = l.Sum(x => x.CantidadDisponible);
            var danado = l.Sum(x => x.CantidadDanada);
            return new ProductoTotalItem(t, producido, producido - disponible - danado, disponible, danado);
        }).ToList();
    }

    public async Task<ReporteProductoDanadoResponse> ProductoDanadoAsync(DateOnly? desde, DateOnly? hasta)
    {
        ValidarRango(desde, hasta);
        var danados = (await _lotes.ListarAsync())
            .Where(l => l.CantidadDanada > 0 && EnRango(l.FechaVencimiento, desde, hasta))
            .OrderBy(l => l.FechaVencimiento).ThenBy(l => l.Id)
            .ToList();

        var lotes = danados
            .Select(l => new ProductoDanadoItem(l.Id, l.Tipo, l.FechaProduccion, l.FechaVencimiento, l.CantidadDanada))
            .ToList();
        var porTipo = danados.GroupBy(l => l.Tipo)
            .Select(g => new TotalDanadoPorTipo(g.Key, g.Sum(l => l.CantidadDanada)))
            .ToList();
        return new ReporteProductoDanadoResponse(lotes, porTipo);
    }

    /// <summary>Reporte extra: producción promedio, vaca que más/menos produce y las que están por debajo del promedio.</summary>
    public async Task<ReporteVacasResponse> VacasAsync(DateOnly? desde, DateOnly? hasta)
    {
        ValidarRango(desde, hasta);
        var granja = await _granjas.ObtenerRequeridaAsync();
        var porVaca = (await _producciones.ListarAsync(desde, hasta))
            .GroupBy(p => p.VacaId).ToDictionary(g => g.Key, g => g.ToList());

        var items = granja.Vacas
            .Where(v => v.Activa || porVaca.ContainsKey(v.Id))
            .OrderBy(v => v.Id)
            .Select(v =>
            {
                porVaca.TryGetValue(v.Id, out var registros);
                var total = registros?.Sum(r => r.Litros) ?? 0m;
                var dias = registros?.Count ?? 0;
                return new VacaProduccionItem(v.Id, v.Nombre, total, dias, dias == 0 ? 0m : total / dias);
            }).ToList();

        var diasVaca = items.Sum(i => i.DiasRegistrados);
        var promedio = diasVaca == 0 ? 0m : items.Sum(i => i.TotalLitros) / diasVaca;

        return new ReporteVacasResponse(
            promedio,
            items.OrderByDescending(i => i.PromedioDiario).FirstOrDefault(),
            items.OrderBy(i => i.PromedioDiario).FirstOrDefault(),
            items.Where(i => i.PromedioDiario < promedio).ToList(),
            items);
    }
}
