using LibraryApi.Models;

namespace LibraryApi.Repositories;

public interface IAuthorRepository : IRepository<Author>
{
    Task<int> CountBooksAsync(int id, CancellationToken cancellationToken);

    Task<int> CountExistingAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
}
