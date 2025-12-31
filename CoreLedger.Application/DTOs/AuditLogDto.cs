using System.Text.Json;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Data transfer object for AuditLog entity.
/// </summary>
public record AuditLogDto(
    long Id,
    string EntityName,
    string EntityId,
    string EventType,
    string? PerformedByUserId,
    DateTime PerformedAt,
    JsonElement? DataBefore,
    JsonElement? DataAfter,
    string? CorrelationId,
    string? RequestId,
    string? Source
);
