using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories;

public class AuthorRepository : Repository<Author>, IAuthorRepository
{
    public AuthorRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<int> CountBooksAsync(int id, CancellationToken cancellationToken)
    {
        return await Context.BookAuthors.CountAsync(bookAuthor => bookAuthor.AuthorId == id, cancellationToken);
    }

    // One query for all ids, so the caller can compare against the requested count without N+1.
    public async Task<int> CountExistingAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await Entities.CountAsync(author => ids.Contains(author.Id), cancellationToken);
    }
}
