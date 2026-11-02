using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    // Tracked on purpose: the caller sets RevokedAt on the returned entity and saves.
    public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return await Entities.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }
}
