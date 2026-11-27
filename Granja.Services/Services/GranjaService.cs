using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;

namespace Granja.Services.Services;

internal static class GranjaRepositoryExtensions
{
    public static async Task<GranjaLechera> ObtenerRequeridaAsync(this IGranjaRepository repo) =>
        await repo.ObtenerAsync() ?? throw new NoEncontradoException("La granja no está configurada.");
}

public class GranjaService
{
    private readonly IGranjaRepository _granjas;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GranjaService(IGranjaRepository granjas, IUnitOfWork uow, IMapper mapper)
    {
        _granjas = granjas;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<GranjaResponse> ObtenerAsync() =>
        _mapper.Map<GranjaResponse>(await _granjas.ObtenerRequeridaAsync());

    public async Task<List<VacaResponse>> ListarVacasAsync()
    {
        var granja = await _granjas.ObtenerRequeridaAsync();
        return _mapper.Map<List<VacaResponse>>(granja.Vacas.OrderBy(v => v.Id).ToList());
    }

    public async Task<GranjaResponse> ActualizarAsync(ActualizarGranjaRequest req)
    {
        var granja = await _granjas.ObtenerRequeridaAsync();
        granja.ActualizarConfiguracion(req.NumeroVacas, req.LitrosPorVaca);

        foreach (var p in req.Parametros ?? new List<ParametroProductoDto>())
        {
            if (!Enum.IsDefined(p.Tipo)) throw new ValidacionException("Tipo de producto inválido.");
            granja.ObtenerParametro(p.Tipo).Actualizar(p.Rendimiento, p.VidaUtilDias, p.Precio);
        }

        await _uow.SaveChangesAsync();
        return _mapper.Map<GranjaResponse>(granja);
    }
}
