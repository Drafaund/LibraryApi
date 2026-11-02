using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using LibraryApi.Common;
using LibraryApi.DTOs.Books;
using LibraryApi.Models;
using LibraryApi.Repositories;

namespace LibraryApi.Services;

public class BookService : IBookService
{
    private const string NotFoundMessage = "Buku tidak ditemukan.";
    private const string IsbnTakenMessage = "ISBN sudah digunakan.";
    private const string CategoryNotFoundMessage = "Kategori tidak ditemukan.";
    private const string AuthorNotFoundMessage = "Satu atau lebih penulis tidak ditemukan.";
    private const string EmptySearchMessage = "Judul yang dicari tidak boleh kosong.";

    private readonly IBookRepository _books;
    private readonly ICategoryRepository _categories;
    private readonly IAuthorRepository _authors;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateBookRequest> _createValidator;
    private readonly IValidator<UpdateBookRequest> _updateValidator;

    public BookService(
        IBookRepository books,
        ICategoryRepository categories,
        IAuthorRepository authors,
        IMapper mapper,
        IValidator<CreateBookRequest> createValidator,
        IValidator<UpdateBookRequest> updateValidator)
    {
        _books = books;
        _categories = categories;
        _authors = authors;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ServiceResult<IReadOnlyList<BookListItemResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var books = await _books.ListWithDetailsAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<BookListItemResponse>>.Ok(
            _mapper.Map<IReadOnlyList<BookListItemResponse>>(books));
    }

    public async Task<ServiceResult<IReadOnlyList<BookListItemResponse>>> SearchByTitleAsync(
        string title,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return ServiceResult<IReadOnlyList<BookListItemResponse>>.Fail(
                ErrorType.Validation, EmptySearchMessage);
        }

        var books = await _books.SearchByTitleAsync(title.Trim(), cancellationToken);

        return ServiceResult<IReadOnlyList<BookListItemResponse>>.Ok(
            _mapper.Map<IReadOnlyList<BookListItemResponse>>(books));
    }

    public async Task<ServiceResult<BookResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var book = await _books.GetWithDetailsAsync(id, cancellationToken);
        if (book is null)
            return ServiceResult<BookResponse>.Fail(ErrorType.NotFound, NotFoundMessage);

        return ServiceResult<BookResponse>.Ok(_mapper.Map<BookResponse>(book));
    }

    public async Task<ServiceResult<BookResponse>> CreateAsync(
        CreateBookRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ServiceResult<BookResponse>.ValidationFail(validation);

        var normalized = request with { Title = request.Title.Trim(), Isbn = request.Isbn.Trim() };

        var violation = await FindViolationAsync(normalized, null, cancellationToken);
        if (violation is not null)
            return violation;

        var book = _mapper.Map<Book>(normalized);
        book.BookAuthors = normalized.AuthorIds
            .Select(authorId => new BookAuthor { AuthorId = authorId, IsPrimary = authorId == normalized.PrimaryAuthorId })
            .ToList();

        await _books.AddAsync(book, cancellationToken);
        await _books.SaveChangesAsync(cancellationToken);

        return await LoadResponseAsync(book.Id, cancellationToken);
    }

    public async Task<ServiceResult<BookResponse>> UpdateAsync(
        int id,
        UpdateBookRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ServiceResult<BookResponse>.ValidationFail(validation);

        var book = await _books.GetForUpdateAsync(id, cancellationToken);
        if (book is null)
            return ServiceResult<BookResponse>.Fail(ErrorType.NotFound, NotFoundMessage);

        var normalized = request with { Title = request.Title.Trim(), Isbn = request.Isbn.Trim() };

        var violation = await FindViolationAsync(normalized, id, cancellationToken);
        if (violation is not null)
            return violation;

        _mapper.Map(normalized, book);
        SyncBookAuthors(book, normalized);
        await _books.SaveChangesAsync(cancellationToken);

        return await LoadResponseAsync(book.Id, cancellationToken);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var book = await _books.GetByIdAsync(id, cancellationToken);
        if (book is null)
            return ServiceResult.Fail(ErrorType.NotFound, NotFoundMessage);

        // The BookAuthors rows are removed by the cascade delete.
        _books.Remove(book);
        await _books.SaveChangesAsync(cancellationToken);

        return ServiceResult.Ok();
    }

    // Rules that need the database: the Category and Authors must exist and the ISBN must be unique.
    // A missing Category or Author is a 400 on a field of the body, not a 404, because the URL itself is valid.
    private async Task<ServiceResult<BookResponse>?> FindViolationAsync(
        IBookRequest request,
        int? excludeBookId,
        CancellationToken cancellationToken)
    {
        if (await _categories.GetByIdAsync(request.CategoryId, cancellationToken) is null)
            return FailField(nameof(IBookRequest.CategoryId), CategoryNotFoundMessage);

        var existingAuthors = await _authors.CountExistingAsync(request.AuthorIds, cancellationToken);
        if (existingAuthors != request.AuthorIds.Count)
            return FailField(nameof(IBookRequest.AuthorIds), AuthorNotFoundMessage);

        if (await _books.IsbnExistsAsync(request.Isbn, excludeBookId, cancellationToken))
            return ServiceResult<BookResponse>.Fail(ErrorType.Conflict, IsbnTakenMessage);

        return null;
    }

    // Applied as a diff in one SaveChanges: removing then re-adding a row with the same key makes EF throw.
    private static void SyncBookAuthors(Book book, IBookRequest request)
    {
        var requestedIds = request.AuthorIds.ToHashSet();

        foreach (var removed in book.BookAuthors.Where(link => !requestedIds.Contains(link.AuthorId)).ToList())
            book.BookAuthors.Remove(removed);

        foreach (var kept in book.BookAuthors)
            kept.IsPrimary = kept.AuthorId == request.PrimaryAuthorId;

        var keptIds = book.BookAuthors.Select(link => link.AuthorId).ToHashSet();
        foreach (var authorId in requestedIds.Where(authorId => !keptIds.Contains(authorId)))
        {
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId, IsPrimary = authorId == request.PrimaryAuthorId });
        }
    }

    private async Task<ServiceResult<BookResponse>> LoadResponseAsync(int id, CancellationToken cancellationToken)
    {
        var saved = await _books.GetWithDetailsAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Book {id} was not found right after it was saved.");

        return ServiceResult<BookResponse>.Ok(_mapper.Map<BookResponse>(saved));
    }

    private static ServiceResult<BookResponse> FailField(string property, string message)
    {
        var failure = new ValidationResult(new[] { new ValidationFailure(property, message) });
        return ServiceResult<BookResponse>.ValidationFail(failure);
    }
}
