using FluentValidation;
using MovieHubAPI.DTOs.Pelicula;

namespace MovieHubAPI.Validators.Pelicula;

public class CreatePeliculaValidator : AbstractValidator<CreatePeliculaDto>
{
    public CreatePeliculaValidator()
    {
        RuleFor(p => p.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(p => p.Director).NotEmpty().MaximumLength(150);
        RuleFor(p => p.Duracion).GreaterThan(0);
        RuleFor(p => p.AnioEstreno).InclusiveBetween(1888, 2030);
        RuleFor(p => p.Imagen).MaximumLength(500);
        RuleFor(p => p.GeneroIds)
            .NotEmpty().WithMessage("La película debe tener al menos un género.");
        RuleForEach(p => p.GeneroIds).GreaterThan(0);
    }
}