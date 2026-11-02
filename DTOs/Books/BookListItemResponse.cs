namespace LibraryApi.DTOs.Books;

public record BookListItemResponse(
    int Id,
    string Title,
    string Isbn,
    int Stock,
    int PublishedYear,
    string CategoryName,
    IReadOnlyList<string> AuthorNames);
