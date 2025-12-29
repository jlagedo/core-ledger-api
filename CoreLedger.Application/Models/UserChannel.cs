using System.Threading.Channels;

namespace CoreLedger.Application.Models;

/// <summary>
/// Represents a notification channel for a specific user with SSE support.
/// Encapsulates a bounded channel with metadata for tracking activity and lifecycle.
/// </summary>
public sealed class UserChannel
{
    private readonly object _lastActivityLock = new();
    private DateTime _lastActivity;

    /// <summary>
    /// Gets the underlying channel for sending SSE messages to the user.
    /// </summary>
    public Channel<string> Channel { get; }

    /// <summary>
    /// Gets the last activity timestamp (updated only on successful message writes, not heartbeats).
    /// </summary>
    public DateTime LastActivity
    {
        get
        {
            lock (_lastActivityLock)
            {
                return _lastActivity;
            }
        }
    }

    /// <summary>
    /// Gets the timestamp when this channel was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the UserChannel class.
    /// </summary>
    /// <param name="capacity">The maximum number of messages the channel can hold.</param>
    public UserChannel(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleWriter = false, // Multiple writers (RedisSubscriber + Heartbeat)
            SingleReader = true,  // Only SSE endpoint reads
            AllowSynchronousContinuations = false // Prevent blocking
        };

        Channel = System.Threading.Channels.Channel.CreateBounded<string>(options);
        CreatedAt = DateTime.UtcNow;
        _lastActivity = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the last activity timestamp to the current UTC time.
    /// Thread-safe operation. Should be called only when a real message is sent (not heartbeats).
    /// </summary>
    public void UpdateLastActivity()
    {
        lock (_lastActivityLock)
        {
            _lastActivity = DateTime.UtcNow;
        }
    }
}
