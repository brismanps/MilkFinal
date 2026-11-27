using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

public class LoteProducto
{
    private LoteProducto() { }

    public LoteProducto(TipoProducto tipo, DateOnly fechaProduccion, int vidaUtilDias, decimal litrosUsados, decimal cantidad)
    {
        if (cantidad <= 0) throw new ValidacionException("La cantidad del lote debe ser mayor que 0.");
        Tipo = tipo;
        FechaProduccion = fechaProduccion;
        FechaVencimiento = fechaProduccion.AddDays(vidaUtilDias);
        LitrosUsados = litrosUsados;
        CantidadInicial = cantidad;
        CantidadDisponible = cantidad;
    }

    public int Id { get; private set; }
    public TipoProducto Tipo { get; private set; }
    public DateOnly FechaProduccion { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public decimal LitrosUsados { get; private set; }
    public decimal CantidadInicial { get; private set; }
    public decimal CantidadDisponible { get; private set; }
    public decimal CantidadDanada { get; private set; }

    public EstadoLote Estado =>
        CantidadDanada > 0 ? EstadoLote.Danado
        : CantidadDisponible == 0 ? EstadoLote.Agotado
        : EstadoLote.Disponible;

    public bool EstaVencido(DateOnly fecha) => fecha > FechaVencimiento;

    public void Descontar(decimal cantidad)
    {
        if (cantidad <= 0 || cantidad > CantidadDisponible)
            throw new InvalidOperationException("Cantidad inválida para el lote.");
        CantidadDisponible -= cantidad;
    }

    public decimal MarcarComoDanado()
    {
        var perdida = CantidadDisponible;
        CantidadDanada += perdida;
        CantidadDisponible = 0;
        return perdida;
    }
}
