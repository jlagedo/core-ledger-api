using CoreLedger.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
/// Background service that sends periodic heartbeat messages to all active SSE connections
/// to keep connections alive and detect disconnected clients.
/// </summary>
public class HeartbeatService : BackgroundService
{
    private readonly INotificationChannelStore _channelStore;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HeartbeatService> _logger;
    private readonly TimeSpan _heartbeatInterval;

    /// <summary>
    /// Initializes a new instance of the HeartbeatService class.
    /// </summary>
    public HeartbeatService(
        INotificationChannelStore channelStore,
        IConfiguration configuration,
        ILogger<HeartbeatService> logger)
    {
        _channelStore = channelStore;
        _configuration = configuration;
        _logger = logger;

        var intervalSeconds = int.Parse(configuration["Notifications:HeartbeatIntervalSeconds"] ?? "15");
        _heartbeatInterval = TimeSpan.FromSeconds(intervalSeconds);
    }

    /// <summary>
    /// Executes the heartbeat service, sending periodic heartbeat messages to all active channels.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "HeartbeatService starting with interval {IntervalSeconds}s",
            _heartbeatInterval.TotalSeconds);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_heartbeatInterval, stoppingToken);

                var activeChannels = _channelStore.GetAll().ToList();
                var successCount = 0;
                var failureCount = 0;

                foreach (var (userId, userChannel) in activeChannels)
                {
                    try
                    {
                        // Try to write heartbeat (non-blocking)
                        // IMPORTANT: Do NOT update LastActivity for heartbeats
                        if (userChannel.Channel.Writer.TryWrite(": heartbeat\n\n"))
                        {
                            successCount++;
                        }
                        else
                        {
                            failureCount++;
                            _logger.LogDebug(
                                "Failed to write heartbeat to channel for userId {UserId} (channel may be full or completed)",
                                userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogWarning(ex, "Error writing heartbeat to channel for userId {UserId}", userId);
                    }
                }

                if (activeChannels.Count > 0)
                {
                    _logger.LogDebug(
                        "Heartbeat sent to {SuccessCount}/{TotalCount} channels ({FailureCount} failures)",
                        successCount, activeChannels.Count, failureCount);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("HeartbeatService stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HeartbeatService");
            throw;
        }
    }

    /// <summary>
    /// Disposes resources when the service is stopped.
    /// </summary>
    public override void Dispose()
    {
        _logger.LogInformation("HeartbeatService disposed");
        base.Dispose();
    }
}
