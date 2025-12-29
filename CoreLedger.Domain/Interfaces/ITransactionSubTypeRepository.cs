using CoreLedger.Domain.Entities;

namespace CoreLedger.Domain.Interfaces;

/// <summary>
/// Repository interface for TransactionSubType entity with specialized queries.
/// </summary>
public interface ITransactionSubTypeRepository : IRepository<TransactionSubType>
{
    /// <summary>
    /// Gets all transaction subtypes for a specific type.
    /// </summary>
    Task<IReadOnlyList<TransactionSubType>> GetByTypeIdAsync(int typeId, CancellationToken cancellationToken = default);
}
