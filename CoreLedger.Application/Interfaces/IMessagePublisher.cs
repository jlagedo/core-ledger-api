namespace CoreLedger.Application.Interfaces;

/// <summary>
/// Interface for publishing messages to message queues.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the specified queue.
    /// </summary>
    /// <param name="queueName">Name of the queue to publish to</param>
    /// <param name="message">Message object to publish</param>
    /// <param name="correlationId">Optional correlation ID for distributed tracing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<T>(string queueName, T message, string? correlationId = null, CancellationToken cancellationToken = default) where T : class;
}
