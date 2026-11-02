using LibraryApi.Models;

namespace LibraryApi.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);
}
