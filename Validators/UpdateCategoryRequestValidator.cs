using FluentValidation;
using LibraryApi.DTOs.Categories;

namespace LibraryApi.Validators;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    private const int MaxNameLength = 100;

    public UpdateCategoryRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Nama kategori tidak boleh kosong.")
            .MaximumLength(MaxNameLength).WithMessage($"Nama kategori maksimal {MaxNameLength} karakter.");
    }
}
