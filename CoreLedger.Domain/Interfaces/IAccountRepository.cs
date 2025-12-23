using CoreLedger.Domain.Entities;

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
}
