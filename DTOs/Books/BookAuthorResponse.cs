namespace LibraryApi.DTOs.Books;

public record BookAuthorResponse(int AuthorId, string AuthorName, bool IsPrimary);
