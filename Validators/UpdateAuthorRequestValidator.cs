using FluentValidation;
using LibraryApi.DTOs.Authors;

namespace LibraryApi.Validators;

public class UpdateAuthorRequestValidator : AbstractValidator<UpdateAuthorRequest>
{
    private const int MaxNameLength = 150;

    public UpdateAuthorRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Nama penulis tidak boleh kosong.")
            .MaximumLength(MaxNameLength).WithMessage($"Nama penulis maksimal {MaxNameLength} karakter.");
    }
}
