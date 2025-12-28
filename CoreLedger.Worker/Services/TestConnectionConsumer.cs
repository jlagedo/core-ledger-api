using System.Text;
using System.Text.Json;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CoreLedger.Worker.Services;

/// <summary>
/// Background service that consumes test connection messages from RabbitMQ.
/// This consumer is used to test the API -> Queue -> Worker flow.
/// </summary>
public class TestConnectionConsumer : BackgroundService
{
    private readonly ILogger<TestConnectionConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private const string QueueName = "worker.test.queue";

    public TestConnectionConsumer(
        ILogger<TestConnectionConsumer> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TestConnectionConsumer starting");

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
                queue: QueueName,
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
                    _logger.LogInformation("========================================");
                    _logger.LogInformation("TEST CONNECTION MESSAGE RECEIVED");
                    _logger.LogInformation("========================================");
                    _logger.LogInformation("Queue: {QueueName}", QueueName);
                    _logger.LogInformation("Correlation ID: {CorrelationId}", correlationId ?? "none");
                    _logger.LogInformation("Message: {Message}", messageJson);

                    try
                    {
                        var message = JsonSerializer.Deserialize<TestConnectionMessage>(messageJson);
                        if (message == null)
                        {
                            _logger.LogError("Failed to deserialize test message: {MessageJson}", messageJson);
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                            return;
                        }

                        _logger.LogInformation("Deserialized Message:");
                        _logger.LogInformation("  - CoreJobId: {CoreJobId}", message.CoreJobId);
                        _logger.LogInformation("  - ReferenceId: {ReferenceId}", message.ReferenceId);
                        _logger.LogInformation("  - CommandType: {CommandType}", message.CommandType);
                        _logger.LogInformation("  - CorrelationId: {CorrelationId}", message.CorrelationId ?? "none");

                        // Update CoreJob status to Running
                        using var scope = _serviceProvider.CreateScope();
                        var coreJobRepository = scope.ServiceProvider.GetRequiredService<ICoreJobRepository>();

                        var coreJob = await coreJobRepository.GetByIdAsync(message.CoreJobId, stoppingToken);
                        if (coreJob == null)
                        {
                            _logger.LogError("CoreJob not found with Id: {CoreJobId}", message.CoreJobId);
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                            return;
                        }

                        _logger.LogInformation("CoreJob found - Id: {CoreJobId}, Current Status: {Status}",
                            coreJob.Id, coreJob.Status);

                        // Update status to Running
                        coreJob.UpdateStatus(JobStatus.Running, runningDate: DateTime.UtcNow);
                        await coreJobRepository.UpdateAsync(coreJob, stoppingToken);
                        _logger.LogInformation("CoreJob status updated to Running");

                        // Simulate processing
                        _logger.LogInformation("Simulating processing for 2 seconds...");
                        await Task.Delay(2000, stoppingToken);

                        // Update status to Complete
                        coreJob.UpdateStatus(JobStatus.Complete, finishedDate: DateTime.UtcNow);
                        await coreJobRepository.UpdateAsync(coreJob, stoppingToken);
                        _logger.LogInformation("CoreJob status updated to Complete");

                        _channel.BasicAck(ea.DeliveryTag, false);

                        _logger.LogInformation("========================================");
                        _logger.LogInformation("TEST CONNECTION COMPLETED SUCCESSFULLY");
                        _logger.LogInformation("CoreJobId: {CoreJobId}, Final Status: {Status}", coreJob.Id, coreJob.Status);
                        _logger.LogInformation("========================================");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing test message: {Message}", messageJson);
                        _logger.LogInformation("========================================");

                        // Update CoreJob status to Failed
                        try
                        {
                            var message = JsonSerializer.Deserialize<TestConnectionMessage>(messageJson);
                            if (message != null)
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var coreJobRepository = scope.ServiceProvider.GetRequiredService<ICoreJobRepository>();
                                var coreJob = await coreJobRepository.GetByIdAsync(message.CoreJobId, stoppingToken);
                                if (coreJob != null)
                                {
                                    coreJob.UpdateStatus(JobStatus.Failed, finishedDate: DateTime.UtcNow);
                                    await coreJobRepository.UpdateAsync(coreJob, stoppingToken);
                                    _logger.LogInformation("CoreJob status updated to Failed");
                                }
                            }
                        }
                        catch (Exception updateEx)
                        {
                            _logger.LogError(updateEx, "Failed to update CoreJob status to Failed");
                        }

                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            _logger.LogInformation("TestConnectionConsumer started and listening on queue: {QueueName}", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TestConnectionConsumer");
            throw;
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
        _logger.LogInformation("TestConnectionConsumer disposed");
    }
}
