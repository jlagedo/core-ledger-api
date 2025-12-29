using Microsoft.EntityFrameworkCore;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;

namespace CoreLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for TransactionSubType entity with specialized queries.
/// </summary>
public class TransactionSubTypeRepository : Repository<TransactionSubType>, ITransactionSubTypeRepository
{
    public TransactionSubTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TransactionSubType>> GetByTypeIdAsync(int typeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(st => st.Type)
            .Where(st => st.TypeId == typeId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
