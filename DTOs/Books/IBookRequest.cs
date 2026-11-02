namespace LibraryApi.DTOs.Books;

// Lets create and update share one set of validation rules.
public interface IBookRequest
{
    string Title { get; }
    string Isbn { get; }
    int Stock { get; }
    int PublishedYear { get; }
    int CategoryId { get; }
    IReadOnlyList<int> AuthorIds { get; }
    int PrimaryAuthorId { get; }
}
