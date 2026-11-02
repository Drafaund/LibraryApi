using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var hasData = await db.Categories.AnyAsync()
            || await db.Authors.AnyAsync()
            || await db.Books.AnyAsync();
        if (hasData)
            return;

        var programming = new Category { Name = "Pemrograman" };
        var fiction = new Category { Name = "Fiksi" };

        var hunt = new Author { Name = "Andrew Hunt" };
        var thomas = new Author { Name = "David Thomas" };
        var hirata = new Author { Name = "Andrea Hirata" };

        db.Books.AddRange(
            new Book
            {
                Title = "The Pragmatic Programmer",
                Isbn = "9780135957059",
                Stock = 5,
                PublishedYear = 2019,
                Category = programming,
                BookAuthors =
                {
                    new BookAuthor { Author = hunt, IsPrimary = true },
                    new BookAuthor { Author = thomas, IsPrimary = false }
                }
            },
            new Book
            {
                Title = "Laskar Pelangi",
                Isbn = "9789793062792",
                Stock = 3,
                PublishedYear = 2005,
                Category = fiction,
                BookAuthors =
                {
                    new BookAuthor { Author = hirata, IsPrimary = true }
                }
            });

        await db.SaveChangesAsync();
    }
}
