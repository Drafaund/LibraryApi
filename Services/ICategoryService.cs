using LibraryApi.Common;
using LibraryApi.DTOs.Categories;

namespace LibraryApi.Services;

public interface ICategoryService
{
    Task<ServiceResult<IReadOnlyList<CategoryResponse>>> ListAsync(CancellationToken cancellationToken);

    Task<ServiceResult<CategoryResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<ServiceResult<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken);

    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken);
}
