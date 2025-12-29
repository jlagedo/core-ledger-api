using System.Text.Json;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
/// Background service that subscribes to Redis Pub/Sub for job status changes
/// and routes notifications to connected SSE clients via user channels.
/// </summary>
public class RedisSubscriberService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly INotificationChannelStore _channelStore;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RedisSubscriberService> _logger;
    private readonly string _channelName;

    /// <summary>
    /// Initializes a new instance of the RedisSubscriberService class.
    /// </summary>
    public RedisSubscriberService(
        IConnectionMultiplexer redis,
        INotificationChannelStore channelStore,
        IConfiguration configuration,
        ILogger<RedisSubscriberService> logger)
    {
        _redis = redis;
        _channelStore = channelStore;
        _configuration = configuration;
        _logger = logger;
        _channelName = configuration["Redis:JobStatusChannel"] ?? "jobs.status-change";

        // Log Redis connection events for observability
        _redis.ConnectionFailed += (sender, args) =>
        {
            _logger.LogWarning("Redis connection failed: {FailureType} - {Exception}",
                args.FailureType, args.Exception?.Message);
        };

        _redis.ConnectionRestored += (sender, args) =>
        {
            _logger.LogInformation("Redis connection restored");
        };
    }

    /// <summary>
    /// Executes the Redis subscriber service.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RedisSubscriberService starting, subscribing to channel: {ChannelName}", _channelName);

        try
        {
            var subscriber = _redis.GetSubscriber();

            // Subscribe to Redis Pub/Sub channel
            await subscriber.SubscribeAsync(RedisChannel.Literal(_channelName), async (channel, message) =>
            {
                try
                {
                    if (message.IsNullOrEmpty)
                    {
                        _logger.LogDebug("Received empty message on channel {ChannelName}", _channelName);
                        return;
                    }

                    var messageJson = message.ToString();

                    // Deserialize the job status change message
                    JobStatusChangeMessage? jobMessage;
                    try
                    {
                        jobMessage = JsonSerializer.Deserialize<JobStatusChangeMessage>(messageJson);
                        if (jobMessage == null)
                        {
                            _logger.LogWarning("Failed to deserialize null message from Redis: {MessageJson}", messageJson);
                            return;
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize message from Redis: {MessageJson}", messageJson);
                        return;
                    }

                    // Log with correlation ID as structured property
                    _logger.LogDebug(
                        "Received job status change: UserId={UserId}, JobId={JobId}, Status={Status}, CorrelationId={CorrelationId}",
                        jobMessage.UserId, jobMessage.JobId, jobMessage.Status, jobMessage.CorrelationId ?? "unknown");

                    // Check if this user is connected to THIS API instance
                    if (_channelStore.TryGet(jobMessage.UserId, out var userChannel))
                    {
                        // Format as SSE message
                        var sseMessage = FormatSseMessage(jobMessage);

                        // Attempt to write to the user's channel (non-blocking)
                        if (userChannel!.Channel.Writer.TryWrite(sseMessage))
                        {
                            // Update LastActivity only on successful write
                            userChannel.UpdateLastActivity();

                            _logger.LogDebug(
                                "Message sent to SSE channel for userId {UserId}, JobId={JobId}, CorrelationId={CorrelationId}",
                                jobMessage.UserId, jobMessage.JobId, jobMessage.CorrelationId ?? "unknown");
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Failed to write to channel for userId {UserId} (channel may be full or completed), CorrelationId={CorrelationId}",
                                jobMessage.UserId, jobMessage.CorrelationId ?? "unknown");
                        }
                    }
                    else
                    {
                        // User not connected to this API instance - this is normal in horizontal scaling
                        _logger.LogDebug(
                            "User {UserId} not connected to this instance, message ignored, CorrelationId={CorrelationId}",
                            jobMessage.UserId, jobMessage.CorrelationId ?? "unknown");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Redis message on channel {ChannelName}", _channelName);
                }
            });

            _logger.LogInformation("RedisSubscriberService subscribed successfully to channel: {ChannelName}", _channelName);

            // Keep the service running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RedisSubscriberService stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RedisSubscriberService");
            throw;
        }
    }

    /// <summary>
    /// Formats a job status change message as an SSE event.
    /// </summary>
    private static string FormatSseMessage(JobStatusChangeMessage msg)
    {
        var json = JsonSerializer.Serialize(msg, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return $"event: jobStatusChange\nid: {msg.JobId}\ndata: {json}\n\n";
    }

    /// <summary>
    /// Disposes resources when the service is stopped.
    /// </summary>
    public override void Dispose()
    {
        _logger.LogInformation("RedisSubscriberService disposed");
        base.Dispose();
    }
}
