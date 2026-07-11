using Mapster;
using Microsoft.EntityFrameworkCore;
using MovieHubAPI.DTOs;
using MovieHubAPI.DTOs.Pelicula;
using MovieHubAPI.Interfaces;

namespace MovieHubAPI.Services
{
    public class PeliculaService : IPeliculaService
    {
        private readonly MovieHubDbContext _context;

        public PeliculaService(MovieHubDbContext context) => _context = context;

        public async Task<PaginadosDto<PeliculaDto>> GetAllPaginadoAsync(int page, int pageSize)
        {
            var query = _context.Peliculas
                .Include(p => p.PeliculaGeneros).ThenInclude(pg => pg.Genero)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var peliculas = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginadosDto<PeliculaDto>(
                peliculas.Adapt<List<PeliculaDto>>(),
                page,
                pageSize,
                totalCount,
                (int)Math.Ceiling(totalCount / (double)pageSize)
            );
        }

        public async Task<PeliculaDto?> GetByIdAsync(int id)
        {
            var pelicula = await _context.Peliculas
                .Include(p => p.PeliculaGeneros).ThenInclude(pg => pg.Genero)
                .FirstOrDefaultAsync(p => p.Id == id);
            return pelicula?.Adapt<PeliculaDto>();
        }

        public async Task<PeliculaDto> CreateAsync(CreatePeliculaDto dto)
        {
            var pelicula = new PeliculaModel
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Duracion = dto.Duracion,
                Anio = dto.AnioEstreno,
                Director = dto.Director,
                PosterUrl = dto.Imagen,
                PeliculaGeneros = dto.GeneroIds
                    .Select(gId => new PeliculaGeneroModel { GeneroId = gId })
                    .ToList()
            };

            _context.Peliculas.Add(pelicula);
            await _context.SaveChangesAsync();

            await _context.Entry(pelicula)
                .Collection(p => p.PeliculaGeneros).Query()
                .Include(pg => pg.Genero).LoadAsync();

            return pelicula.Adapt<PeliculaDto>();
        }

        public async Task<PeliculaDto?> UpdateAsync(int id, UpdatePeliculaDto dto)
        {
            var pelicula = await _context.Peliculas
                .Include(p => p.PeliculaGeneros)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pelicula is null) return null;

            pelicula.Titulo = dto.Titulo;
            pelicula.Descripcion = dto.Descripcion;
            pelicula.Duracion = dto.Duracion;
            pelicula.Anio = dto.AnioEstreno;
            pelicula.Director = dto.Director;
            pelicula.PosterUrl = dto.Imagen;

            _context.PeliculaGeneros.RemoveRange(pelicula.PeliculaGeneros);
            pelicula.PeliculaGeneros = dto.GeneroIds
                .Select(gId => new PeliculaGeneroModel { PeliculaId = id, GeneroId = gId })
                .ToList();

            await _context.SaveChangesAsync();

            await _context.Entry(pelicula)
                .Collection(p => p.PeliculaGeneros).Query()
                .Include(pg => pg.Genero).LoadAsync();

            return pelicula.Adapt<PeliculaDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pelicula = await _context.Peliculas.FindAsync(id);
            if (pelicula is null) return false;

            _context.Peliculas.Remove(pelicula);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
