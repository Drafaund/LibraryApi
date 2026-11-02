namespace LibraryApi.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<Book> Books { get; set; } = new();
}
