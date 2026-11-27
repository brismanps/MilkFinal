using Granja.Domain.Enums;

namespace Granja.Services.Dtos;

// ---------- Granja ----------
public class ParametroProductoDto
{
    public TipoProducto Tipo { get; set; }
    public decimal Rendimiento { get; set; }
    public int VidaUtilDias { get; set; }
    public decimal Precio { get; set; }
}

public class GranjaResponse
{
    public int NumeroVacas { get; set; }
    public decimal LitrosPorVaca { get; set; }
    public List<ParametroProductoDto> Parametros { get; set; } = new();
}

public record ActualizarGranjaRequest(int NumeroVacas, decimal LitrosPorVaca, List<ParametroProductoDto>? Parametros);

public class VacaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; }
}

// ---------- Producción ----------
public record ProduccionVacaRequest(int VacaId, decimal Litros);
public record ProduccionRequest(DateOnly? Fecha, List<ProduccionVacaRequest>? Vacas);
public record ProduccionDiariaResponse(DateOnly Fecha, decimal LitrosProducidos, decimal LitrosTransformados, decimal LitrosNoAprovechados);
public record TransformarRequest(TipoProducto Tipo, decimal Litros, DateOnly? Fecha);

public class LoteResponse
{
    public int Id { get; set; }
    public TipoProducto Tipo { get; set; }
    public DateOnly FechaProduccion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public decimal LitrosUsados { get; set; }
    public decimal CantidadInicial { get; set; }
    public decimal CantidadDisponible { get; set; }
    public decimal CantidadDanada { get; set; }
    public EstadoLote Estado { get; set; }
}

// ---------- Inventario ----------
public record ResumenInventarioItem(TipoProducto Tipo, decimal DisponibleVendible, decimal VencidoSinProcesar);
public record InventarioResponse(DateOnly Fecha, List<LoteResponse> Lotes, List<ResumenInventarioItem> Resumen);
public record VencimientosResponse(DateOnly Fecha, List<LoteResponse> LotesProcesados, decimal TotalDanado);

// ---------- Ventas ----------
public record VentaRequest(TipoProducto TipoProducto, decimal Cantidad, DateOnly? Fecha);

public class VentaDetalleResponse
{
    public int LoteId { get; set; }
    public DateOnly FechaProduccion { get; set; }
    public decimal Cantidad { get; set; }
}

public class VentaResponse
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public List<VentaDetalleResponse> Detalle { get; set; } = new();
}

// ---------- Reportes ----------
public record VentaReporteItem(int VentaId, DateOnly Fecha, TipoProducto Tipo, decimal Cantidad, decimal PrecioUnitario, decimal Total);
public record ReporteVentasResponse(DateOnly? Desde, DateOnly? Hasta, List<VentaReporteItem> Ventas, decimal TotalGeneral);

public record LecheDiariaItem(DateOnly Fecha, decimal LitrosProducidos, decimal LitrosTransformados, decimal LitrosNoAprovechados);
public record ProductoDiarioItem(DateOnly Fecha, TipoProducto Tipo, decimal Cantidad);
public record ProductoTotalItem(TipoProducto Tipo, decimal Producido, decimal Vendido, decimal Disponible, decimal Danado);

public record ProductoDanadoItem(int LoteId, TipoProducto Tipo, DateOnly FechaProduccion, DateOnly FechaVencimiento, decimal CantidadDanada);
public record TotalDanadoPorTipo(TipoProducto Tipo, decimal CantidadDanada);
public record ReporteProductoDanadoResponse(List<ProductoDanadoItem> Lotes, List<TotalDanadoPorTipo> PorTipo);

public record VacaProduccionItem(int VacaId, string Nombre, decimal TotalLitros, int DiasRegistrados, decimal PromedioDiario);
public record ReporteVacasResponse(
    decimal PromedioDiarioPorVaca,
    VacaProduccionItem? MasProduce,
    VacaProduccionItem? MenosProduce,
    List<VacaProduccionItem> PorDebajoDelPromedio,
    List<VacaProduccionItem> Vacas);
