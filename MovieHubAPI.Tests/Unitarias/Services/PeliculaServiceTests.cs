using Mapster;
using Microsoft.EntityFrameworkCore;
using MovieHubAPI.Configurations;
using MovieHubAPI.DTOs.Pelicula;
using MovieHubAPI.Services;

namespace MovieHubAPI.Tests.Unitarias.Services;

public class PeliculaServiceTests
{
    private readonly MovieHubDbContext _context;
    private readonly PeliculaService _service;

    public PeliculaServiceTests()
    {
        var options = new DbContextOptionsBuilder<MovieHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MovieHubDbContext(options);
        MappingConfig.Configure();
        _service = new PeliculaService(_context);
    }

    [Fact]
    public async Task GetAllPaginadoAsync_CuandoExistenPeliculas_RetornaPagina()
    {
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Test 1", Anio = 2020 });
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Test 2", Anio = 2021 });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllPaginadoAsync(1, 10, null, null, null);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetAllPaginadoAsync_CuandoNoHayPeliculas_RetornaPaginaVacia()
    {
        var result = await _service.GetAllPaginadoAsync(1, 10, null, null, null);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task GetAllPaginadoAsync_ConFiltroTitulo_RetornaSoloCoincidencias()
    {
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Batman Begins", Anio = 2005 });
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Batman Returns", Anio = 1992 });
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Superman", Anio = 1978 });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllPaginadoAsync(1, 10, "Batman", null, null);

        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetAllPaginadoAsync_ConFiltroGenero_RetornaSoloCoincidencias()
    {
        var genero = new GeneroModel { Nombre = "Acción" };
        _context.Generos.Add(genero);
        await _context.SaveChangesAsync();

        var peliConGenero = new PeliculaModel { Titulo = "Accion Total", Anio = 2020 };
        peliConGenero.PeliculaGeneros.Add(new PeliculaGeneroModel { GeneroId = genero.Id });
        _context.Peliculas.Add(peliConGenero);
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Sin género", Anio = 2021 });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllPaginadoAsync(1, 10, null, genero.Id, null);

        Assert.Single(result.Items);
        Assert.Equal("Accion Total", result.Items[0].Titulo);
    }

    [Fact]
    public async Task GetAllPaginadoAsync_ConOrdenPuntuacion_RetornaOrdenadoDescendente()
    {
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Baja", PuntuacionMedia = 2.0, Anio = 2020 });
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Alta", PuntuacionMedia = 5.0, Anio = 2020 });
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Media", PuntuacionMedia = 3.5, Anio = 2020 });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllPaginadoAsync(1, 10, null, null, "puntuacion");

        Assert.Equal("Alta", result.Items[0].Titulo);
        Assert.Equal("Media", result.Items[1].Titulo);
        Assert.Equal("Baja", result.Items[2].Titulo);
    }

    [Fact]
    public async Task GetAllPaginadoAsync_PaginaDos_RetornaSegundoLote()
    {
        for (int i = 0; i < 25; i++)
            _context.Peliculas.Add(new PeliculaModel { Titulo = $"Peli {i}", Anio = 2000 });

        await _context.SaveChangesAsync();

        var result = await _service.GetAllPaginadoAsync(2, 10, null, null, null);

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }
}
