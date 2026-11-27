using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;

namespace Granja.Services.Services;

public class VentaService
{
    private readonly IGranjaRepository _granjas;
    private readonly ILoteRepository _lotes;
    private readonly IVentaRepository _ventas;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public VentaService(IGranjaRepository granjas, ILoteRepository lotes, IVentaRepository ventas,
        IUnitOfWork uow, IClock clock, IMapper mapper)
    {
        _granjas = granjas;
        _lotes = lotes;
        _ventas = ventas;
        _uow = uow;
        _clock = clock;
        _mapper = mapper;
    }

    /// <summary>Venta FIFO atómica (RN-05 a RN-09).</summary>
    public async Task<VentaResponse> RegistrarVentaAsync(VentaRequest req)
    {
        if (!Enum.IsDefined(req.TipoProducto)) throw new ValidacionException("Tipo de producto inválido.");
        if (req.Cantidad <= 0) throw new ValidacionException("La cantidad a vender debe ser mayor que 0.");

        var fecha = req.Fecha ?? _clock.Today;
        var granja = await _granjas.ObtenerRequeridaAsync();
        var precio = granja.ObtenerParametro(req.TipoProducto).Precio;

        // Lotes con existencias, no vencidos a la fecha, ordenados por FechaProduccion ASC
        var lotes = await _lotes.ObtenerVendiblesAsync(req.TipoProducto, fecha);

        // RN-07: si no alcanza, se rechaza completa y no se toca nada
        if (lotes.Sum(l => l.CantidadDisponible) < req.Cantidad)
            throw new InventarioInsuficienteException(req.TipoProducto, req.Cantidad);

        var venta = new Venta(fecha, req.TipoProducto, precio);
        var restante = req.Cantidad;

        foreach (var lote in lotes)
        {
            if (restante == 0) break;
            var tomar = Math.Min(lote.CantidadDisponible, restante);
            lote.Descontar(tomar);
            venta.AgregarDetalle(lote, tomar);
            restante -= tomar;
        }

        await _ventas.AgregarAsync(venta);
        await _uow.SaveChangesAsync(); // una sola transacción
        return _mapper.Map<VentaResponse>(venta);
    }

    public async Task<VentaResponse> ObtenerAsync(int id)
    {
        var venta = await _ventas.ObtenerPorIdAsync(id)
                    ?? throw new NoEncontradoException($"No existe la venta {id}.");
        return _mapper.Map<VentaResponse>(venta);
    }

    public async Task<List<VentaResponse>> ListarAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new ValidacionException("'desde' no puede ser mayor que 'hasta'.");
        return _mapper.Map<List<VentaResponse>>(await _ventas.ListarAsync(desde, hasta, tipo));
    }
}
