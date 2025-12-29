using System.Collections.Concurrent;
using CoreLedger.Application.Interfaces;
using CoreLedger.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
/// Thread-safe singleton store for managing per-user notification channels for SSE connections.
/// </summary>
public class NotificationChannelStore : INotificationChannelStore
{
    private readonly ConcurrentDictionary<int, UserChannel> _channels;
    private readonly int _channelCapacity;
    private readonly ILogger<NotificationChannelStore> _logger;
    private volatile bool _isShuttingDown;

    /// <summary>
    /// Initializes a new instance of the NotificationChannelStore class.
    /// </summary>
    /// <param name="configuration">Configuration for reading channel capacity.</param>
    /// <param name="logger">Logger for structured logging.</param>
    public NotificationChannelStore(
        IConfiguration configuration,
        ILogger<NotificationChannelStore> logger)
    {
        _channels = new ConcurrentDictionary<int, UserChannel>();
        _channelCapacity = int.Parse(configuration["Notifications:ChannelCapacity"] ?? "100");
        _logger = logger;
        _isShuttingDown = false;

        _logger.LogInformation("NotificationChannelStore initialized with capacity {ChannelCapacity}", _channelCapacity);
    }

    /// <inheritdoc/>
    public UserChannel GetOrCreate(int userId)
    {
        var channel = _channels.GetOrAdd(userId, _ =>
        {
            var userChannel = new UserChannel(_channelCapacity);
            _logger.LogDebug("Channel created for userId {UserId}", userId);
            return userChannel;
        });

        return channel;
    }

    /// <inheritdoc/>
    public bool TryGet(int userId, out UserChannel? channel)
    {
        return _channels.TryGetValue(userId, out channel);
    }

    /// <inheritdoc/>
    public bool Remove(int userId)
    {
        if (_channels.TryRemove(userId, out var channel))
        {
            _logger.LogDebug("Channel removed for userId {UserId}", userId);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public IEnumerable<KeyValuePair<int, UserChannel>> GetAll()
    {
        return _channels.ToArray(); // Snapshot to avoid collection modification issues
    }

    /// <inheritdoc/>
    public int ActiveCount => _channels.Count;

    /// <inheritdoc/>
    public bool IsShuttingDown => _isShuttingDown;

    /// <inheritdoc/>
    public void MarkShuttingDown()
    {
        _isShuttingDown = true;
        _logger.LogWarning("NotificationChannelStore marked as shutting down. Active channels: {ActiveCount}", ActiveCount);
    }
}
