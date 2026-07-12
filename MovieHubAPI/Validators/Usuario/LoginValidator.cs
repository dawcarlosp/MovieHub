using FluentValidation;
using MovieHubAPI.DTOs.Usuario;

namespace MovieHubAPI.Validators.Usuario;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(l => l.Email).NotEmpty().EmailAddress();
        RuleFor(l => l.Password).NotEmpty();
    }
}
