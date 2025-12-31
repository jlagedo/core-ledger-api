using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Domain.Interfaces;

/// <summary>
///     Repository interface for Transaction entity with specialized queries.
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    /// <summary>
    ///     Gets a transaction by ID with all navigation properties loaded.
    /// </summary>
    Task<Transaction?> GetByIdWithNavigationAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets transactions with query parameters (filtering, sorting, pagination).
    /// </summary>
    Task<(IReadOnlyList<Transaction> Transactions, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);
}