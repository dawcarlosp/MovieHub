using Mapster;
using MovieHubAPI.DTOs.Genero;
using MovieHubAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace MovieHubAPI.Services
{
    public class GeneroService : IGeneroService
    {
        private readonly MovieHubDbContext _context;

        public GeneroService(MovieHubDbContext context) => _context = context;

        public async Task<List<GeneroDto>> GetAllAsync()
        {
            var generos = await _context.Generos.ToListAsync();
            return generos.Adapt<List<GeneroDto>>();
        }

        public async Task<GeneroDto?> GetByIdAsync(int id)
        {
            var genero = await _context.Generos.FindAsync(id);
            return genero?.Adapt<GeneroDto>();
        }

        public async Task<GeneroDto> CreateAsync(CreateGeneroDto dto)
        {
            var genero = dto.Adapt<GeneroModel>();
            _context.Generos.Add(genero);
            await _context.SaveChangesAsync();
            return genero.Adapt<GeneroDto>();
        }

        public async Task<GeneroDto?> UpdateAsync(int id, UpdateGeneroDto dto)
        {
            var genero = await _context.Generos.FindAsync(id);
            if (genero is null) return null;

            genero.Nombre = dto.Nombre;
            await _context.SaveChangesAsync();
            return genero.Adapt<GeneroDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var genero = await _context.Generos.FindAsync(id);
            if (genero is null) return false;

            _context.Generos.Remove(genero);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
