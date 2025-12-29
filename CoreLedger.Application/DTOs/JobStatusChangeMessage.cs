namespace CoreLedger.Application.DTOs;

/// <summary>
/// Message contract for job status change notifications via Redis Pub/Sub.
/// Must match the format published by RedisJobNotificationService in the worker.
/// </summary>
public record JobStatusChangeMessage(
    int UserId,
    Guid JobId,
    string Message,
    string Status,
    string? CorrelationId
);
