using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Represents an outbox message for transaction creation events.
///     Implements the Transactional Outbox pattern to ensure reliable message publication.
/// </summary>
public class TransactionCreatedOutboxMessage
{
    /// <summary>
    ///     Private constructor for EF Core.
    /// </summary>
    private TransactionCreatedOutboxMessage()
    {
        Type = string.Empty;
        Payload = [];
    }

    /// <summary>
    ///     Unique identifier for the outbox message entry.
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    ///     Timestamp when the event occurred (UTC).
    /// </summary>
    public DateTime OccurredOn { get; private set; }

    /// <summary>
    ///     Type of the event (fully qualified class name).
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    ///     Serialized message payload (Protobuf binary format).
    /// </summary>
    public byte[] Payload { get; private set; }

    /// <summary>
    ///     Current processing status of the outbox message.
    /// </summary>
    public OutboxMessageStatus Status { get; private set; } = OutboxMessageStatus.Pending;

    /// <summary>
    ///     Number of times publication has been attempted.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    ///     Error message from the last failed publication attempt.
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    ///     Timestamp when the message was successfully published (UTC).
    /// </summary>
    public DateTime? PublishedOn { get; private set; }

    /// <summary>
    ///     Factory method to create a new transaction created outbox message.
    /// </summary>
    /// <param name="type">Fully qualified type name of the event.</param>
    /// <param name="payload">Protobuf-serialized event payload.</param>
    /// <param name="occurredOn">Optional timestamp of when the event occurred (defaults to UTC now).</param>
    /// <returns>A new TransactionCreatedOutboxMessage instance.</returns>
    /// <exception cref="ArgumentException">Thrown when type or payload is invalid.</exception>
    public static TransactionCreatedOutboxMessage Create(string type, byte[] payload, DateTime? occurredOn = null)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Event type cannot be empty.", nameof(type));

        if (payload == null || payload.Length == 0)
            throw new ArgumentException("Payload cannot be null or empty.", nameof(payload));

        return new TransactionCreatedOutboxMessage
        {
            Type = type.Trim(),
            Payload = payload,
            OccurredOn = occurredOn ?? DateTime.UtcNow,
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0
        };
    }

    /// <summary>
    ///     Marks the message as successfully published.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the message is already published.</exception>
    public void MarkAsPublished()
    {
        if (Status == OutboxMessageStatus.Published)
            throw new DomainValidationException("Message is already published.");

        Status = OutboxMessageStatus.Published;
        PublishedOn = DateTime.UtcNow;
    }

    /// <summary>
    ///     Records a failed publication attempt with the error details.
    /// </summary>
    /// <param name="errorMessage">Description of the error that occurred.</param>
    /// <exception cref="ArgumentException">Thrown when error message is empty.</exception>
    public void RecordFailure(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty.", nameof(errorMessage));

        RetryCount++;
        LastError = errorMessage.Trim();
        Status = OutboxMessageStatus.Failed;
    }

    /// <summary>
    ///     Resets the message for retry after a failed publication attempt.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the message is already published.</exception>
    public void ResetForRetry()
    {
        if (Status == OutboxMessageStatus.Published)
            throw new DomainValidationException("Cannot retry a published message.");

        Status = OutboxMessageStatus.Pending;
        LastError = null;
    }
}
