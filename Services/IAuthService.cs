using LibraryApi.Common;
using LibraryApi.DTOs.Auth;

namespace LibraryApi.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);

    Task<ServiceResult> LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
}
