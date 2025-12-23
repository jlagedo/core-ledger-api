using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Domain.Interfaces;

/// <summary>
/// Repository interface for Account-specific operations.
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    /// <summary>
    /// Gets an account by code.
    /// </summary>
    Task<Account?> GetByCodeAsync(long code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an account by ID with its type included.
    /// </summary>
    Task<Account?> GetByIdWithTypeAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all accounts with their types included.
    /// </summary>
    Task<IReadOnlyList<Account>> GetAllWithTypeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets accounts with query parameters using raw SQL (RFC-8040 compliant).
    /// </summary>
    Task<(IReadOnlyList<Account> Accounts, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters, 
        CancellationToken cancellationToken = default);
}
