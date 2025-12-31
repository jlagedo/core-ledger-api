using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Domain.Interfaces;

/// <summary>
///     Repository interface for Fund-specific operations.
/// </summary>
public interface IFundRepository : IRepository<Fund>
{
    /// <summary>
    ///     Gets a fund by name.
    /// </summary>
    Task<Fund?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets funds with query parameters using raw SQL (RFC-8040 compliant).
    /// </summary>
    Task<(IReadOnlyList<Fund> Funds, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);
}