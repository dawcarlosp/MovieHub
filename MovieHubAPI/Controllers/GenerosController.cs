using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.ApiDefinitions;
using MovieHubAPI.DTOs.Genero;
using MovieHubAPI.Interfaces;

namespace MovieHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EndpointGroupName("Géneros")]
    public class GenerosController : ControllerBase, IGeneroApi
    {
        private readonly IGeneroService _generoService;

        public GenerosController(IGeneroService generoService)
        {
            _generoService = generoService;
        }

        public async Task<ActionResult<List<GeneroDto>>> GetAll()
        {
            var generos = await _generoService.GetAllAsync();
            return Ok(generos);
        }

        public async Task<ActionResult<GeneroDto>> GetById(int id)
        {
            var genero = await _generoService.GetByIdAsync(id);
            if (genero is null) return NotFound();
            return Ok(genero);
        }

        public async Task<ActionResult<GeneroDto>> Create(CreateGeneroDto dto)
        {
            var genero = await _generoService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = genero.Id }, genero);
        }

        public async Task<ActionResult<GeneroDto>> Update(int id, UpdateGeneroDto dto)
        {
            var genero = await _generoService.UpdateAsync(id, dto);
            if (genero is null) return NotFound();
            return Ok(genero);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _generoService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
