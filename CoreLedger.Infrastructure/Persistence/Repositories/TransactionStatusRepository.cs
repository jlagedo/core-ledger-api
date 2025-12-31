using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;

namespace CoreLedger.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository implementation for TransactionStatus entity.
/// </summary>
public class TransactionStatusRepository : Repository<TransactionStatus>, ITransactionStatusRepository
{
    public TransactionStatusRepository(ApplicationDbContext context) : base(context)
    {
    }
}