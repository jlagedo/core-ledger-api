namespace CoreLedger.Domain.Enums;

/// <summary>
///     Represents the processing status of an outbox message.
/// </summary>
public enum OutboxMessageStatus
{
    /// <summary>
    ///     Message is pending publication to message queue.
    /// </summary>
    Pending = 0,

    /// <summary>
    ///     Message has been successfully published to message queue.
    /// </summary>
    Published = 1,

    /// <summary>
    ///     Message publication failed after retry attempts.
    /// </summary>
    Failed = 2
}
