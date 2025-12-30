using CoreLedger.Domain.Entities;

namespace CoreLedger.Domain.Interfaces;

/// <summary>
/// Repository interface for User-specific operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by their Auth Provider ID and Provider combination.
    /// </summary>
    Task<User?> GetByAuthProviderIdAsync(
        string authProviderId,
        string provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
