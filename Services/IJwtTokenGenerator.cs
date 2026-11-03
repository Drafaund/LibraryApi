using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IJwtTokenGenerator
{
    Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(ApplicationUser user);
}
