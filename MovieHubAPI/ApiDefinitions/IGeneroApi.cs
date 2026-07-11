using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.DTOs.Genero;

namespace MovieHubAPI.ApiDefinitions;

public interface IGeneroApi
{
    Task<ActionResult<List<GeneroDto>>> GetAll();
    Task<ActionResult<GeneroDto>> GetById(int id);
    Task<ActionResult<GeneroDto>> Create(CreateGeneroDto dto);
    Task<ActionResult<GeneroDto>> Update(int id, UpdateGeneroDto dto);
    Task<IActionResult> Delete(int id);
}
