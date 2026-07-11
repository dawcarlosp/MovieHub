using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.DTOs.Genero;

namespace MovieHubAPI.ApiDefinitions;

[EndpointGroupName("Géneros")]
public interface IGeneroApi
{
    [HttpGet]
    [EndpointSummary("Listar todos los géneros")]
    [EndpointDescription("Devuelve la lista completa de géneros disponibles.")]
    [ProducesResponseType<List<GeneroDto>>(StatusCodes.Status200OK)]
    Task<ActionResult<List<GeneroDto>>> GetAll();

    [HttpGet("{id}")]
    [EndpointSummary("Obtener género por ID")]
    [ProducesResponseType<GeneroDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    Task<ActionResult<GeneroDto>> GetById(int id);

    [HttpPost]
    [EndpointSummary("Crear nuevo género")]
    [ProducesResponseType<GeneroDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    Task<ActionResult<GeneroDto>> Create(CreateGeneroDto dto);

    [HttpPut("{id}")]
    [EndpointSummary("Actualizar género existente")]
    [ProducesResponseType<GeneroDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    Task<ActionResult<GeneroDto>> Update(int id, UpdateGeneroDto dto);

    [HttpDelete("{id}")]
    [EndpointSummary("Eliminar género")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    Task<IActionResult> Delete(int id);
}
