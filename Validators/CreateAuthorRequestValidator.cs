using FluentValidation;
using LibraryApi.DTOs.Authors;

namespace LibraryApi.Validators;

public class CreateAuthorRequestValidator : AbstractValidator<CreateAuthorRequest>
{
    private const int MaxNameLength = 150;

    public CreateAuthorRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Nama penulis tidak boleh kosong.")
            .MaximumLength(MaxNameLength).WithMessage($"Nama penulis maksimal {MaxNameLength} karakter.");
    }
}
