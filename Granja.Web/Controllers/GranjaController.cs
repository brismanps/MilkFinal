using Granja.Services.Dtos;
using Granja.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/granja")]
public class GranjaController : ControllerBase
{
    private readonly GranjaService _service;
    public GranjaController(GranjaService service) => _service = service;

    /// <summary>Configuración (n, l) y parámetros por producto.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(GranjaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener() => Ok(await _service.ObtenerAsync());

    /// <summary>Actualiza n, l y rendimiento / vida útil / precio por producto.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(GranjaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromBody] ActualizarGranjaRequest request)
        => Ok(await _service.ActualizarAsync(request));

    /// <summary>Lista las vacas (útil para registrar producción individual).</summary>
    [HttpGet("vacas")]
    public async Task<IActionResult> Vacas() => Ok(await _service.ListarVacasAsync());
}
