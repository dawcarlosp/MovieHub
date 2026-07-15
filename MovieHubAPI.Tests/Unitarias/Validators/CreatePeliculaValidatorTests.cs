using FluentValidation.TestHelper;
using MovieHubAPI.DTOs.Pelicula;
using MovieHubAPI.Validators.Pelicula;

namespace MovieHubAPI.Tests.Unitarias.Validators;

public class CreatePeliculaValidatorTests
{
    private readonly CreatePeliculaValidator _validator = new();

    [Fact]
    public void TituloVacio_RetornaError()
    {
        var dto = new CreatePeliculaDto("", "Desc", 90, 2020, "Dir", null, new List<int> { 1 });

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(p => p.Titulo);
    }

    [Fact]
    public void DuracionCero_RetornaError()
    {
        var dto = new CreatePeliculaDto("Test", "Desc", 0, 2020, "Dir", null, new List<int> { 1 });

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(p => p.Duracion);
    }

    [Fact]
    public void AnioEstrenoFuturo_RetornaError()
    {
        var dto = new CreatePeliculaDto("Test", "Desc", 90, 2100, "Dir", null, new List<int> { 1 });

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(p => p.AnioEstreno);
    }

    [Fact]
    public void SinGeneros_RetornaError()
    {
        var dto = new CreatePeliculaDto("Test", "Desc", 90, 2020, "Dir", null, new List<int>());

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(p => p.GeneroIds);
    }

    [Fact]
    public void DatosValidos_NoRetornaError()
    {
        var dto = new CreatePeliculaDto("Test", "Desc", 90, 2020, "Dir", null, new List<int> { 1 });

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
