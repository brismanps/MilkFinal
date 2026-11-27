using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

public class ParametroProducto
{
    private ParametroProducto() { }

    public ParametroProducto(TipoProducto tipo, decimal rendimiento, int vidaUtilDias, decimal precio)
    {
        Tipo = tipo;
        Actualizar(rendimiento, vidaUtilDias, precio);
    }

    public int Id { get; private set; }
    public TipoProducto Tipo { get; private set; }
    /// <summary>Litros de leche necesarios por unidad de producto.</summary>
    public decimal Rendimiento { get; private set; }
    public int VidaUtilDias { get; private set; }
    public decimal Precio { get; private set; }

    public void Actualizar(decimal rendimiento, int vidaUtilDias, decimal precio)
    {
        if (rendimiento <= 0) throw new ValidacionException($"El rendimiento de {Tipo} debe ser mayor que 0.");
        if (vidaUtilDias <= 0) throw new ValidacionException($"La vida útil de {Tipo} debe ser mayor que 0.");
        if (precio < 0) throw new ValidacionException($"El precio de {Tipo} no puede ser negativo.");
        Rendimiento = rendimiento;
        VidaUtilDias = vidaUtilDias;
        Precio = precio;
    }
}
