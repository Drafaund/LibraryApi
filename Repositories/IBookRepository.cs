using LibraryApi.Models;

namespace LibraryApi.Repositories;

public interface IBookRepository : IRepository<Book>
{
    Task<IReadOnlyList<Book>> ListWithDetailsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Book>> SearchByTitleAsync(string title, CancellationToken cancellationToken);

    Task<Book?> GetWithDetailsAsync(int id, CancellationToken cancellationToken);

    Task<Book?> GetForUpdateAsync(int id, CancellationToken cancellationToken);

    Task<bool> IsbnExistsAsync(string isbn, int? excludeId, CancellationToken cancellationToken);
}
