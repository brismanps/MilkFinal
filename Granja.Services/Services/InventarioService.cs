using AutoMapper;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;

namespace Granja.Services.Services;

public class InventarioService
{
    private readonly ILoteRepository _lotes;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public InventarioService(ILoteRepository lotes, IUnitOfWork uow, IClock clock, IMapper mapper)
    {
        _lotes = lotes;
        _uow = uow;
        _clock = clock;
        _mapper = mapper;
    }

    public async Task<InventarioResponse> ObtenerInventarioAsync(DateOnly? fecha = null)
    {
        var f = fecha ?? _clock.Today;
        var conExistencias = (await _lotes.ListarAsync()).Where(l => l.CantidadDisponible > 0).ToList();

        var resumen = Enum.GetValues<TipoProducto>().Select(t => new ResumenInventarioItem(
            t,
            conExistencias.Where(l => l.Tipo == t && !l.EstaVencido(f)).Sum(l => l.CantidadDisponible),
            conExistencias.Where(l => l.Tipo == t && l.EstaVencido(f)).Sum(l => l.CantidadDisponible))).ToList();

        return new InventarioResponse(f, _mapper.Map<List<LoteResponse>>(conExistencias), resumen);
    }

    /// <summary>RN-08: lo vencido y no vendido pasa de disponible a dañado.</summary>
    public async Task<VencimientosResponse> ProcesarVencimientosAsync(DateOnly? fecha = null)
    {
        var f = fecha ?? _clock.Today;
        var vencidos = (await _lotes.ListarAsync())
            .Where(l => l.CantidadDisponible > 0 && l.EstaVencido(f))
            .ToList();

        var total = vencidos.Sum(l => l.MarcarComoDanado());
        await _uow.SaveChangesAsync();
        return new VencimientosResponse(f, _mapper.Map<List<LoteResponse>>(vencidos), total);
    }

    /// <summary>Extensión opcional: lotes que vencen dentro de los próximos N días.</summary>
    public async Task<List<LoteResponse>> ProximosAVencerAsync(int dias = 3, DateOnly? fecha = null)
    {
        var f = fecha ?? _clock.Today;
        var limite = f.AddDays(dias);
        var lotes = (await _lotes.ListarAsync())
            .Where(l => l.CantidadDisponible > 0 && !l.EstaVencido(f) && l.FechaVencimiento <= limite)
            .OrderBy(l => l.FechaVencimiento)
            .ToList();
        return _mapper.Map<List<LoteResponse>>(lotes);
    }
}
