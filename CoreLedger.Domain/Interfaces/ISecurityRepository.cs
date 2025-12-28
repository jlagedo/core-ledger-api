using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Domain.Interfaces;

/// <summary>
/// Repository interface for Security entity.
/// </summary>
public interface ISecurityRepository : IRepository<Security>
{
    /// <summary>
    /// Retrieves a security by its ticker symbol.
    /// </summary>
    Task<Security?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves securities with advanced query parameters (filtering, sorting, pagination).
    /// </summary>
    Task<(IReadOnlyList<Security> Securities, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);
}
