using MediatR;
using Microsoft.AspNetCore.Mvc;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.TransactionStatuses.Queries;

namespace CoreLedger.API.Controllers;

/// <summary>
/// Controller for managing TransactionStatus resources.
/// </summary>
[ApiController]
[Route("api/transactions/status")]
public class TransactionStatusesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionStatusesController> _logger;

    public TransactionStatusesController(IMediator mediator, ILogger<TransactionStatusesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all transaction statuses.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllTransactionStatusesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific transaction status by ID.
    /// </summary>
    [HttpGet("{id}", Name = "GetTransactionStatusById")]
    [ProducesResponseType(typeof(TransactionStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetTransactionStatusByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
