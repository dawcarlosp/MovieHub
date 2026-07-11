using FluentValidation;
using MovieHubAPI.DTOs.Genero;

namespace MovieHubAPI.Validators.Genero;

public class UpdateGeneroValidator : AbstractValidator<UpdateGeneroDto>
{
    public UpdateGeneroValidator()
    {
        RuleFor(g => g.Nombre).NotEmpty().MaximumLength(50);
    }
}