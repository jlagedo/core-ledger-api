using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Domain.Interfaces;

/// <summary>
///     Repository interface for CoreJob-specific operations.
/// </summary>
public interface ICoreJobRepository : IRepository<CoreJob>
{
    /// <summary>
    ///     Gets a core job by reference ID.
    /// </summary>
    Task<CoreJob?> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets core jobs with query parameters using raw SQL (RFC-8040 compliant).
    /// </summary>
    Task<(IReadOnlyList<CoreJob> Jobs, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);
}