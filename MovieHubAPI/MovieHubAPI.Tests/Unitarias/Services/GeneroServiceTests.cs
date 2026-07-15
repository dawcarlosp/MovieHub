using Mapster;
using Microsoft.EntityFrameworkCore;
using MovieHubAPI.Configurations;
using MovieHubAPI.DTOs.Genero;
using MovieHubAPI.Services;

namespace MovieHubAPI.Tests.Unitarias.Services;

public class GeneroServiceTests
{
    private readonly MovieHubDbContext _context;
    private readonly GeneroService _service;

    public GeneroServiceTests()
    {
        var options = new DbContextOptionsBuilder<MovieHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MovieHubDbContext(options);
        MappingConfig.Configure();
        _service = new GeneroService(_context);
    }

    [Fact]
    public async Task GetAllAsync_CuandoExistenGeneros_RetornaLista()
    {
        _context.Generos.Add(new GeneroModel { Nombre = "Acción" });
        _context.Generos.Add(new GeneroModel { Nombre = "Comedia" });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Nombre == "Acción");
        Assert.Contains(result, g => g.Nombre == "Comedia");
    }

    [Fact]
    public async Task GetAllAsync_SinGeneros_RetornaListaVacia()
    {
        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_CuandoExiste_RetornaGenero()
    {
        var genero = new GeneroModel { Nombre = "Terror" };
        _context.Generos.Add(genero);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(genero.Id);

        Assert.NotNull(result);
        Assert.Equal("Terror", result.Nombre);
    }

    [Fact]
    public async Task GetByIdAsync_CuandoNoExiste_RetornaNull()
    {
        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ConDatosValidos_RetornaGeneroCreado()
    {
        var dto = new CreateGeneroDto("Drama");

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Drama", result.Nombre);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task CreateAsync_GuardaEnBD()
    {
        await _service.CreateAsync(new CreateGeneroDto("Drama"));

        var guardado = await _context.Generos.FirstOrDefaultAsync(g => g.Nombre == "Drama");
        Assert.NotNull(guardado);
    }

    [Fact]
    public async Task UpdateAsync_CuandoExiste_ActualizaNombre()
    {
        var genero = new GeneroModel { Nombre = "Viejo" };
        _context.Generos.Add(genero);
        await _context.SaveChangesAsync();

        var dto = new UpdateGeneroDto("Nuevo");

        var result = await _service.UpdateAsync(genero.Id, dto);

        Assert.NotNull(result);
        Assert.Equal("Nuevo", result.Nombre);
    }

    [Fact]
    public async Task UpdateAsync_CuandoNoExiste_RetornaNull()
    {
        var dto = new UpdateGeneroDto("Test");

        var result = await _service.UpdateAsync(999, dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_CuandoExiste_RetornaTrue()
    {
        _context.Generos.Add(new GeneroModel { Nombre = "Test" });
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
        Assert.Equal(0, await _context.Generos.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_CuandoNoExiste_RetornaFalse()
    {
        var result = await _service.DeleteAsync(999);

        Assert.False(result);
    }
}
