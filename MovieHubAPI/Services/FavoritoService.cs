using Microsoft.EntityFrameworkCore;
using MovieHubAPI.DTOs.Favorito;
using MovieHubAPI.Interfaces;

namespace MovieHubAPI.Services;

public class FavoritoService : IFavoritoService
{
    private readonly MovieHubDbContext _context;

    public FavoritoService(MovieHubDbContext context) => _context = context;

    public async Task<List<FavoritoDto>> GetByUserIdAsync(long usuarioId)
    {
        return await _context.Favoritos
            .Where(f => f.UsuarioId == usuarioId)
            .Include(f => f.Pelicula)
            .OrderByDescending(f => f.Pelicula.Titulo)
            .Select(f => new FavoritoDto(
                f.PeliculaId,
                f.Pelicula.Titulo,
                f.Pelicula.PosterUrl,
                f.Pelicula.PuntuacionMedia
            ))
            .ToListAsync();
    }

    public async Task<bool> AddAsync(int peliculaId, long usuarioId)
    {
        var existe = await _context.Favoritos
            .AnyAsync(f => f.UsuarioId == usuarioId && f.PeliculaId == peliculaId);

        if (existe) return false;

        _context.Favoritos.Add(new FavoritoModel
        {
            UsuarioId = usuarioId,
            PeliculaId = peliculaId
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(int peliculaId, long usuarioId)
    {
        var favorito = await _context.Favoritos
            .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.PeliculaId == peliculaId);

        if (favorito is null) return false;

        _context.Favoritos.Remove(favorito);
        await _context.SaveChangesAsync();
        return true;
    }
}
