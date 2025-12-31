using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.TransactionSubTypes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Controllers;

/// <summary>
///     Controller for managing TransactionSubType resources.
/// </summary>
[Authorize]
[ApiController]
[Route("api/transactions/subtypes")]
public class TransactionSubTypesController : ControllerBase
{
    private readonly ILogger<TransactionSubTypesController> _logger;
    private readonly IMediator _mediator;

    public TransactionSubTypesController(IMediator mediator, ILogger<TransactionSubTypesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieves all transaction subtypes with optional filtering by type ID.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionSubTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? typeId, CancellationToken cancellationToken)
    {
        var query = new GetAllTransactionSubTypesQuery(typeId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    ///     Retrieves a specific transaction subtype by ID.
    /// </summary>
    [HttpGet("{id}", Name = "GetTransactionSubTypeById")]
    [ProducesResponseType(typeof(TransactionSubTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetTransactionSubTypeByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}