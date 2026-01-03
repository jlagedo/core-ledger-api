using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.Transactions.Commands;
using CoreLedger.Application.UseCases.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Controllers;

/// <summary>
///     Controller for managing Transaction resources.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ILogger<TransactionsController> _logger;
    private readonly IMediator _mediator;

    public TransactionsController(
        IMediator mediator,
        ILogger<TransactionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieves all transactions with optional filtering, sorting, and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTransactionsWithQueryQuery(
            limit,
            offset,
            sortBy,
            sortDirection,
            filter);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    ///     Retrieves a specific transaction by ID.
    /// </summary>
    [HttpGet("{id}", Name = "GetTransactionById")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetTransactionByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    ///     Creates a new transaction.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User claim 'sub' not found in token");
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        // Extract correlation ID and request ID from HttpContext for audit logging
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        var requestId = HttpContext.TraceIdentifier;

        var command = new CreateTransactionCommand(
            dto.FundId,
            dto.SecurityId,
            dto.TransactionSubTypeId,
            dto.TradeDate,
            dto.SettleDate,
            dto.Quantity,
            dto.Price,
            dto.Amount,
            dto.Currency,
            dto.StatusId,
            userId,
            correlationId,
            requestId);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtRoute("GetTransactionById", new { id = result.Id }, result);
    }

    /// <summary>
    ///     Updates an existing transaction.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateTransactionDto dto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTransactionCommand(
            id,
            dto.FundId,
            dto.SecurityId,
            dto.TransactionSubTypeId,
            dto.TradeDate,
            dto.SettleDate,
            dto.Quantity,
            dto.Price,
            dto.Amount,
            dto.Currency,
            dto.StatusId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}