using System.Text;
using System.Text.Json;
using CoreLedger.Application.Constants;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CoreLedger.Worker.Services;

/// <summary>
/// Background service that consumes B3 import messages from RabbitMQ.
/// </summary>
public class B3ImportConsumer : BackgroundService
{
    private readonly ILogger<B3ImportConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public B3ImportConsumer(
        ILogger<B3ImportConsumer> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("B3ImportConsumer starting");

        var hostname = _configuration["RabbitMQ:Hostname"] ?? "localhost";
        var port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
        var username = _configuration["RabbitMQ:Username"] ?? "guest";
        var password = _configuration["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = hostname,
            Port = port,
            UserName = username,
            Password = password,
            DispatchConsumersAsync = true
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: QueueNames.B3Import,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                // Extract correlation ID from message headers or properties
                var correlationId = ea.BasicProperties?.CorrelationId;
                if (string.IsNullOrWhiteSpace(correlationId) && ea.BasicProperties?.Headers != null)
                {
                    if (ea.BasicProperties.Headers.TryGetValue("X-Correlation-ID", out var headerValue))
                    {
                        correlationId = Encoding.UTF8.GetString((byte[])headerValue);
                    }
                }

                // Set up Serilog LogContext with correlation ID for distributed tracing
                using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId ?? "unknown"))
                {
                    _logger.LogInformation("Received message from {QueueName}: {Message}", QueueNames.B3Import, messageJson);

                    try
                    {
                        var message = JsonSerializer.Deserialize<CoreJobB3ImportMessage>(messageJson);
                        if (message == null)
                        {
                            _logger.LogError("Failed to deserialize message: {MessageJson}", messageJson);
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                            return;
                        }

                        if (message.CommandType != "CoreJobB3Import")
                        {
                            _logger.LogWarning("Unexpected command type: {CommandType}", message.CommandType);
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                            return;
                        }

                        using var scope = _serviceProvider.CreateScope();
                        var processor = scope.ServiceProvider.GetRequiredService<IB3ImportProcessor>();

                        await processor.ProcessAsync(message.CoreJobId, message.ReferenceId, stoppingToken);

                        _channel.BasicAck(ea.DeliveryTag, false);
                        _logger.LogInformation("Successfully processed B3 import for CoreJob {CoreJobId}", message.CoreJobId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message: {Message}", messageJson);
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                }
            };

            _channel.BasicConsume(queue: QueueNames.B3Import, autoAck: false, consumer: consumer);

            _logger.LogInformation("B3ImportConsumer started and listening on queue: {QueueName}", QueueNames.B3Import);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in B3ImportConsumer");
            throw;
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
        _logger.LogInformation("B3ImportConsumer disposed");
    }
}
