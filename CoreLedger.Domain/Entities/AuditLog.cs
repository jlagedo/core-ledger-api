using System.Text.Json;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Represents an audit log entry for tracking entity changes.
/// </summary>
public class AuditLog
{
    /// <summary>
    ///     Private constructor for EF Core.
    /// </summary>
    private AuditLog()
    {
        EntityName = string.Empty;
        EntityId = string.Empty;
        EventType = string.Empty;
    }

    /// <summary>
    ///     Unique identifier for the audit log entry.
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Name of the entity being audited (e.g., 'Transaction', 'Account').
    /// </summary>
    public string EntityName { get; private set; }

    /// <summary>
    ///     Identifier of the entity being audited (supports UUID, int, string keys).
    /// </summary>
    public string EntityId { get; private set; }

    /// <summary>
    ///     Type of event that occurred (e.g., 'Created', 'Updated', 'Deleted').
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    ///     Identifier of the user who triggered the event.
    /// </summary>
    public string? PerformedByUserId { get; private set; }

    /// <summary>
    ///     Timestamp when the event occurred.
    /// </summary>
    public DateTime PerformedAt { get; private set; }

    /// <summary>
    ///     JSON snapshot of the entity state before the change.
    /// </summary>
    public JsonDocument? DataBefore { get; private set; }

    /// <summary>
    ///     JSON snapshot of the entity state after the change.
    /// </summary>
    public JsonDocument? DataAfter { get; private set; }

    /// <summary>
    ///     Correlation ID for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    ///     ASP.NET Core request ID.
    /// </summary>
    public string? RequestId { get; private set; }

    /// <summary>
    ///     Source of the event (e.g., 'API', 'Job', 'System').
    /// </summary>
    public string? Source { get; private set; }

    /// <summary>
    ///     Factory method to create a new audit log entry.
    /// </summary>
    /// <param name="entityName">Name of the entity being audited.</param>
    /// <param name="entityId">Identifier of the entity.</param>
    /// <param name="eventType">Type of event (Created, Updated, Deleted).</param>
    /// <param name="performedByUserId">User who triggered the event.</param>
    /// <param name="dataBefore">JSON snapshot before the change.</param>
    /// <param name="dataAfter">JSON snapshot after the change.</param>
    /// <param name="correlationId">Correlation ID for tracing.</param>
    /// <param name="requestId">ASP.NET Core request ID.</param>
    /// <param name="source">Source of the event.</param>
    /// <returns>A new AuditLog instance.</returns>
    public static AuditLog Create(
        string entityName,
        string entityId,
        string eventType,
        string? performedByUserId = null,
        JsonDocument? dataBefore = null,
        JsonDocument? dataAfter = null,
        string? correlationId = null,
        string? requestId = null,
        string? source = null)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Entity name is required.", nameof(entityName));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity ID is required.", nameof(entityId));

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.", nameof(eventType));

        return new AuditLog
        {
            EntityName = entityName.Trim(),
            EntityId = entityId.Trim(),
            EventType = eventType.Trim(),
            PerformedByUserId = performedByUserId?.Trim(),
            PerformedAt = DateTime.UtcNow,
            DataBefore = dataBefore,
            DataAfter = dataAfter,
            CorrelationId = correlationId?.Trim(),
            RequestId = requestId?.Trim(),
            Source = source?.Trim()
        };
    }
}
