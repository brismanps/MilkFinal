using Granja.Domain.Enums;

namespace Granja.Domain.Exceptions;

public abstract class DominioException : Exception
{
    protected DominioException(string mensaje) : base(mensaje) { }
}

/// <summary>Se mapea a 400 Bad Request.</summary>
public class ValidacionException : DominioException
{
    public ValidacionException(string mensaje) : base(mensaje) { }
}

/// <summary>Se mapea a 404 Not Found.</summary>
public class NoEncontradoException : DominioException
{
    public NoEncontradoException(string mensaje) : base(mensaje) { }
}

/// <summary>Se mapea a 409 Conflict.</summary>
public class ConflictoException : DominioException
{
    public ConflictoException(string mensaje) : base(mensaje) { }
}

public class InventarioInsuficienteException : ConflictoException
{
    public InventarioInsuficienteException(TipoProducto tipo, decimal cantidad)
        : base($"No hay existencias no vencidas suficientes de {tipo} para vender {cantidad}.") { }
}

public class LecheInsuficienteException : ConflictoException
{
    public LecheInsuficienteException(DateOnly fecha, decimal disponible, decimal solicitado)
        : base($"Leche insuficiente el {fecha:yyyy-MM-dd}: quedan {disponible} L sin transformar y se piden {solicitado} L.") { }
}
