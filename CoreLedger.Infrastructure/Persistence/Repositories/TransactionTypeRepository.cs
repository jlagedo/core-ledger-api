using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;

namespace CoreLedger.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository implementation for TransactionType entity.
/// </summary>
public class TransactionTypeRepository : Repository<TransactionType>, ITransactionTypeRepository
{
    public TransactionTypeRepository(ApplicationDbContext context) : base(context)
    {
    }
}