using Granja.Domain.Enums;
using Granja.Services.Dtos;
using Granja.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/ventas")]
public class VentasController : ControllerBase
{
    private readonly VentaService _service;
    public VentasController(VentaService service) => _service = service;

    /// <summary>Registra una venta (FIFO, atómica). 409 si no hay existencias no vencidas suficientes.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VentaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] VentaRequest request)
    {
        var venta = await _service.RegistrarVentaAsync(request);
        return CreatedAtAction(nameof(Obtener), new { id = venta.Id }, venta);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VentaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(int id) => Ok(await _service.ObtenerAsync(id));

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, [FromQuery] TipoProducto? tipo)
        => Ok(await _service.ListarAsync(desde, hasta, tipo));
}
