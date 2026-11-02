using FluentValidation;
using LibraryApi.DTOs.Books;

namespace LibraryApi.Validators;

public abstract class BookRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : IBookRequest
{
    private const int MaxTitleLength = 200;
    private const int MaxIsbnLength = 20;
    private const int MinPublishedYear = 1000;

    protected BookRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty().WithMessage("Judul tidak boleh kosong.")
            .MaximumLength(MaxTitleLength).WithMessage($"Judul maksimal {MaxTitleLength} karakter.");

        RuleFor(request => request.Isbn)
            .NotEmpty().WithMessage("ISBN tidak boleh kosong.")
            .MaximumLength(MaxIsbnLength).WithMessage($"ISBN maksimal {MaxIsbnLength} karakter.");

        RuleFor(request => request.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok tidak boleh negatif.");

        RuleFor(request => request.PublishedYear)
            .Must(year => year >= MinPublishedYear && year <= DateTime.UtcNow.Year)
            .WithMessage($"Tahun terbit harus antara {MinPublishedYear} dan tahun sekarang.");

        RuleFor(request => request.CategoryId)
            .GreaterThan(0).WithMessage("Kategori wajib dipilih.");

        ConfigureAuthorRules();
    }

    private void ConfigureAuthorRules()
    {
        RuleFor(request => request.AuthorIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Buku harus memiliki minimal satu penulis.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Daftar penulis tidak boleh berisi duplikat.");

        // Skipped when the list is empty: that case is already reported by the AuthorIds rule.
        RuleFor(request => request.PrimaryAuthorId)
            .Must((request, primaryAuthorId) => request.AuthorIds.Contains(primaryAuthorId))
            .WithMessage("Penulis utama harus ada di daftar penulis.")
            .When(request => request.AuthorIds is { Count: > 0 });
    }
}
