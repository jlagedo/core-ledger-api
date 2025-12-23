using Microsoft.EntityFrameworkCore;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;

namespace CoreLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for AccountType entity with specific queries.
/// </summary>
public class AccountTypeRepository : Repository<AccountType>, IAccountTypeRepository
{
    public AccountTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<AccountType?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(at => at.Description.ToLower() == description.ToLower(), cancellationToken);
    }
}
