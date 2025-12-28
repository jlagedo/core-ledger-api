using Microsoft.AspNetCore.Mvc;
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
    /// </summary>
    /// <param name="request">B3 import request containing reference ID and job description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import response with CoreJob details</returns>
    [HttpPost("import-b3-instruction-file")]
    [ProducesResponseType(typeof(ImportB3InstructionFileResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportB3InstructionFile(
        [FromBody] ImportB3InstructionFileRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting B3 instruction file import for ReferenceId: {ReferenceId}", request.ReferenceId);

        var coreJob = CoreJob.Create(
            referenceId: request.ReferenceId,
            jobDescription: request.JobDescription);

        await _coreJobRepository.AddAsync(coreJob, cancellationToken);

        _logger.LogInformation("CoreJob created with Id: {CoreJobId}", coreJob.Id);

        var message = new CoreJobB3ImportMessage(
            CoreJobId: coreJob.Id,
            ReferenceId: coreJob.ReferenceId,
            CommandType: "CoreJobB3Import");

        await _messagePublisher.PublishAsync(
            queueName: "worker.b3.import.queue",
            message: message,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Message published to worker.b3.import.queue for CoreJobId: {CoreJobId}", coreJob.Id);

        var response = new ImportB3InstructionFileResponse(
            CoreJobId: coreJob.Id,
            ReferenceId: coreJob.ReferenceId,
            Status: "Accepted",
            Message: "B3 instruction file import job has been queued successfully");

        return AcceptedAtAction(
            actionName: null,
            value: response);
    }
}
