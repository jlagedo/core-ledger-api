using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.Funds.Commands;
using CoreLedger.Application.UseCases.Funds.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Controllers;

/// <summary>
///     Controller for managing Fund resources.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FundsController : ControllerBase
{
    private readonly ILogger<FundsController> _logger;
    private readonly IMediator _mediator;

    public FundsController(
        IMediator mediator,
        ILogger<FundsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieves all funds with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="limit">Maximum number of items to return (max 100)</param>
    /// <param name="offset">Number of items to skip</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortDirection">Sort direction (asc or desc)</param>
    /// <param name="filter">Filter expression (field=value)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FundDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFundsWithQueryQuery(
            limit,
            offset,
            sortBy,
            sortDirection,
            filter);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    ///     Retrieves a specific fund by ID.
    /// </summary>
    [HttpGet("{id}", Name = "GetFundById")]
    [ProducesResponseType(typeof(FundDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetFundByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    ///     Creates a new fund.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FundDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFundDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User claim 'sub' not found in token");
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        var command = new CreateFundCommand(
            dto.Code,
            dto.Name,
            dto.BaseCurrency,
            dto.InceptionDate,
            dto.ValuationFrequency,
            userId);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtRoute("GetFundById", new { id = result.Id }, result);
    }

    /// <summary>
    ///     Updates an existing fund.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateFundDto dto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateFundCommand(
            id,
            dto.Code,
            dto.Name,
            dto.BaseCurrency,
            dto.InceptionDate,
            dto.ValuationFrequency);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}