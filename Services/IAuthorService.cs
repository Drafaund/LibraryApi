using LibraryApi.Common;
using LibraryApi.DTOs.Authors;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<ServiceResult<IReadOnlyList<AuthorResponse>>> ListAsync(CancellationToken cancellationToken);

    Task<ServiceResult<AuthorResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<ServiceResult<AuthorResponse>> CreateAsync(CreateAuthorRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<AuthorResponse>> UpdateAsync(int id, UpdateAuthorRequest request, CancellationToken cancellationToken);

    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken);
}
