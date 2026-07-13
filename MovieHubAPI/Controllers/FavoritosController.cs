using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.DTOs.Favorito;
using MovieHubAPI.Interfaces;

namespace MovieHubAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Favoritos")]
[Authorize]
public class FavoritosController : ControllerBase
{
    private readonly IFavoritoService _favoritoService;

    public FavoritosController(IFavoritoService favoritoService)
    {
        _favoritoService = favoritoService;
    }

    [HttpGet]
    [EndpointSummary("Listar favoritos del usuario")]
    [ProducesResponseType<List<FavoritoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<FavoritoDto>>> GetAll()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _favoritoService.GetByUserIdAsync(userId));
    }

    [HttpPost("{peliculaId}")]
    [EndpointSummary("Añadir película a favoritos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Add(int peliculaId)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var added = await _favoritoService.AddAsync(peliculaId, userId);
        if (!added) return Conflict(new { message = "La película ya está en favoritos." });
        return CreatedAtAction(nameof(GetAll), null);
    }

    [HttpDelete("{peliculaId}")]
    [EndpointSummary("Quitar película de favoritos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Remove(int peliculaId)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var removed = await _favoritoService.RemoveAsync(peliculaId, userId);
        if (!removed) return NotFound();
        return NoContent();
    }
}
