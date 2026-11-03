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
    private const string InvalidRefreshTokenMessage = "Refresh token tidak valid atau sudah kedaluwarsa.";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenService refreshTokens,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshValidator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _refreshTokens = refreshTokens;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
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

        // The User role is seeded at startup, so a failure here is unexpected and not a client error.
        var roleAssigned = await _userManager.AddToRoleAsync(user, RoleNames.User);
        if (!roleAssigned.Succeeded)
            throw new InvalidOperationException($"Could not assign the {RoleNames.User} role to a new user.");

        return ServiceResult<AuthResponse>.Ok(await BuildResponseAsync(user, cancellationToken));
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

        return ServiceResult<AuthResponse>.Ok(await BuildResponseAsync(user, cancellationToken));
    }

    // Rotation: the presented token is revoked and a brand new pair is issued. Roles are read again from the
    // database when the access token is built, so a role change takes effect on the next refresh.
    public async Task<ServiceResult<AuthResponse>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _refreshValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ServiceResult<AuthResponse>.ValidationFail(validation);

        var redeemed = await _refreshTokens.RedeemAsync(request.RefreshToken, cancellationToken);
        if (!redeemed.IsSuccess)
            return ServiceResult<AuthResponse>.Fail(redeemed.ErrorType, redeemed.Message!);

        var user = await _userManager.FindByIdAsync(redeemed.Data!);
        if (user is null)
            return ServiceResult<AuthResponse>.Fail(ErrorType.Unauthorized, InvalidRefreshTokenMessage);

        return ServiceResult<AuthResponse>.Ok(await BuildResponseAsync(user, cancellationToken));
    }

    // Always succeeds for a well-formed request, so logging out twice or with an unknown token is not an error.
    public async Task<ServiceResult> LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var validation = await _refreshValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ServiceResult.ValidationFail(validation);

        await _refreshTokens.RevokeAsync(request.RefreshToken, cancellationToken);

        return ServiceResult.Ok();
    }

    private async Task<AuthResponse> BuildResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var (accessToken, expiresAt) = await _tokenGenerator.GenerateAccessTokenAsync(user);
        var refreshToken = await _refreshTokens.IssueAsync(user.Id, cancellationToken);

        return new AuthResponse(accessToken, expiresAt, refreshToken);
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
