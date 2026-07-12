using FluentValidation;
using MovieHubAPI.DTOs.Usuario;

namespace MovieHubAPI.Validators.Usuario;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(r => r.UserName).NotEmpty().MaximumLength(50);
        RuleFor(r => r.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(r => r.Password)
            .NotEmpty().MinimumLength(6)
            .Matches("[A-Z]").WithMessage("Password debe contener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("Password debe contener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("Password debe contener al menos un dígito.");
    }
}
