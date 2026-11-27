using Granja.Domain.Enums;
using Granja.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly ReporteService _service;
    public ReportesController(ReporteService service) => _service = service;

    [HttpGet("ventas")]
    public async Task<IActionResult> Ventas([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, [FromQuery] TipoProducto? tipo)
        => Ok(await _service.VentasAsync(desde, hasta, tipo));

    [HttpGet("leche-diaria")]
    public async Task<IActionResult> LecheDiaria([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
        => Ok(await _service.LecheDiariaAsync(desde, hasta));

    [HttpGet("productodiario")]
    public async Task<IActionResult> ProductoDiario([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
        => Ok(await _service.ProductoDiarioAsync(desde, hasta));

    [HttpGet("producto-total")]
    public async Task<IActionResult> ProductoTotal() => Ok(await _service.ProductoTotalAsync());

    [HttpGet("productodanado")]
    public async Task<IActionResult> ProductoDanado([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
        => Ok(await _service.ProductoDanadoAsync(desde, hasta));

    /// <summary>Producción promedio, vaca que más y menos produce, y las que están por debajo del promedio.</summary>
    [HttpGet("vacas")]
    public async Task<IActionResult> Vacas([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
        => Ok(await _service.VacasAsync(desde, hasta));
}
