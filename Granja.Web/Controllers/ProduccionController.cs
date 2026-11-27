using Granja.Services.Dtos;
using Granja.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/produccion")]
public class ProduccionController : ControllerBase
{
    private readonly ProduccionService _service;
    public ProduccionController(ProduccionService service) => _service = service;

    /// <summary>Registra la producción de leche del día (n × l litros). Una sola vez por fecha.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProduccionDiariaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] ProduccionRequest request)
    {
        var r = await _service.RegistrarProduccionAsync(request);
        return CreatedAtAction(nameof(ObtenerDia), new { fecha = r.Fecha.ToString("yyyy-MM-dd") }, r);
    }

    /// <summary>Resumen de la leche de una fecha: producida, transformada y no aprovechada.</summary>
    [HttpGet("{fecha}")]
    public async Task<IActionResult> ObtenerDia(DateOnly fecha) => Ok(await _service.ResumenDiaAsync(fecha));

    /// <summary>Transforma leche de una fecha en un lote de queso o yogurt.</summary>
    [HttpPost("transformar")]
    [ProducesResponseType(typeof(LoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Transformar([FromBody] TransformarRequest request)
        => StatusCode(StatusCodes.Status201Created, await _service.TransformarAsync(request));
}
