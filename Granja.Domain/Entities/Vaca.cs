namespace Granja.Domain.Entities;

public class Vaca
{
    private Vaca() { }

    public Vaca(string nombre)
    {
        Nombre = nombre;
        Activa = true;
    }

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public bool Activa { get; private set; }

    public void Activar() => Activa = true;
    public void Desactivar() => Activa = false;
}
