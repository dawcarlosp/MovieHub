using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.ApiDefinitions;
using MovieHubAPI.DTOs.Genero;
using MovieHubAPI.Interfaces;

namespace MovieHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Géneros")]
    public class GenerosController : ControllerBase, IGeneroApi
    {
        private readonly IGeneroService _generoService;

        public GenerosController(IGeneroService generoService)
        {
            _generoService = generoService;
        }
        [HttpGet]
        [EndpointSummary("Listar todos los géneros")]
        [EndpointDescription("Devuelve la lista completa de géneros disponibles.")]
        [ProducesResponseType<List<GeneroDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GeneroDto>>> GetAll()
        {
            var generos = await _generoService.GetAllAsync();
            return Ok(generos);
        }

        [HttpGet("{id}")]
        [EndpointSummary("Obtener género por ID")]
        [ProducesResponseType<GeneroDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneroDto>> GetById(int id)
        {
            var genero = await _generoService.GetByIdAsync(id);
            if (genero is null) return NotFound();
            return Ok(genero);
        }

        [HttpPost]
        [EndpointSummary("Crear nuevo género")]
        [ProducesResponseType<GeneroDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GeneroDto>> Create(CreateGeneroDto dto)
        {
            var genero = await _generoService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = genero.Id }, genero);
        }

        [HttpPut("{id}")]
        [EndpointSummary("Actualizar género existente")]
        [ProducesResponseType<GeneroDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneroDto>> Update(int id, UpdateGeneroDto dto)
        {
            var genero = await _generoService.UpdateAsync(id, dto);
            if (genero is null) return NotFound();
            return Ok(genero);
        }

        [HttpDelete("{id}")]
        [EndpointSummary("Eliminar género")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _generoService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
