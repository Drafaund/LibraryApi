using LibraryApi.Models;

namespace LibraryApi.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken);

    Task<bool> HasBooksAsync(int id, CancellationToken cancellationToken);
}
