using System.Text;
using System.Text.Json;
using CoreLedger.Application.Interfaces;
using CoreLedger.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
/// RabbitMQ implementation of the message publisher.
/// </summary>
public class RabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly ILogger<RabbitMQPublisher> _logger;
    private readonly RabbitMQOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private bool _disposed;

    public RabbitMQPublisher(IOptions<RabbitMQOptions> options, ILogger<RabbitMQPublisher> logger)
    {
        _logger = logger;
        _options = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = _options.Hostname,
            Port = int.Parse(_options.Port),
            UserName = _options.Username,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _logger.LogInformation("RabbitMQ connection established to {Hostname}:{Port}", _options.Hostname, _options.Port);
    }

    /// <summary>
    /// Publishes a message to the specified queue.
    /// </summary>
    public Task PublishAsync<T>(string queueName, T message, string? correlationId = null, CancellationToken cancellationToken = default) where T : class
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: _options.QueueDurable,
            exclusive: _options.QueueExclusive,
            autoDelete: _options.QueueAutoDelete,
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
