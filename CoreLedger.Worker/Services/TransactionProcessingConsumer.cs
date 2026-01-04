using System.Net.Http.Json;
using System.Text;
using CoreLedger.Application.Events;
using CoreLedger.Application.UseCases.Transactions.Commands;
using CoreLedger.Infrastructure.Configuration;
using CoreLedger.Worker.Configuration;
using MediatR;
using Microsoft.Extensions.Options;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog.Context;

namespace CoreLedger.Worker.Services;

/// <summary>
/// Background service that consumes transaction created events from RabbitMQ,
/// validates domain rules, updates transaction status, and notifies the API.
/// </summary>
public class TransactionProcessingConsumer : BackgroundService
{
    private readonly ILogger<TransactionProcessingConsumer> _logger;
    private readonly RabbitMQOptions _rabbitMQOptions;
    private readonly QueueNamesOptions _queueNames;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private IModel? _channel;
    private IConnection? _connection;

    public TransactionProcessingConsumer(
        ILogger<TransactionProcessingConsumer> logger,
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        IOptions<RabbitMQOptions> rabbitMQOptions,
        IOptions<QueueNamesOptions> queueNames)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _rabbitMQOptions = rabbitMQOptions.Value;
        _queueNames = queueNames.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TransactionProcessingConsumer starting");

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMQOptions.Hostname,
            Port = int.Parse(_rabbitMQOptions.Port),
            UserName = _rabbitMQOptions.Username,
            Password = _rabbitMQOptions.Password,
            DispatchConsumersAsync = true
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                _queueNames.TransactionCreated,
                _rabbitMQOptions.QueueDurable,
                _rabbitMQOptions.QueueExclusive,
                _rabbitMQOptions.QueueAutoDelete,
                null);

            _channel.BasicQos(
                _rabbitMQOptions.PrefetchSize,
                _rabbitMQOptions.PrefetchCount,
                false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();

                // Extract correlation ID from message headers or properties
                var correlationId = ea.BasicProperties?.CorrelationId;
                if (string.IsNullOrWhiteSpace(correlationId) && ea.BasicProperties?.Headers != null)
                    if (ea.BasicProperties.Headers.TryGetValue("X-Correlation-ID", out var headerValue))
                        correlationId = Encoding.UTF8.GetString((byte[])headerValue);

                // Set up Serilog LogContext with correlation ID for distributed tracing
                using (LogContext.PushProperty("CorrelationId", correlationId ?? "unknown"))
                {
                    try
                    {
                        // 1. Deserialize Protobuf message
                        TransactionCreatedEvent transactionEvent;
                        using (var memoryStream = new MemoryStream(body))
                        {
                            transactionEvent = Serializer.Deserialize<TransactionCreatedEvent>(memoryStream);
                        }

                        _logger.LogInformation(
                            "Transaction message received - TransactionId: {TransactionId}, " +
                            "FundCode: {FundCode}, Amount: {Amount} {Currency}",
                            transactionEvent.TransactionId,
                            transactionEvent.FundCode,
                            transactionEvent.Amount,
                            transactionEvent.Currency);

                        // 2. Process transaction via MediatR
                        using var scope = _serviceProvider.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        var command = new ProcessTransactionCommand(
                            transactionEvent.TransactionId,
                            correlationId ?? "unknown");

                        var result = await mediator.Send(command, stoppingToken);

                        _logger.LogInformation(
                            "Transaction processing completed - TransactionId: {TransactionId}, " +
                            "Success: {Success}, Status: {Status}",
                            result.TransactionId, result.Success, result.FinalStatusId);

                        // 3. HTTP callback to API (best-effort notification)
                        try
                        {
                            await NotifyApiAsync(result, correlationId ?? "unknown", stoppingToken);
                        }
                        catch (Exception notifyEx)
                        {
                            _logger.LogWarning(notifyEx,
                                "Failed to notify API of transaction processing - TransactionId: {TransactionId}",
                                result.TransactionId);
                            // Don't fail the message - notification is best-effort
                        }

                        // 4. Acknowledge message
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error processing transaction message - Payload size: {PayloadSize} bytes",
                            body.Length);

                        // Requeue for retry on unexpected errors
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                }
            };

            _channel.BasicConsume(_queueNames.TransactionCreated, false, consumer);

            _logger.LogInformation(
                "TransactionProcessingConsumer started and listening on queue: {QueueName}",
                _queueNames.TransactionCreated);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TransactionProcessingConsumer");
            throw;
        }
    }

    /// <summary>
    /// Notifies the API of transaction processing completion via HTTP POST.
    /// </summary>
    private async Task NotifyApiAsync(
        ProcessTransactionResult result,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("WorkerHttpClient");

        var notification = new
        {
            TransactionId = result.TransactionId,
            Success = result.Success,
            FinalStatusId = result.FinalStatusId,
            ErrorMessage = result.ErrorMessage,
            ProcessedAt = DateTime.UtcNow,
            CorrelationId = correlationId
        };

        var response = await httpClient.PostAsJsonAsync(
            "/api/worker-notifications/transaction-processed",
            notification,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation(
            "API notification sent successfully - TransactionId: {TransactionId}, StatusCode: {StatusCode}",
            result.TransactionId, response.StatusCode);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
        _logger.LogInformation("TransactionProcessingConsumer disposed");
    }
}
