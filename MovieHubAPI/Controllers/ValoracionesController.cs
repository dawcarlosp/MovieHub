using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.DTOs.Valoracion;
using MovieHubAPI.Interfaces;

namespace MovieHubAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Valoraciones")]
[Authorize]
public class ValoracionesController : ControllerBase
{
    private readonly IValoracionService _valoracionService;

    public ValoracionesController(IValoracionService valoracionService)
    {
        _valoracionService = valoracionService;
    }

    [HttpPost]
    [EndpointSummary("Crear valoración")]
    [ProducesResponseType<ValoracionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ValoracionDto>> Create(CreateValoracionDto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var valoracion = await _valoracionService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetByPelicula), new { peliculaId = dto.PeliculaId }, valoracion);
    }

    [HttpPut("{id}")]
    [EndpointSummary("Modificar valoración")]
    [ProducesResponseType<ValoracionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ValoracionDto>> Update(int id, [FromBody] int puntuacion)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _valoracionService.UpdateAsync(id, puntuacion, userId);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Eliminar valoración")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var deleted = await _valoracionService.DeleteAsync(id, userId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("pelicula/{peliculaId}")]
    [EndpointSummary("Obtener valoraciones de una película")]
    [ProducesResponseType<List<ValoracionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ValoracionDto>>> GetByPelicula(int peliculaId)
    {
        var valoraciones = await _valoracionService.GetByPeliculaIdAsync(peliculaId);
        return Ok(valoraciones);
    }
}
