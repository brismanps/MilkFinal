using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

public class Venta
{
    private readonly List<VentaDetalle> _detalles = new();

    private Venta() { }

    public Venta(DateOnly fecha, TipoProducto tipo, decimal precioUnitario)
    {
        Fecha = fecha;
        Tipo = tipo;
        PrecioUnitario = precioUnitario;
    }

    public int Id { get; private set; }
    public DateOnly Fecha { get; private set; }
    public TipoProducto Tipo { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public IReadOnlyCollection<VentaDetalle> Detalles => _detalles;

    public decimal Cantidad => _detalles.Sum(d => d.Cantidad);
    public decimal Total => Cantidad * PrecioUnitario;

    public void AgregarDetalle(LoteProducto lote, decimal cantidad)
    {
        if (cantidad <= 0) throw new ValidacionException("La cantidad del detalle debe ser mayor que 0.");
        _detalles.Add(new VentaDetalle(lote, cantidad));
    }
}
