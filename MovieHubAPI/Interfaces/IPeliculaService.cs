using MovieHubAPI.DTOs.Pelicula;

namespace MovieHubAPI.Interfaces
{
    public interface IPeliculaService
    {
        Task<List<PeliculaDto>> GetAllAsync();
        Task<PeliculaDto?> GetByIdAsync(int id);
        Task<PeliculaDto> CreateAsync(CreatePeliculaDto dto);
        Task<PeliculaDto?> UpdateAsync(int id, UpdatePeliculaDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
