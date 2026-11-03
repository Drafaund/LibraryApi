using System.Security.Cryptography;
using System.Text;
using LibraryApi.Common;
using LibraryApi.Models;
using LibraryApi.Repositories;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace LibraryApi.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private const int TokenByteLength = 32;
    private const string InvalidTokenMessage = "Refresh token tidak valid atau sudah kedaluwarsa.";

    private readonly IRefreshTokenRepository _repository;
    private readonly JwtOptions _options;

    public RefreshTokenService(IRefreshTokenRepository repository, IOptions<JwtOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    public async Task<string> IssueAsync(string userId, CancellationToken cancellationToken)
    {
        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenByteLength));

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.Add(_options.RefreshTokenLifetime)
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return rawToken;
    }

    public async Task<ServiceResult<string>> RedeemAsync(string rawToken, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByHashAsync(HashToken(rawToken), cancellationToken);
        if (entity is null || !IsActive(entity))
            return ServiceResult<string>.Fail(ErrorType.Unauthorized, InvalidTokenMessage);

        entity.RevokedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<string>.Ok(entity.UserId);
    }

    public async Task RevokeAsync(string rawToken, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByHashAsync(HashToken(rawToken), cancellationToken);
        if (entity is null || entity.RevokedAt is not null)
            return;

        entity.RevokedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static bool IsActive(RefreshToken token) =>
        token.RevokedAt is null && token.ExpiresAt > DateTime.UtcNow;

    // SHA-256 is enough here: the token is 256 random bits, so there is nothing to brute-force like a password.
    private static string HashToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
