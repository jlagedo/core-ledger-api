namespace CoreLedger.Domain.Enums;

/// <summary>
/// Represents the status of a core job.
/// </summary>
public enum JobStatus
{
    New = 1,
    Running = 2,
    Complete = 3,
    Failed = 4
}
