using FluentValidation;
using LibraryApi.DTOs.Auth;

namespace LibraryApi.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty().WithMessage("Refresh token tidak boleh kosong.");
    }
}
