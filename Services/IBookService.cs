using LibraryApi.Common;
using LibraryApi.DTOs.Books;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<ServiceResult<IReadOnlyList<BookListItemResponse>>> ListAsync(CancellationToken cancellationToken);

    Task<ServiceResult<IReadOnlyList<BookListItemResponse>>> SearchByTitleAsync(string title, CancellationToken cancellationToken);

    Task<ServiceResult<BookResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<ServiceResult<BookResponse>> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<BookResponse>> UpdateAsync(int id, UpdateBookRequest request, CancellationToken cancellationToken);

    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken);
}
