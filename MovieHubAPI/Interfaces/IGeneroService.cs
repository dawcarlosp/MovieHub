using MovieHubAPI.DTOs.Genero;

namespace MovieHubAPI.Interfaces
{
    public interface IGeneroService
    {
        Task<List<GeneroDto>> GetAllAsync();
        Task<GeneroDto?> GetByIdAsync(int id);
        Task<GeneroDto> CreateAsync(CreateGeneroDto dto);
        Task<GeneroDto?> UpdateAsync(int id, UpdateGeneroDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
