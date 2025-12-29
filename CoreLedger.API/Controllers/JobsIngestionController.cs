using Microsoft.AspNetCore.Mvc;
using CoreLedger.Application.Constants;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;

namespace CoreLedger.API.Controllers;

/// <summary>
/// Controller for job ingestion operations.
/// </summary>
[ApiController]
[Route("api/jobs-ingestion")]
public class JobsIngestionController : ControllerBase
{
    private readonly ILogger<JobsIngestionController> _logger;
    private readonly ICoreJobRepository _coreJobRepository;
    private readonly IMessagePublisher _messagePublisher;

    public JobsIngestionController(
        ILogger<JobsIngestionController> logger,
        ICoreJobRepository coreJobRepository,
        IMessagePublisher messagePublisher)
    {
        _logger = logger;
        _coreJobRepository = coreJobRepository;
        _messagePublisher = messagePublisher;
    }

    /// <summary>
    /// Imports a B3 instruction file by creating a CoreJob and sending a message to RabbitMQ.
    /// The job reference ID and description are auto-generated with the current datetime.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import response with CoreJob details</returns>
    [HttpPost("import-b3-instruction-file")]
    [ProducesResponseType(typeof(ImportB3InstructionFileResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportB3InstructionFile(
        CancellationToken cancellationToken = default)
    {
        // Extract correlation ID from HttpContext (set by CorrelationIdMiddleware)
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

        // Auto-generate reference ID in format: CJB3-YYYYMMDDHHMMSS-XXXXXX (with unique suffix to prevent collisions)
        var now = DateTime.UtcNow;
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..6]; // 6-character unique suffix
        var referenceId = $"CJB3-{now:yyyyMMddHHmmss}-{uniqueSuffix}";

        // Auto-generate job description with current datetime
        var jobDescription = $"B3 import initialization started at {now:yyyy-MM-dd HH:mm:ss} UTC";

        _logger.LogInformation("Starting B3 instruction file import for ReferenceId: {ReferenceId}", referenceId);

        var coreJob = CoreJob.Create(
            referenceId: referenceId,
            jobDescription: jobDescription);

        await _coreJobRepository.AddAsync(coreJob, cancellationToken);

        _logger.LogInformation("CoreJob created with Id: {CoreJobId}", coreJob.Id);

        var message = new CoreJobB3ImportMessage(
            CoreJobId: coreJob.Id,
            ReferenceId: coreJob.ReferenceId,
            CommandType: "CoreJobB3Import",
            CorrelationId: correlationId);

        await _messagePublisher.PublishAsync(
            queueName: QueueNames.B3Import,
            message: message,
            correlationId: correlationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Message published to {QueueName} for CoreJobId: {CoreJobId}", QueueNames.B3Import, coreJob.Id);

        var response = new ImportB3InstructionFileResponse(
            CoreJobId: coreJob.Id,
            ReferenceId: coreJob.ReferenceId,
            Status: "Accepted",
            Message: "B3 instruction file import job has been queued successfully");

        return AcceptedAtAction(
            actionName: null,
            value: response);
    }

    /// <summary>
    /// Tests the API -> Queue -> Worker connection by creating a CoreJob and sending a test message.
    /// The worker will only log the message and update the job status.
    /// </summary>
    /// <param name="request">Test connection request containing reference ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test response with CoreJob details and correlation ID</returns>
    [HttpPost("test-connection")]
    [ProducesResponseType(typeof(TestConnectionResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestConnection(
        [FromBody] TestConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        // Extract correlation ID from HttpContext (set by CorrelationIdMiddleware)
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

        _logger.LogInformation("Testing API -> Queue -> Worker connection for ReferenceId: {ReferenceId}", request.ReferenceId);

        var coreJob = CoreJob.Create(
            referenceId: request.ReferenceId,
            jobDescription: request.JobDescription ?? "Test connection job");

        await _coreJobRepository.AddAsync(coreJob, cancellationToken);

        _logger.LogInformation("Test CoreJob created with Id: {CoreJobId}, Status: {Status}", coreJob.Id, coreJob.Status);

        var message = new TestConnectionMessage(
            CoreJobId: coreJob.Id,
            ReferenceId: coreJob.ReferenceId,
            CommandType: "TestConnection",
            CorrelationId: correlationId);

        await _messagePublisher.PublishAsync(
            queueName: QueueNames.TestConnection,
            message: message,
            correlationId: correlationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Test message published to {QueueName} for CoreJobId: {CoreJobId}", QueueNames.TestConnection, coreJob.Id);

        var response = new TestConnectionResponse(
            CoreJobId: coreJob.Id,
            ReferenceId: coreJob.ReferenceId,
            Status: coreJob.Status.ToString(),
            Message: "Test connection job has been queued successfully. Check worker logs to verify message processing.",
            CorrelationId: correlationId);

        return AcceptedAtAction(
            actionName: null,
            value: response);
    }
}
