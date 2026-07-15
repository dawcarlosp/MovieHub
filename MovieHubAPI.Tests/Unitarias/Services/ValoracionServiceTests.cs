using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieHubAPI.DTOs.Valoracion;
using MovieHubAPI.Services;

namespace MovieHubAPI.Tests.Unitarias.Services;

public class ValoracionServiceTests
{
    private MovieHubDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MovieHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MovieHubDbContext(options);
    }

    private async Task SeedPeliculaAsync(MovieHubDbContext context)
    {
        context.Peliculas.Add(new PeliculaModel { Titulo = "Test", Anio = 2020 });
        await context.SaveChangesAsync();
    }

    private async Task SeedUserAsync(MovieHubDbContext context, long id, string email)
    {
        var user = new UsuarioModel
        {
            Id = id,
            UserName = email,
            NormalizedUserName = email.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
        };
        user.PasswordHash = new PasswordHasher<UsuarioModel>().HashPassword(user, "Test123!");
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAsync_ValoracionValida_RetornaValoracion()
    {
        var context = CreateContext();
        var service = new ValoracionService(context);

        await SeedUserAsync(context, 1, "user1@test.com");
        await SeedPeliculaAsync(context);

        var dto = new CreateValoracionDto(1, 4);

        var result = await service.CreateAsync(dto, 1);

        Assert.NotNull(result);
        Assert.Equal(4, result.Puntuacion);
        Assert.Equal("user1@test.com", result.UsuarioEmail);
    }

    [Fact]
    public async Task CreateAsync_ActualizaPuntuacionMedia()
    {
        var context = CreateContext();
        var service = new ValoracionService(context);

        await SeedUserAsync(context, 1, "user1@test.com");
        await SeedUserAsync(context, 2, "user2@test.com");
        await SeedPeliculaAsync(context);

        await service.CreateAsync(new CreateValoracionDto(1, 4), 1);
        await service.CreateAsync(new CreateValoracionDto(1, 2), 2);

        var pelicula = await context.Peliculas.FindAsync(1);
        Assert.NotNull(pelicula);
        Assert.Equal(3.0, pelicula.PuntuacionMedia);
    }

    [Fact]
    public async Task UpdateAsync_ValoracionExistente_ActualizaPuntuacion()
    {
        var context = CreateContext();
        var service = new ValoracionService(context);

        await SeedUserAsync(context, 1, "user1@test.com");
        await SeedPeliculaAsync(context);
        var creada = await service.CreateAsync(new CreateValoracionDto(1, 3), 1);

        var result = await service.UpdateAsync(creada.Id, 5, 1);

        Assert.NotNull(result);
        Assert.Equal(5, result.Puntuacion);
    }

    [Fact]
    public async Task UpdateAsync_ValoracionNoExistente_RetornaNull()
    {
        var context = CreateContext();
        var service = new ValoracionService(context);

        await SeedUserAsync(context, 1, "user1@test.com");

        var result = await service.UpdateAsync(999, 5, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ValoracionExistente_RetornaTrue()
    {
        var context = CreateContext();
        var service = new ValoracionService(context);

        await SeedUserAsync(context, 1, "user1@test.com");
        await SeedPeliculaAsync(context);
        var creada = await service.CreateAsync(new CreateValoracionDto(1, 3), 1);

        var result = await service.DeleteAsync(creada.Id, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ValoracionNoExistente_RetornaFalse()
    {
        var context = CreateContext();
        var service = new ValoracionService(context);

        var result = await service.DeleteAsync(999, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_RecalculaPuntuacionMedia()
    {
        var context = CreateContext();
        var service = new ValoracionService(context);

        await SeedUserAsync(context, 1, "user1@test.com");
        await SeedUserAsync(context, 2, "user2@test.com");
        await SeedPeliculaAsync(context);
        var v1 = await service.CreateAsync(new CreateValoracionDto(1, 5), 1);
        var v2 = await service.CreateAsync(new CreateValoracionDto(1, 1), 2);

        await service.DeleteAsync(v1.Id, 1);

        var pelicula = await context.Peliculas.FindAsync(1);
        Assert.NotNull(pelicula);
        Assert.Equal(1.0, pelicula.PuntuacionMedia);
    }

    [Fact]
    public async Task GetByPeliculaIdAsync_RetornaValoraciones()
    {
        var context = CreateContext();
        var service = new ValoracionService(context);

        await SeedUserAsync(context, 1, "user1@test.com");
        await SeedUserAsync(context, 2, "user2@test.com");
        await SeedPeliculaAsync(context);
        await service.CreateAsync(new CreateValoracionDto(1, 4), 1);
        await service.CreateAsync(new CreateValoracionDto(1, 5), 2);

        var result = await service.GetByPeliculaIdAsync(1);

        Assert.Equal(2, result.Count);
    }
}
