using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

public class ProduccionLeche
{
    private ProduccionLeche() { }

    public ProduccionLeche(int vacaId, DateOnly fecha, decimal litros)
    {
        if (litros < 0) throw new ValidacionException("Los litros producidos no pueden ser negativos.");
        VacaId = vacaId;
        Fecha = fecha;
        Litros = litros;
    }

    public int Id { get; private set; }
    public int VacaId { get; private set; }
    public Vaca Vaca { get; private set; } = null!;
    public DateOnly Fecha { get; private set; }
    public decimal Litros { get; private set; }
}
