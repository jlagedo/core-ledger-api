using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository implementation for User entity with specific queries.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByAuthProviderIdAsync(
        string authProviderId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.AuthProviderId == authProviderId && u.Provider == provider,
                cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}