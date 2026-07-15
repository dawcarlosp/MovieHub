using Microsoft.EntityFrameworkCore;
using MovieHubAPI.DTOs.Favorito;
using MovieHubAPI.Services;

namespace MovieHubAPI.Tests.Unitarias.Services;

public class FavoritoServiceTests
{
    private MovieHubDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MovieHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MovieHubDbContext(options);
    }

    [Fact]
    public async Task AddAsync_FavoritoNuevo_RetornaTrue()
    {
        var context = CreateContext();
        var service = new FavoritoService(context);

        context.Peliculas.Add(new PeliculaModel { Titulo = "Test", Anio = 2020 });
        await context.SaveChangesAsync();

        var result = await service.AddAsync(1, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task AddAsync_FavoritoDuplicado_RetornaFalse()
    {
        var context = CreateContext();
        var service = new FavoritoService(context);

        context.Peliculas.Add(new PeliculaModel { Titulo = "Test", Anio = 2020 });
        await context.SaveChangesAsync();
        await service.AddAsync(1, 1);

        var result = await service.AddAsync(1, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveAsync_FavoritoExistente_RetornaTrue()
    {
        var context = CreateContext();
        var service = new FavoritoService(context);

        context.Peliculas.Add(new PeliculaModel { Titulo = "Test", Anio = 2020 });
        await context.SaveChangesAsync();
        await service.AddAsync(1, 1);

        var result = await service.RemoveAsync(1, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task RemoveAsync_FavoritoNoExistente_RetornaFalse()
    {
        var context = CreateContext();
        var service = new FavoritoService(context);

        var result = await service.RemoveAsync(999, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ConFavoritos_RetornaLista()
    {
        var context = CreateContext();
        var service = new FavoritoService(context);

        context.Peliculas.Add(new PeliculaModel { Titulo = "Test", Anio = 2020, PuntuacionMedia = 4.5 });
        await context.SaveChangesAsync();
        await service.AddAsync(1, 1);

        var result = await service.GetByUserIdAsync(1);

        Assert.Single(result);
        Assert.Equal("Test", result[0].Titulo);
        Assert.Equal(4.5, result[0].PuntuacionMedia);
    }

    [Fact]
    public async Task GetByUserIdAsync_SinFavoritos_RetornaListaVacia()
    {
        var context = CreateContext();
        var service = new FavoritoService(context);

        var result = await service.GetByUserIdAsync(1);

        Assert.Empty(result);
    }
}
