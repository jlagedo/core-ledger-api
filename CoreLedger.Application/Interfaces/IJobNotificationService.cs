using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Interfaces;

/// <summary>
/// Interface for publishing job status notifications to external systems.
/// </summary>
public interface IJobNotificationService
{
    /// <summary>
    /// Notifies external systems about a CoreJob status change.
    /// </summary>
    /// <param name="coreJob">The CoreJob entity with updated status</param>
    /// <param name="correlationId">Optional correlation ID for distributed tracing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task NotifyJobStatusChangeAsync(CoreJob coreJob, string? correlationId = null, CancellationToken cancellationToken = default);
}
