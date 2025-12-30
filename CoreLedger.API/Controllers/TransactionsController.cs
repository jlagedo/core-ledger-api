using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Transactions.Commands;
using CoreLedger.Application.UseCases.Transactions.Queries;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Models;

namespace CoreLedger.API.Controllers;

/// <summary>
/// Controller for managing Transaction resources.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionsController> _logger;
    private readonly ITransactionRepository _transactionRepository;

    public TransactionsController(
        IMediator mediator,
        ILogger<TransactionsController> logger,
        ITransactionRepository transactionRepository)
    {
        _mediator = mediator;
        _logger = logger;
        _transactionRepository = transactionRepository;
    }

    /// <summary>
    /// Retrieves all transactions with optional filtering, sorting, and pagination.
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
        var parameters = new QueryParameters
        {
            Limit = limit,
            Offset = offset,
            SortBy = sortBy,
            SortDirection = sortDirection,
            Filter = filter
        };

        // ARCHITECTURAL DECISION: Clean Architecture pattern intentionally bypassed for performance
        // This controller directly calls the repository for query operations with filters, ordering, and pagination.
        // Rationale: Avoiding the overhead of MediatR handlers and additional mapping layers for read-heavy operations
        // that require dynamic SQL generation. The performance benefit of direct repository access outweighs
        // the architectural purity in this specific use case. Write operations should still follow CQRS pattern.
        var (transactions, totalCount) = await _transactionRepository.GetWithQueryAsync(parameters, cancellationToken);

        var transactionDtos = transactions.Select(t => new TransactionDto(
            t.Id,
            t.FundId,
            t.Fund?.Code ?? string.Empty,
            t.Fund?.Name ?? string.Empty,
            t.SecurityId,
            t.Security?.Ticker,
            t.Security?.Name,
            t.TransactionSubTypeId,
            t.TransactionSubType?.ShortDescription ?? string.Empty,
            t.TransactionSubType?.TypeId ?? 0,
            t.TransactionSubType?.Type?.ShortDescription ?? string.Empty,
            t.TradeDate,
            t.SettleDate,
            t.Quantity,
            t.Price,
            t.Amount,
            t.Currency,
            t.StatusId,
            t.Status?.ShortDescription ?? string.Empty,
            t.CreatedAt,
            t.UpdatedAt
        )).ToList();

        var result = new PagedResult<TransactionDto>(transactionDtos, totalCount, parameters.Limit, parameters.Offset);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific transaction by ID.
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
    /// Creates a new transaction.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionDto dto,
        CancellationToken cancellationToken)
    {
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
            dto.StatusId);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtRoute("GetTransactionById", new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing transaction.
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
