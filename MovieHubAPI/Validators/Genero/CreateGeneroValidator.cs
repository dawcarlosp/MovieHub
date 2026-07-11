using FluentValidation;
using MovieHubAPI.DTOs.Genero;

namespace MovieHubAPI.Validators.Genero;

public class CreateGeneroValidator : AbstractValidator<CreateGeneroDto>
{
    public CreateGeneroValidator()
    {
        RuleFor(g => g.Nombre).NotEmpty().MaximumLength(50);
    }
}