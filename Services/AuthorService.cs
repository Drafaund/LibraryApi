using AutoMapper;
using FluentValidation;
using LibraryApi.Common;
using LibraryApi.DTOs.Authors;
using LibraryApi.Models;
using LibraryApi.Repositories;

namespace LibraryApi.Services;

public class AuthorService : IAuthorService
{
    private const string NotFoundMessage = "Penulis tidak ditemukan.";
    private const string InUseMessage = "Penulis masih dipakai buku dan tidak dapat dihapus.";

    private readonly IAuthorRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateAuthorRequest> _createValidator;
    private readonly IValidator<UpdateAuthorRequest> _updateValidator;

    public AuthorService(
        IAuthorRepository repository,
        IMapper mapper,
        IValidator<CreateAuthorRequest> createValidator,
        IValidator<UpdateAuthorRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ServiceResult<IReadOnlyList<AuthorResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var authors = await _repository.ListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<AuthorResponse>>.Ok(
            _mapper.Map<IReadOnlyList<AuthorResponse>>(authors));
    }

    public async Task<ServiceResult<AuthorResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var author = await _repository.GetByIdAsync(id, cancellationToken);
        if (author is null)
            return ServiceResult<AuthorResponse>.Fail(ErrorType.NotFound, NotFoundMessage);

        return ServiceResult<AuthorResponse>.Ok(_mapper.Map<AuthorResponse>(author));
    }

    public async Task<ServiceResult<AuthorResponse>> CreateAsync(
        CreateAuthorRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ServiceResult<AuthorResponse>.ValidationFail(validation);

        var author = _mapper.Map<Author>(request with { Name = request.Name.Trim() });
        await _repository.AddAsync(author, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthorResponse>.Ok(_mapper.Map<AuthorResponse>(author));
    }

    public async Task<ServiceResult<AuthorResponse>> UpdateAsync(
        int id,
        UpdateAuthorRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ServiceResult<AuthorResponse>.ValidationFail(validation);

        var author = await _repository.GetByIdAsync(id, cancellationToken);
        if (author is null)
            return ServiceResult<AuthorResponse>.Fail(ErrorType.NotFound, NotFoundMessage);

        // The entity is tracked, so SaveChangesAsync picks up the mapped changes without Update().
        _mapper.Map(request with { Name = request.Name.Trim() }, author);
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthorResponse>.Ok(_mapper.Map<AuthorResponse>(author));
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var author = await _repository.GetByIdAsync(id, cancellationToken);
        if (author is null)
            return ServiceResult.Fail(ErrorType.NotFound, NotFoundMessage);

        if (await _repository.CountBooksAsync(id, cancellationToken) > 0)
            return ServiceResult.Fail(ErrorType.Conflict, InUseMessage);

        _repository.Remove(author);
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult.Ok();
    }
}
