using CoreLedger.Application.Interfaces;
using CoreLedger.Infrastructure.Services;

namespace CoreLedger.API.Extensions;

/// <summary>
/// Extension methods for configuring notification services.
/// </summary>
public static class NotificationExtensions
{
    /// <summary>
    /// Registers notification services for Server-Sent Events (SSE) support with Redis Pub/Sub.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to read settings from.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register channel store as singleton (shared across all requests and background services)
        services.AddSingleton<INotificationChannelStore, NotificationChannelStore>();

        // Register background services for SSE support
        services.AddHostedService<RedisSubscriberService>();
        services.AddHostedService<HeartbeatService>();
        services.AddHostedService<ChannelCleanupService>();

        return services;
    }
}
