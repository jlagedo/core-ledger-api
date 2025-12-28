using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
/// Data transfer object for CoreJob entity.
/// </summary>
public record CoreJobDto(
    int Id,
    string ReferenceId,
    JobStatus Status,
    string StatusDescription,
    string JobDescription,
    DateTime CreationDate,
    DateTime? RunningDate,
    DateTime? FinishedDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
