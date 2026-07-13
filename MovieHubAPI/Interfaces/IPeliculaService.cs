using MovieHubAPI.DTOs;
using MovieHubAPI.DTOs.Pelicula;

namespace MovieHubAPI.Interfaces
{
    public interface IPeliculaService
    {
        Task<PaginadosDto<PeliculaDto>> GetAllPaginadoAsync(int page, int pageSize, string? titulo, int? generoId, string? orden);
        Task<PeliculaDto?> GetByIdAsync(int id);
        Task<PeliculaDto> CreateAsync(CreatePeliculaDto dto);
        Task<PeliculaDto?> UpdateAsync(int id, UpdatePeliculaDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<PeliculaDto>> GetMejorValoradasAsync();
        Task<List<PeliculaDto>> GetMasRecientesAsync();
        Task<EstadisticasDto> GetEstadisticasAsync();
    }
}
