namespace LibraryApi.DTOs.Books;

public record UpdateBookRequest(
    string Title,
    string Isbn,
    int Stock,
    int PublishedYear,
    int CategoryId,
    IReadOnlyList<int> AuthorIds,
    int PrimaryAuthorId) : IBookRequest;
