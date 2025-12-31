using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Domain.Interfaces;

/// <summary>
///     Repository interface for AuditLog entity operations.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    ///     Retrieves an audit log entry by its unique identifier.
    /// </summary>
    /// <param name="id">The audit log entry ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The audit log entry if found; otherwise, null.</returns>
    Task<AuditLog?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves audit log entries with filtering, sorting, and pagination.
    /// </summary>
    /// <param name="parameters">Query parameters for filtering, sorting, and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the list of audit logs and the total count.</returns>
    Task<(IReadOnlyList<AuditLog> AuditLogs, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a new audit log entry to the repository.
    /// </summary>
    /// <param name="auditLog">The audit log entry to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added audit log entry with generated ID.</returns>
    Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
