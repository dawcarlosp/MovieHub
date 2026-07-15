using FluentValidation.TestHelper;
using MovieHubAPI.DTOs.Genero;
using MovieHubAPI.Validators.Genero;

namespace MovieHubAPI.Tests.Unitarias.Validators;

public class CreateGeneroValidatorTests
{
    private readonly CreateGeneroValidator _validator = new();

    [Fact]
    public void NombreVacio_RetornaError()
    {
        var dto = new CreateGeneroDto("");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(g => g.Nombre);
    }

    [Fact]
    public void NombreValido_NoRetornaError()
    {
        var dto = new CreateGeneroDto("Acción");

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
