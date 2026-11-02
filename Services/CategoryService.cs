using AutoMapper;
using FluentValidation;
using LibraryApi.Common;
using LibraryApi.DTOs.Categories;
using LibraryApi.Models;
using LibraryApi.Repositories;

namespace LibraryApi.Services;

public class CategoryService : ICategoryService
{
    private const string NotFoundMessage = "Kategori tidak ditemukan.";
    private const string NameTakenMessage = "Nama kategori sudah digunakan.";
    private const string HasBooksMessage = "Kategori masih memiliki buku dan tidak dapat dihapus.";

    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCategoryRequest> _createValidator;
    private readonly IValidator<UpdateCategoryRequest> _updateValidator;

    public CategoryService(
        ICategoryRepository repository,
        IMapper mapper,
        IValidator<CreateCategoryRequest> createValidator,
        IValidator<UpdateCategoryRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ServiceResult<IReadOnlyList<CategoryResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var categories = await _repository.ListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<CategoryResponse>>.Ok(
            _mapper.Map<IReadOnlyList<CategoryResponse>>(categories));
    }

    public async Task<ServiceResult<CategoryResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return ServiceResult<CategoryResponse>.Fail(ErrorType.NotFound, NotFoundMessage);

        return ServiceResult<CategoryResponse>.Ok(_mapper.Map<CategoryResponse>(category));
    }

    public async Task<ServiceResult<CategoryResponse>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ServiceResult<CategoryResponse>.ValidationFail(validation);

        var name = request.Name.Trim();
        if (await _repository.NameExistsAsync(name, null, cancellationToken))
            return ServiceResult<CategoryResponse>.Fail(ErrorType.Conflict, NameTakenMessage);

        var category = _mapper.Map<Category>(request with { Name = name });
        await _repository.AddAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<CategoryResponse>.Ok(_mapper.Map<CategoryResponse>(category));
    }

    public async Task<ServiceResult<CategoryResponse>> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ServiceResult<CategoryResponse>.ValidationFail(validation);

        var category = await _repository.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return ServiceResult<CategoryResponse>.Fail(ErrorType.NotFound, NotFoundMessage);

        var name = request.Name.Trim();
        if (await _repository.NameExistsAsync(name, id, cancellationToken))
            return ServiceResult<CategoryResponse>.Fail(ErrorType.Conflict, NameTakenMessage);

        // The entity is tracked, so SaveChangesAsync picks up the mapped changes without Update().
        _mapper.Map(request with { Name = name }, category);
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<CategoryResponse>.Ok(_mapper.Map<CategoryResponse>(category));
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return ServiceResult.Fail(ErrorType.NotFound, NotFoundMessage);

        if (await _repository.HasBooksAsync(id, cancellationToken))
            return ServiceResult.Fail(ErrorType.Conflict, HasBooksMessage);

        _repository.Remove(category);
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult.Ok();
    }
}
