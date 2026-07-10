using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.DTOs.Pelicula;
using MovieHubAPI.Interfaces;

namespace MovieHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaService _peliculaService;

        public PeliculasController(IPeliculaService peliculaService)
        {
            _peliculaService = peliculaService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PeliculaDto>>> GetAll()
        {
            var peliculas = await _peliculaService.GetAllAsync();
            return Ok(peliculas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PeliculaDto>> GetById(int id)
        {
            var pelicula = await _peliculaService.GetByIdAsync(id);
            if (pelicula is null) return NotFound();
            return Ok(pelicula);
        }

        [HttpPost]
        public async Task<ActionResult<PeliculaDto>> Create(CreatePeliculaDto dto)
        {
            var pelicula = await _peliculaService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = pelicula.Id }, pelicula);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PeliculaDto>> Update(int id, UpdatePeliculaDto dto)
        {
            var pelicula = await _peliculaService.UpdateAsync(id, dto);
            if (pelicula is null) return NotFound();
            return Ok(pelicula);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _peliculaService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
