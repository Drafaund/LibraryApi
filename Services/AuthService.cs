using FluentValidation;
using FluentValidation.Results;
using LibraryApi.Common;
using LibraryApi.DTOs.Auth;
using LibraryApi.Models;
using Microsoft.AspNetCore.Identity;

namespace LibraryApi.Services;

public class AuthService : IAuthService
{
    private const string EmailTakenMessage = "Email sudah terdaftar.";
    private const string InvalidCredentialsMessage = "Email atau password salah.";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator tokenGenerator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ServiceResult<AuthResponse>.ValidationFail(validation);

        // Email doubles as the UserName so login only needs one identifier.
        var email = request.Email.Trim();
        var user = new ApplicationUser { UserName = email, Email = email };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return FailIdentity(created.Errors);

        return ServiceResult<AuthResponse>.Ok(BuildResponse(user));
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ServiceResult<AuthResponse>.ValidationFail(validation);

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());

        // One message for unknown email and wrong password so the response does not reveal which emails exist.
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return ServiceResult<AuthResponse>.Fail(ErrorType.Unauthorized, InvalidCredentialsMessage);

        return ServiceResult<AuthResponse>.Ok(BuildResponse(user));
    }

    private AuthResponse BuildResponse(ApplicationUser user)
    {
        var (token, expiresAt) = _tokenGenerator.GenerateAccessToken(user);
        return new AuthResponse(token, expiresAt);
    }

    // A taken email is a 409. Any other Identity error (password policy, malformed email) is a 400 on its field.
    private static ServiceResult<AuthResponse> FailIdentity(IEnumerable<IdentityError> identityErrors)
    {
        var errors = identityErrors.ToList();

        if (errors.Any(error => error.Code is nameof(IdentityErrorDescriber.DuplicateUserName)
                                           or nameof(IdentityErrorDescriber.DuplicateEmail)))
        {
            return ServiceResult<AuthResponse>.Fail(ErrorType.Conflict, EmailTakenMessage);
        }

        var failures = errors.Select(error => new ValidationFailure(
            error.Code.StartsWith("Password", StringComparison.Ordinal) ? nameof(RegisterRequest.Password) : nameof(RegisterRequest.Email),
            error.Description));

        return ServiceResult<AuthResponse>.ValidationFail(new ValidationResult(failures));
    }
}
