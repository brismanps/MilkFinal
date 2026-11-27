using AutoMapper;
using Granja.Domain.Entities;
using Granja.Services.Dtos;

namespace Granja.Services.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ParametroProducto, ParametroProductoDto>();
        CreateMap<GranjaLechera, GranjaResponse>();
        CreateMap<Vaca, VacaResponse>();
        CreateMap<LoteProducto, LoteResponse>();

        CreateMap<VentaDetalle, VentaDetalleResponse>()
            .ForMember(d => d.FechaProduccion, o => o.MapFrom(s => s.Lote.FechaProduccion));

        CreateMap<Venta, VentaResponse>()
            .ForMember(d => d.TipoProducto, o => o.MapFrom(s => s.Tipo))
            .ForMember(d => d.Detalle, o => o.MapFrom(s => s.Detalles));
    }
}
