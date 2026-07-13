using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.ApiDefinitions;
using MovieHubAPI.DTOs;
using MovieHubAPI.DTOs.Pelicula;
using MovieHubAPI.Interfaces;
using System.ComponentModel;

namespace MovieHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Películas")]
    [Authorize]
    public class PeliculasController : ControllerBase, IPeliculaApi
    {
        private readonly IPeliculaService _peliculaService;

        public PeliculasController(IPeliculaService peliculaService)
        {
            _peliculaService = peliculaService;
        }

        [HttpGet]
        [EndpointSummary("Listar películas paginadas")]
        [EndpointDescription("Devuelve una página de películas con información detallada.")]
        [ProducesResponseType<PaginadosDto<PeliculaDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PaginadosDto<PeliculaDto>>> GetAll(
              [Description("Número de página (empieza en 1)")][FromQuery] int page = 1,
        [Description("Elementos por página (máx 50)")][FromQuery] int pageSize = 10,
        [Description("Filtrar por título")][FromQuery] string? titulo = null,
        [Description("Filtrar por género")][FromQuery] int? generoId = null,
        [Description("Ordenar por (puntuacion, anio)")][FromQuery] string? orden = null)
        {
            if (pageSize > 50) pageSize = 50;
            if (page < 1) page = 1;
            var resultado = await _peliculaService.GetAllPaginadoAsync(page, pageSize, titulo, generoId, orden);
            return Ok(resultado);
        }

        [HttpGet("mejor-valoradas")]
        [EndpointSummary("Top 10 mejor valoradas")]
        [ProducesResponseType<List<PeliculaDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<PeliculaDto>>> GetMejorValoradas()
        {
            return Ok(await _peliculaService.GetMejorValoradasAsync());
        }

        [HttpGet("mas-recientes")]
        [EndpointSummary("Top 10 más recientes")]
        [ProducesResponseType<List<PeliculaDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<PeliculaDto>>> GetMasRecientes()
        {
            return Ok(await _peliculaService.GetMasRecientesAsync());
        }

        [HttpGet("estadisticas")]
        [EndpointSummary("Estadísticas generales")]
        [ProducesResponseType<EstadisticasDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EstadisticasDto>> GetEstadisticas()
        {
            return Ok(await _peliculaService.GetEstadisticasAsync());
        }

        [HttpGet("{id}")]
        [EndpointSummary("Obtener película por ID")]
        [ProducesResponseType<PeliculaDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PeliculaDto>> GetById(int id)
        {
            var pelicula = await _peliculaService.GetByIdAsync(id);
            if (pelicula is null) return NotFound();
            return Ok(pelicula);
        }

        [HttpPost]
        [EndpointSummary("Crear nueva película")]
        [ProducesResponseType<PeliculaDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PeliculaDto>> Create(CreatePeliculaDto dto)
        {
            var pelicula = await _peliculaService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = pelicula.Id }, pelicula);
        }

        [HttpPut("{id}")]
        [EndpointSummary("Actualizar película existente")]
        [ProducesResponseType<PeliculaDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PeliculaDto>> Update(int id, UpdatePeliculaDto dto)
        {
            var pelicula = await _peliculaService.UpdateAsync(id, dto);
            if (pelicula is null) return NotFound();
            return Ok(pelicula);
        }

        [HttpDelete("{id}")]
        [EndpointSummary("Eliminar película")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _peliculaService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
