using CoreLedger.Domain.Entities;

namespace CoreLedger.Domain.Interfaces;

/// <summary>
/// Repository interface for AccountType-specific operations.
/// </summary>
public interface IAccountTypeRepository : IRepository<AccountType>
{
    /// <summary>
    /// Gets an account type by description (case-insensitive).
    /// </summary>
    Task<AccountType?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default);
}
