using Mapster;
using Microsoft.EntityFrameworkCore;
using MovieHubAPI.Configurations;
using MovieHubAPI.DTOs.Pelicula;
using MovieHubAPI.Services;
using MovieHubAPI.DTOs;

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
    public async Task GetByIdAsync_CuandoExiste_RetornaPelicula()
    {
        var pelicula = new PeliculaModel { Titulo = "Test", Anio = 2020 };
        _context.Peliculas.Add(pelicula);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(pelicula.Id);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Titulo);
    }

    [Fact]
    public async Task GetByIdAsync_CuandoNoExiste_RetornaNull()
    {
        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_CuandoExiste_IncluyeGeneros()
    {
        var genero = new GeneroModel { Nombre = "Acción" };
        _context.Generos.Add(genero);
        await _context.SaveChangesAsync();

        var pelicula = new PeliculaModel { Titulo = "Test", Anio = 2020 };
        pelicula.PeliculaGeneros.Add(new PeliculaGeneroModel { GeneroId = genero.Id });
        _context.Peliculas.Add(pelicula);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(pelicula.Id);

        Assert.NotNull(result);
        Assert.Contains("Acción", result.Generos);
    }

    [Fact]
    public async Task CreateAsync_ConDatosValidos_RetornaPeliculaCreada()
    {
        var dto = new CreatePeliculaDto("Nueva Peli", "Descripción", 120, 2024, "Director", null, new List<int>());

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Nueva Peli", result.Titulo);
        Assert.Equal(2024, result.AnioEstreno);
        Assert.Equal(120, result.Duracion);
        Assert.Equal("Director", result.Director);
        Assert.Equal(1, await _context.Peliculas.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_ConGeneros_AsociaGeneros()
    {
        var genero = new GeneroModel { Nombre = "Acción" };
        _context.Generos.Add(genero);
        await _context.SaveChangesAsync();

        var dto = new CreatePeliculaDto("Test", "", 90, 2020, "Dir", null, new List<int> { genero.Id });

        var result = await _service.CreateAsync(dto);

        Assert.Single(result.Generos);
        Assert.Contains("Acción", result.Generos);
    }

    [Fact]
    public async Task CreateAsync_GuardaPeliculaEnBD()
    {
        var dto = new CreatePeliculaDto("Test", "", 90, 2020, "Dir", null, new List<int>());

        await _service.CreateAsync(dto);

        var guardada = await _context.Peliculas.FirstOrDefaultAsync(p => p.Titulo == "Test");
        Assert.NotNull(guardada);
        Assert.Equal(2020, guardada.Anio);
    }

    [Fact]
    public async Task UpdateAsync_CuandoExiste_ActualizaCampos()
    {
        var pelicula = new PeliculaModel { Titulo = "Original", Anio = 2020, Duracion = 90, Director = "Dir" };
        _context.Peliculas.Add(pelicula);
        await _context.SaveChangesAsync();

        var dto = new UpdatePeliculaDto("Modificada", "Nueva desc", 120, 2024, "Nuevo Dir", null, new List<int>());

        var result = await _service.UpdateAsync(pelicula.Id, dto);

        Assert.NotNull(result);
        Assert.Equal("Modificada", result.Titulo);
        Assert.Equal(2024, result.AnioEstreno);
        Assert.Equal(120, result.Duracion);
        Assert.Equal("Nuevo Dir", result.Director);
    }

    [Fact]
    public async Task UpdateAsync_CuandoNoExiste_RetornaNull()
    {
        var dto = new UpdatePeliculaDto("Test", "", 90, 2020, "Dir", null, new List<int>());

        var result = await _service.UpdateAsync(999, dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ActualizaGeneros()
    {
        var generoViejo = new GeneroModel { Nombre = "Viejo" };
        var generoNuevo = new GeneroModel { Nombre = "Nuevo" };
        _context.Generos.AddRange(generoViejo, generoNuevo);
        await _context.SaveChangesAsync();

        var pelicula = new PeliculaModel { Titulo = "Test", Anio = 2020 };
        pelicula.PeliculaGeneros.Add(new PeliculaGeneroModel { GeneroId = generoViejo.Id });
        _context.Peliculas.Add(pelicula);
        await _context.SaveChangesAsync();

        var dto = new UpdatePeliculaDto("Test", "", 90, 2020, "Dir", null, new List<int> { generoNuevo.Id });

        var result = await _service.UpdateAsync(pelicula.Id, dto);

        Assert.Single(result.Generos);
        Assert.Contains("Nuevo", result.Generos);
        Assert.DoesNotContain("Viejo", result.Generos);
    }

    [Fact]
    public async Task DeleteAsync_CuandoExiste_RetornaTrue()
    {
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Test", Anio = 2020 });
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
        Assert.Equal(0, await _context.Peliculas.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_CuandoNoExiste_RetornaFalse()
    {
        var result = await _service.DeleteAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_EliminaPeliculaCorrectamente()
    {
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Test", Anio = 2020 });
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Otra", Anio = 2021 });
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(1);

        Assert.Single(await _context.Peliculas.ToListAsync());
        Assert.Equal("Otra", (await _context.Peliculas.FirstAsync()).Titulo);
    }

    [Fact]
    public async Task GetMejorValoradasAsync_RetornaTop10PorPuntuacion()
    {
        for (int i = 0; i < 15; i++)
            _context.Peliculas.Add(new PeliculaModel { Titulo = $"Peli {i}", PuntuacionMedia = i, Anio = 2020 });

        await _context.SaveChangesAsync();

        var result = await _service.GetMejorValoradasAsync();

        Assert.Equal(10, result.Count);
        Assert.All(result, p => Assert.True(p.PuntuacionMedia > 0));
    }

    [Fact]
    public async Task GetMejorValoradasAsync_SinPeliculasValoradas_RetornaListaVacia()
    {
        for (int i = 0; i < 5; i++)
            _context.Peliculas.Add(new PeliculaModel { Titulo = $"Peli {i}", PuntuacionMedia = 0, Anio = 2020 });

        await _context.SaveChangesAsync();

        var result = await _service.GetMejorValoradasAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMasRecientesAsync_RetornaTop10PorAnio()
    {
        for (int i = 0; i < 15; i++)
            _context.Peliculas.Add(new PeliculaModel { Titulo = $"Peli {i}", Anio = 2000 + i });

        await _context.SaveChangesAsync();

        var result = await _service.GetMasRecientesAsync();

        Assert.Equal(10, result.Count);
        Assert.Equal(2014, result[0].AnioEstreno);
    }

    [Fact]
    public async Task GetEstadisticasAsync_RetornaDatosCorrectos()
    {
        var genero = new GeneroModel { Nombre = "Test" };
        _context.Generos.Add(genero);
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Peli 1", Anio = 2020 });
        _context.Peliculas.Add(new PeliculaModel { Titulo = "Peli 2", Anio = 2021 });
        _context.Valoraciones.Add(new ValoracionModel { UsuarioId = 1, PeliculaId = 1, Puntuacion = 4 });
        await _context.SaveChangesAsync();

        var result = await _service.GetEstadisticasAsync();

        Assert.Equal(2, result.TotalPeliculas);
        Assert.Equal(1, result.TotalGeneros);
        Assert.Equal(1, result.TotalValoraciones);
        Assert.Equal(4.0, result.PuntuacionMediaGlobal);
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
