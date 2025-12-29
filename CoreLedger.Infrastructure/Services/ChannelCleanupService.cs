using CoreLedger.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
/// Background service that periodically removes idle notification channels
/// to prevent memory leaks from abandoned SSE connections.
/// </summary>
public class ChannelCleanupService : BackgroundService
{
    private readonly INotificationChannelStore _channelStore;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChannelCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval;
    private readonly TimeSpan _idleTimeout;

    /// <summary>
    /// Initializes a new instance of the ChannelCleanupService class.
    /// </summary>
    public ChannelCleanupService(
        INotificationChannelStore channelStore,
        IConfiguration configuration,
        ILogger<ChannelCleanupService> logger)
    {
        _channelStore = channelStore;
        _configuration = configuration;
        _logger = logger;

        var cleanupIntervalSeconds = int.Parse(configuration["Notifications:CleanupIntervalSeconds"] ?? "30");
        var idleTimeoutMinutes = int.Parse(configuration["Notifications:IdleTimeoutMinutes"] ?? "15");

        _cleanupInterval = TimeSpan.FromSeconds(cleanupIntervalSeconds);
        _idleTimeout = TimeSpan.FromMinutes(idleTimeoutMinutes);
    }

    /// <summary>
    /// Executes the channel cleanup service, periodically removing idle channels.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ChannelCleanupService starting with cleanup interval {CleanupIntervalSeconds}s and idle timeout {IdleTimeoutMinutes}m",
            _cleanupInterval.TotalSeconds, _idleTimeout.TotalMinutes);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                var now = DateTime.UtcNow;
                var activeChannels = _channelStore.GetAll().ToList();
                var removedCount = 0;

                foreach (var (userId, userChannel) in activeChannels)
                {
                    try
                    {
                        var idleDuration = now - userChannel.LastActivity;

                        if (idleDuration > _idleTimeout)
                        {
                            // Complete the channel to signal the SSE endpoint to close
                            userChannel.Channel.Writer.Complete();

                            // Remove from store
                            if (_channelStore.Remove(userId))
                            {
                                removedCount++;
                                _logger.LogInformation(
                                    "Removed idle channel for userId {UserId}, idle for {IdleMinutes}m",
                                    userId, (int)idleDuration.TotalMinutes);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error cleaning up channel for userId {UserId}", userId);
                    }
                }

                if (removedCount > 0)
                {
                    _logger.LogInformation(
                        "Cleanup completed: removed {RemovedCount} idle channels, {ActiveCount} channels remaining",
                        removedCount, _channelStore.ActiveCount);
                }
                else if (activeChannels.Count > 0)
                {
                    _logger.LogDebug(
                        "Cleanup completed: no idle channels found, {ActiveCount} channels active",
                        _channelStore.ActiveCount);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ChannelCleanupService stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ChannelCleanupService");
            throw;
        }
    }

    /// <summary>
    /// Disposes resources when the service is stopped.
    /// </summary>
    public override void Dispose()
    {
        _logger.LogInformation("ChannelCleanupService disposed");
        base.Dispose();
    }
}
