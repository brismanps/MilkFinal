namespace Granja.Domain.Entities;

public class VentaDetalle
{
    private VentaDetalle() { }

    public VentaDetalle(LoteProducto lote, decimal cantidad)
    {
        Lote = lote;
        LoteId = lote.Id;
        Cantidad = cantidad;
    }

    public int Id { get; private set; }
    public int LoteId { get; private set; }
    public LoteProducto Lote { get; private set; } = null!;
    public decimal Cantidad { get; private set; }
}
