using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieHubAPI.Controllers;
using MovieHubAPI.DTOs;
using MovieHubAPI.DTOs.Pelicula;
using MovieHubAPI.Interfaces;

namespace MovieHubAPI.Tests.Integracion;

public class PeliculasControllerTests
{
    private readonly Mock<IPeliculaService> _mockService;
    private readonly PeliculasController _controller;

    public PeliculasControllerTests()
    {
        _mockService = new Mock<IPeliculaService>();
        _controller = new PeliculasController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_RetornaOkConPaginados()
    {
        var paginados = new PaginadosDto<PeliculaDto>(
            new List<PeliculaDto> { new(1, "Test", "", 90, 2020, "Dir", null, 4.0, new List<string>()) },
            1, 10, 1, 1
        );
        _mockService.Setup(s => s.GetAllPaginadoAsync(1, 10, null, null, null))
            .ReturnsAsync(paginados);

        var result = await _controller.GetAll(1, 10, null, null, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsType<PaginadosDto<PeliculaDto>>(okResult.Value);
        Assert.Single(data.Items);
    }

    [Fact]
    public async Task GetById_CuandoExiste_RetornaOk()
    {
        var pelicula = new PeliculaDto(1, "Test", "", 90, 2020, "Dir", null, 4.0, new List<string>());
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(pelicula);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsType<PeliculaDto>(okResult.Value);
        Assert.Equal("Test", data.Titulo);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_RetornaNotFound()
    {
        _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((PeliculaDto?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_RetornaCreatedAtAction()
    {
        var dto = new CreatePeliculaDto("Nueva", "", 90, 2024, "Dir", null, new List<int> { 1 });
        var creada = new PeliculaDto(1, "Nueva", "", 90, 2024, "Dir", null, 0, new List<string>());
        _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(creada);

        var result = await _controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(1, ((PeliculaDto)createdResult.Value!).Id);
    }

    [Fact]
    public async Task Update_CuandoExiste_RetornaOk()
    {
        var dto = new UpdatePeliculaDto("Modificada", "", 120, 2024, "Dir2", null, new List<int>());
        var actualizada = new PeliculaDto(1, "Modificada", "", 120, 2024, "Dir2", null, 0, new List<string>());
        _mockService.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(actualizada);

        var result = await _controller.Update(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsType<PeliculaDto>(okResult.Value);
        Assert.Equal("Modificada", data.Titulo);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_RetornaNotFound()
    {
        var dto = new UpdatePeliculaDto("Test", "", 90, 2020, "Dir", null, new List<int>());
        _mockService.Setup(s => s.UpdateAsync(999, dto)).ReturnsAsync((PeliculaDto?)null);

        var result = await _controller.Update(999, dto);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_CuandoExiste_RetornaNoContent()
    {
        _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_CuandoNoExiste_RetornaNotFound()
    {
        _mockService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
