namespace LibraryApi.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int PublishedYear { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public List<BookAuthor> BookAuthors { get; set; } = new();
}
