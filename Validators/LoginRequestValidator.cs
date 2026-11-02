using FluentValidation;
using LibraryApi.DTOs.Auth;

namespace LibraryApi.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    // Only presence is checked: format and strength rules would hint at what a valid credential looks like.
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("Email tidak boleh kosong.");

        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("Password tidak boleh kosong.");
    }
}
