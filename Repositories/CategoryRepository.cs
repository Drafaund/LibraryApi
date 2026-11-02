using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    // The unique index on Name is case-sensitive, so case-insensitive uniqueness is checked here.
    public async Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var lowerName = name.ToLowerInvariant();

        return await Entities.AnyAsync(
            category => category.Name.ToLower() == lowerName
                && (excludeId == null || category.Id != excludeId),
            cancellationToken);
    }

    public async Task<bool> HasBooksAsync(int id, CancellationToken cancellationToken)
    {
        return await Context.Books.AnyAsync(book => book.CategoryId == id, cancellationToken);
    }
}
