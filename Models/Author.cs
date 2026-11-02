namespace LibraryApi.Models;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<BookAuthor> BookAuthors { get; set; } = new();
}
