using Granja.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/inventario")]
public class InventarioController : ControllerBase
{
    private readonly InventarioService _service;
    public InventarioController(InventarioService service) => _service = service;

    /// <summary>Lotes con existencias y resumen vendible / vencido por producto.</summary>
    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] DateOnly? fecha) => Ok(await _service.ObtenerInventarioAsync(fecha));

    /// <summary>Pasa a "dañado" lo vencido y no vendido.</summary>
    [HttpPost("procesar-vencimientos")]
    public async Task<IActionResult> ProcesarVencimientos([FromQuery] DateOnly? fecha)
        => Ok(await _service.ProcesarVencimientosAsync(fecha));

    /// <summary>Alerta de lotes próximos a vencer (por defecto 3 días).</summary>
    [HttpGet("proximos-a-vencer")]
    public async Task<IActionResult> ProximosAVencer([FromQuery] int dias = 3, [FromQuery] DateOnly? fecha = null)
        => Ok(await _service.ProximosAVencerAsync(dias, fecha));
}
