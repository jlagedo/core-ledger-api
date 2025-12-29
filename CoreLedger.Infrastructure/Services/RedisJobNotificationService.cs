using System.Text.Json;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
/// Redis pub/sub implementation of job notification service.
/// Publishes job status changes to a Redis channel for real-time client notifications.
/// </summary>
public class RedisJobNotificationService : IJobNotificationService, IDisposable
{
    private readonly ILogger<RedisJobNotificationService> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly string _channelName;
    private bool _disposed;

    public RedisJobNotificationService(
        IConfiguration configuration,
        ILogger<RedisJobNotificationService> logger)
    {
        _logger = logger;

        var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        _channelName = configuration["Redis:JobStatusChannel"] ?? "jobs.status-change";

        try
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _logger.LogInformation("Redis connection established to {ConnectionString}", connectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Redis at {ConnectionString}", connectionString);
            throw;
        }
    }

    /// <summary>
    /// Publishes job status change notification to Redis pub/sub channel.
    /// Error handling: Logs errors but does not throw to prevent job processing failures.
    /// </summary>
    public async Task NotifyJobStatusChangeAsync(CoreJob coreJob, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            _logger.LogWarning("RedisJobNotificationService is disposed, cannot send notification");
            return;
        }

        try
        {
            var message = new
            {
                UserId = 1000,
                JobId = coreJob.Id,
                Message = coreJob.JobDescription,
                Status = coreJob.Status.ToString(),
                CorrelationId = correlationId
            };

            var messageJson = JsonSerializer.Serialize(message);
            var subscriber = _redis.GetSubscriber();

            await subscriber.PublishAsync(RedisChannel.Literal(_channelName), messageJson);

            _logger.LogInformation(
                "Job status notification published to Redis: JobId={JobId}, Status={Status}, CorrelationId={CorrelationId}",
                coreJob.Id, coreJob.Status, correlationId ?? "none");
        }
        catch (Exception ex)
        {
            // Log error but don't throw - Redis failures should not break job processing
            _logger.LogError(ex,
                "Failed to publish job status notification for JobId={JobId}, Status={Status}",
                coreJob.Id, coreJob.Status);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            _redis?.Dispose();
            _logger.LogInformation("Redis connection disposed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing Redis connection");
        }

        _disposed = true;
    }
}
