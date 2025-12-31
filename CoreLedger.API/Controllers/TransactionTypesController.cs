using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.TransactionTypes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Controllers;

/// <summary>
///     Controller for managing TransactionType resources.
/// </summary>
[Authorize]
[ApiController]
[Route("api/transactions/types")]
public class TransactionTypesController : ControllerBase
{
    private readonly ILogger<TransactionTypesController> _logger;
    private readonly IMediator _mediator;

    public TransactionTypesController(IMediator mediator, ILogger<TransactionTypesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieves all transaction types.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllTransactionTypesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    ///     Retrieves a specific transaction type by ID.
    /// </summary>
    [HttpGet("{id}", Name = "GetTransactionTypeById")]
    [ProducesResponseType(typeof(TransactionTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetTransactionTypeByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}