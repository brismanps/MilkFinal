using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>
/// Entidad "Granja". Se llama GranjaLechera para no chocar con el namespace Granja.
/// </summary>
public class GranjaLechera
{
    private readonly List<Vaca> _vacas = new();
    private readonly List<ParametroProducto> _parametros = new();

    private GranjaLechera() { }

    public int Id { get; private set; }
    public int NumeroVacas { get; private set; }
    public decimal LitrosPorVaca { get; private set; }
    public IReadOnlyCollection<Vaca> Vacas => _vacas;
    public IReadOnlyCollection<ParametroProducto> Parametros => _parametros;

    public static GranjaLechera Crear(int numeroVacas, decimal litrosPorVaca)
    {
        var g = new GranjaLechera();
        g.ActualizarConfiguracion(numeroVacas, litrosPorVaca);
        return g;
    }

    public void ActualizarConfiguracion(int numeroVacas, decimal litrosPorVaca)
    {
        if (numeroVacas <= 0) throw new ValidacionException("El número de vacas (n) debe ser mayor que 0.");
        if (litrosPorVaca <= 0) throw new ValidacionException("Los litros por vaca (l) deben ser mayores que 0.");
        NumeroVacas = numeroVacas;
        LitrosPorVaca = litrosPorVaca;
        AjustarVacas();
    }

    public void AgregarParametro(TipoProducto tipo, decimal rendimiento, int vidaUtilDias, decimal precio)
        => _parametros.Add(new ParametroProducto(tipo, rendimiento, vidaUtilDias, precio));

    public ParametroProducto ObtenerParametro(TipoProducto tipo)
        => _parametros.FirstOrDefault(p => p.Tipo == tipo)
           ?? throw new NoEncontradoException($"No hay parámetros configurados para {tipo}.");

    // Mantiene sincronizada la cantidad de vacas activas con n (las vacas con historial no se borran).
    private void AjustarVacas()
    {
        var activas = _vacas.Where(v => v.Activa).OrderBy(v => v.Id).ToList();
        if (activas.Count > NumeroVacas)
        {
            foreach (var v in activas.Skip(NumeroVacas)) v.Desactivar();
            return;
        }

        var faltan = NumeroVacas - activas.Count;
        foreach (var v in _vacas.Where(v => !v.Activa).OrderBy(v => v.Id).Take(faltan).ToList())
            v.Activar();

        faltan = NumeroVacas - _vacas.Count(v => v.Activa);
        for (var i = 0; i < faltan; i++)
            _vacas.Add(new Vaca($"Vaca {_vacas.Count + 1}"));
    }
}
