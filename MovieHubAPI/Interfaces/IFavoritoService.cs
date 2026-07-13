using MovieHubAPI.DTOs.Favorito;

namespace MovieHubAPI.Interfaces;

public interface IFavoritoService
{
    Task<List<FavoritoDto>> GetByUserIdAsync(long usuarioId);
    Task<bool> AddAsync(int peliculaId, long usuarioId);
    Task<bool> RemoveAsync(int peliculaId, long usuarioId);
}
