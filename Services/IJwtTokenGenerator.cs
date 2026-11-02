using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user);
}
