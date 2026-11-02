using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories;

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Book>> ListWithDetailsAsync(CancellationToken cancellationToken)
    {
        return await BuildDetailsQuery()
            .OrderBy(book => book.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> SearchByTitleAsync(string title, CancellationToken cancellationToken)
    {
        var lowerTitle = title.ToLowerInvariant();

        return await BuildDetailsQuery()
            .Where(book => book.Title.ToLower().Contains(lowerTitle))
            .OrderBy(book => book.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetWithDetailsAsync(int id, CancellationToken cancellationToken)
    {
        return await BuildDetailsQuery().FirstOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    // Tracked, and loads only the pivot rows: the update diffs BookAuthors in place.
    public async Task<Book?> GetForUpdateAsync(int id, CancellationToken cancellationToken)
    {
        return await Entities
            .Include(book => book.BookAuthors)
            .FirstOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    public async Task<bool> IsbnExistsAsync(string isbn, int? excludeId, CancellationToken cancellationToken)
    {
        return await Entities.AnyAsync(
            book => book.Isbn == isbn && (excludeId == null || book.Id != excludeId),
            cancellationToken);
    }

    private IQueryable<Book> BuildDetailsQuery()
    {
        return Entities
            .AsNoTracking()
            .Include(book => book.Category)
            .Include(book => book.BookAuthors)
            .ThenInclude(bookAuthor => bookAuthor.Author);
    }
}
