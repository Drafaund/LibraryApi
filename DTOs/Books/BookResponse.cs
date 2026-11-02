using LibraryApi.DTOs.Categories;

namespace LibraryApi.DTOs.Books;

public record BookResponse(
    int Id,
    string Title,
    string Isbn,
    int Stock,
    int PublishedYear,
    CategoryResponse Category,
    IReadOnlyList<BookAuthorResponse> Authors);
