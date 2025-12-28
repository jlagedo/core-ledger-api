using System.Text;
using System.Text.Json;
using CoreLedger.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
/// RabbitMQ implementation of the message publisher.
/// </summary>
public class RabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly ILogger<RabbitMQPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private bool _disposed;

    public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
    {
        _logger = logger;

        var hostname = configuration["RabbitMQ:Hostname"] ?? "localhost";
        var port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        var username = configuration["RabbitMQ:Username"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = hostname,
            Port = port,
            UserName = username,
            Password = password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _logger.LogInformation("RabbitMQ connection established to {Hostname}:{Port}", hostname, port);
    }

    /// <summary>
    /// Publishes a message to the specified queue.
    /// </summary>
    public Task PublishAsync<T>(string queueName, T message, string? correlationId = null, CancellationToken cancellationToken = default) where T : class
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        // Add correlation ID to message headers for distributed tracing
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            properties.CorrelationId = correlationId;
            properties.Headers ??= new Dictionary<string, object>();
            properties.Headers["X-Correlation-ID"] = correlationId;
        }

        _channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Message published to queue {QueueName} with CorrelationId {CorrelationId}: {Message}",
            queueName, correlationId ?? "none", json);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _channel?.Dispose();
        _connection?.Dispose();
        _disposed = true;

        _logger.LogInformation("RabbitMQ connection disposed");
    }
}
