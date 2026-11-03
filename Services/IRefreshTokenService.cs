using LibraryApi.Common;

namespace LibraryApi.Services;

public interface IRefreshTokenService
{
    // Returns the raw token. Only its hash is stored, so this is the one time it can be read.
    Task<string> IssueAsync(string userId, CancellationToken cancellationToken);

    // Revokes the token and returns the id of its owner. Unknown, expired or revoked tokens are Unauthorized.
    Task<ServiceResult<string>> RedeemAsync(string rawToken, CancellationToken cancellationToken);

    // Idempotent: an unknown or already revoked token is not an error.
    Task RevokeAsync(string rawToken, CancellationToken cancellationToken);
}
