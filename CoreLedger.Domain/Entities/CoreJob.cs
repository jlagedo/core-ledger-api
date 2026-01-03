using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     CoreJob domain entity representing a background job with status tracking.
/// </summary>
public class CoreJob : BaseEntity
{
    private CoreJob()
    {
    }

    public string ReferenceId { get; private set; } = string.Empty;
    public JobStatus Status { get; private set; }
    public string JobDescription { get; private set; } = string.Empty;
    public DateTime CreationDate { get; private set; }
    public DateTime? RunningDate { get; private set; }
    public DateTime? FinishedDate { get; private set; }

    /// <summary>
    ///     Factory method to create a new CoreJob with validation.
    /// </summary>
    public static CoreJob Create(
        string referenceId,
        string jobDescription)
    {
        ValidateReferenceId(referenceId);
        ValidateJobDescription(jobDescription);

        return new CoreJob
        {
            ReferenceId = referenceId.Trim(),
            JobDescription = jobDescription.Trim(),
            Status = JobStatus.New,
            CreationDate = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Updates the job status with optional running and finished dates.
    /// </summary>
    public void UpdateStatus(
        JobStatus status,
        DateTime? runningDate = null,
        DateTime? finishedDate = null)
    {
        ValidateStatusTransition(status, runningDate, finishedDate);

        Status = status;
        RunningDate = runningDate;
        FinishedDate = finishedDate;
        SetUpdated();
    }

    private static void ValidateReferenceId(string referenceId)
    {
        if (string.IsNullOrWhiteSpace(referenceId))
            throw new DomainValidationException("Reference ID cannot be empty");

        if (referenceId.Length > 50)
            throw new DomainValidationException("Reference ID cannot exceed 50 characters");
    }

    private static void ValidateJobDescription(string jobDescription)
    {
        if (string.IsNullOrWhiteSpace(jobDescription))
            throw new DomainValidationException("Job description cannot be empty");

        if (jobDescription.Length > 255)
            throw new DomainValidationException("Job description cannot exceed 255 characters");
    }

    private static void ValidateStatusTransition(
        JobStatus status,
        DateTime? runningDate,
        DateTime? finishedDate)
    {
        if (status == JobStatus.Running && !runningDate.HasValue)
            throw new DomainValidationException("Running date must be set when status is Running");

        if ((status == JobStatus.Complete || status == JobStatus.Failed) && !finishedDate.HasValue)
            throw new DomainValidationException("Finished date must be set when status is Complete or Failed");
    }
}