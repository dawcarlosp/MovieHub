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

        public async Task<PaginadosDto<PeliculaDto>> GetAllPaginadoAsync(int page, int pageSize, string? titulo, int? generoId, string? orden)
        {
            var query = _context.Peliculas
                .Include(p => p.PeliculaGeneros).ThenInclude(pg => pg.Genero)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(titulo))
                query = query.Where(p => p.Titulo.Contains(titulo));

            if (generoId.HasValue)
                query = query.Where(p => p.PeliculaGeneros.Any(pg => pg.GeneroId == generoId.Value));

            query = orden switch
            {
                "puntuacion" => query.OrderByDescending(p => p.PuntuacionMedia),
                "anio" => query.OrderByDescending(p => p.Anio),
                _ => query.OrderBy(p => p.Titulo)
            };

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

        public async Task<PaginadosDto<PeliculaDto>> BuscarAsync(string? q, int? generoId, int? anioMin, int? anioMax, string? orden, int page, int pageSize)
        {
            var query = _context.Peliculas
                .Include(p => p.PeliculaGeneros).ThenInclude(pg => pg.Genero)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.ToLower();
                query = query.Where(p =>
                    p.Titulo.ToLower().Contains(term) ||
                    (p.Descripcion != null && p.Descripcion.ToLower().Contains(term)) ||
                    (p.Director != null && p.Director.ToLower().Contains(term))
                );
            }

            if (generoId.HasValue)
                query = query.Where(p => p.PeliculaGeneros.Any(pg => pg.GeneroId == generoId.Value));

            if (anioMin.HasValue)
                query = query.Where(p => p.Anio >= anioMin.Value);

            if (anioMax.HasValue)
                query = query.Where(p => p.Anio <= anioMax.Value);

            query = orden switch
            {
                "puntuacion" => query.OrderByDescending(p => p.PuntuacionMedia),
                "anio" => query.OrderByDescending(p => p.Anio),
                _ => query.OrderBy(p => p.Titulo)
            };

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

        public async Task<List<PeliculaDto>> GetMejorValoradasAsync()
        {
            return await _context.Peliculas
                .Include(p => p.PeliculaGeneros).ThenInclude(pg => pg.Genero)
                .Where(p => p.PuntuacionMedia > 0)
                .OrderByDescending(p => p.PuntuacionMedia)
                .Take(10)
                .ProjectToType<PeliculaDto>()
                .ToListAsync();
        }

        public async Task<List<PeliculaDto>> GetMasRecientesAsync()
        {
            return await _context.Peliculas
                .Include(p => p.PeliculaGeneros).ThenInclude(pg => pg.Genero)
                .OrderByDescending(p => p.Anio)
                .Take(10)
                .ProjectToType<PeliculaDto>()
                .ToListAsync();
        }

        public async Task<EstadisticasDto> GetEstadisticasAsync()
        {
            var totalPeliculas = await _context.Peliculas.CountAsync();
            var totalGeneros = await _context.Generos.CountAsync();
            var totalValoraciones = await _context.Valoraciones.CountAsync();
            var mediaGlobal = await _context.Valoraciones
                .AverageAsync(v => (double?)v.Puntuacion) ?? 0;

            return new EstadisticasDto(
                totalPeliculas,
                Math.Round(mediaGlobal, 1),
                totalGeneros,
                totalValoraciones
            );
        }
    }
}
