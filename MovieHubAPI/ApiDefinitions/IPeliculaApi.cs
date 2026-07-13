using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.DTOs;
using MovieHubAPI.DTOs.Pelicula;

namespace MovieHubAPI.ApiDefinitions;

public interface IPeliculaApi
{
    Task<ActionResult<PaginadosDto<PeliculaDto>>> GetAll(
       int page, int pageSize, string? titulo, int? generoId, string? orden
    );
    Task<ActionResult<PeliculaDto>> GetById(int id);
    Task<ActionResult<PeliculaDto>> Create(CreatePeliculaDto dto);
    Task<ActionResult<PeliculaDto>> Update(int id, UpdatePeliculaDto dto);
    Task<IActionResult> Delete(int id);
    Task<ActionResult<List<PeliculaDto>>> GetMejorValoradas();
    Task<ActionResult<List<PeliculaDto>>> GetMasRecientes();
    Task<ActionResult<EstadisticasDto>> GetEstadisticas();
}
