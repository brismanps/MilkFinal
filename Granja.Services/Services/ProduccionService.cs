using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;

namespace Granja.Services.Services;

public class ProduccionService
{
    private readonly IGranjaRepository _granjas;
    private readonly IProduccionRepository _producciones;
    private readonly ILoteRepository _lotes;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public ProduccionService(IGranjaRepository granjas, IProduccionRepository producciones, ILoteRepository lotes,
        IUnitOfWork uow, IClock clock, IMapper mapper)
    {
        _granjas = granjas;
        _producciones = producciones;
        _lotes = lotes;
        _uow = uow;
        _clock = clock;
        _mapper = mapper;
    }

    /// <summary>RN-01: cada vaca activa produce l litros (o el valor indicado) una sola vez por fecha.</summary>
    public async Task<ProduccionDiariaResponse> RegistrarProduccionAsync(ProduccionRequest req)
    {
        var fecha = req.Fecha ?? _clock.Today;
        var granja = await _granjas.ObtenerRequeridaAsync();

        if (await _producciones.ExisteEnFechaAsync(fecha))
            throw new ConflictoException($"Ya existe producción registrada para {fecha:yyyy-MM-dd}.");

        var vacas = granja.Vacas.Where(v => v.Activa).ToList();
        var indicados = req.Vacas ?? new List<ProduccionVacaRequest>();

        if (indicados.GroupBy(x => x.VacaId).Any(g => g.Count() > 1))
            throw new ValidacionException("Hay vacas repetidas en la solicitud.");

        foreach (var i in indicados)
            if (vacas.All(v => v.Id != i.VacaId))
                throw new NoEncontradoException($"No existe la vaca activa con id {i.VacaId}.");

        foreach (var vaca in vacas)
        {
            var indicado = indicados.FirstOrDefault(x => x.VacaId == vaca.Id);
            var litros = indicado?.Litros ?? granja.LitrosPorVaca;
            await _producciones.AgregarAsync(new ProduccionLeche(vaca.Id, fecha, litros));
        }

        await _uow.SaveChangesAsync();
        return await ResumenDiaAsync(fecha);
    }

    public async Task<ProduccionDiariaResponse> ResumenDiaAsync(DateOnly fecha)
    {
        var producidos = (await _producciones.ListarAsync(fecha, fecha)).Sum(p => p.Litros);
        var transformados = (await _lotes.ListarPorFechaProduccionAsync(fecha)).Sum(l => l.LitrosUsados);
        return new ProduccionDiariaResponse(fecha, producidos, transformados, producidos - transformados);
    }

    /// <summary>RN-02, RN-03, RN-04: transforma leche de una fecha en un lote de queso o yogurt.</summary>
    public async Task<LoteResponse> TransformarAsync(TransformarRequest req)
    {
        if (!Enum.IsDefined(req.Tipo)) throw new ValidacionException("Tipo de producto inválido.");
        if (req.Litros <= 0) throw new ValidacionException("Los litros a transformar deben ser mayores que 0.");

        var fecha = req.Fecha ?? _clock.Today;
        var granja = await _granjas.ObtenerRequeridaAsync();
        var parametro = granja.ObtenerParametro(req.Tipo);

        var resumen = await ResumenDiaAsync(fecha);
        if (req.Litros > resumen.LitrosNoAprovechados)
            throw new LecheInsuficienteException(fecha, resumen.LitrosNoAprovechados, req.Litros);

        var cantidad = req.Litros / parametro.Rendimiento;
        var lote = new LoteProducto(req.Tipo, fecha, parametro.VidaUtilDias, req.Litros, cantidad);

        await _lotes.AgregarAsync(lote);
        await _uow.SaveChangesAsync();
        return _mapper.Map<LoteResponse>(lote);
    }
}
