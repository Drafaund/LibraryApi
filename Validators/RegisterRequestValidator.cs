using FluentValidation;
using LibraryApi.DTOs.Auth;

namespace LibraryApi.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    // Matches the Identity column length for email.
    private const int MaxEmailLength = 256;

    // Password strength is enforced by Identity's PasswordOptions, not duplicated here.
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("Email tidak boleh kosong.")
            .MaximumLength(MaxEmailLength).WithMessage($"Email maksimal {MaxEmailLength} karakter.")
            .EmailAddress().WithMessage("Format email tidak valid.");

        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("Password tidak boleh kosong.");
    }
}
