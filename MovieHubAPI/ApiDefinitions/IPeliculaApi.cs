using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.DTOs;
using MovieHubAPI.DTOs.Pelicula;

namespace MovieHubAPI.ApiDefinitions;

public interface IPeliculaApi
{
    [HttpGet]
    [EndpointSummary("Listar películas paginadas")]
    [EndpointDescription("Devuelve una página de películas con información detallada.")]
    [ProducesResponseType<PaginadosDto<PeliculaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    Task<ActionResult<PaginadosDto<PeliculaDto>>> GetAll(
        [Description("Número de página (empieza en 1)")][FromQuery] int page = 1,
        [Description("Elementos por página (máx 50)")][FromQuery] int pageSize = 10
    );

    [HttpGet("{id}")]
    [EndpointSummary("Obtener película por ID")]
    [ProducesResponseType<PeliculaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    Task<ActionResult<PeliculaDto>> GetById(int id);

    [HttpPost]
    [EndpointSummary("Crear nueva película")]
    [ProducesResponseType<PeliculaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    Task<ActionResult<PeliculaDto>> Create(CreatePeliculaDto dto);

    [HttpPut("{id}")]
    [EndpointSummary("Actualizar película existente")]
    [ProducesResponseType<PeliculaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    Task<ActionResult<PeliculaDto>> Update(int id, UpdatePeliculaDto dto);

    [HttpDelete("{id}")]
    [EndpointSummary("Eliminar película")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    Task<IActionResult> Delete(int id);
}
