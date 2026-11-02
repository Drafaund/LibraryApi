using FluentValidation;
using LibraryApi.DTOs.Categories;

namespace LibraryApi.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    private const int MaxNameLength = 100;

    public CreateCategoryRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Nama kategori tidak boleh kosong.")
            .MaximumLength(MaxNameLength).WithMessage($"Nama kategori maksimal {MaxNameLength} karakter.");
    }
}
