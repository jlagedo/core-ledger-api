using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.Securities.Commands;
using CoreLedger.Application.UseCases.Securities.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Controllers;

/// <summary>
///     Controller for managing Security resources.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SecuritiesController : ControllerBase
{
    private readonly ILogger<SecuritiesController> _logger;
    private readonly IMediator _mediator;

    public SecuritiesController(
        IMediator mediator,
        ILogger<SecuritiesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieves all securities with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="limit">Maximum number of items to return (max 100)</param>
    /// <param name="offset">Number of items to skip</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortDirection">Sort direction (asc or desc)</param>
    /// <param name="filter">Filter expression (field=value)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SecurityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst("sub")?.Value;
        _logger.LogInformation(
            "Retrieving securities - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}, User: {UserId}",
            limit, offset, sortBy ?? "none", filter ?? "none", userId);

        var query = new GetSecuritiesWithQueryQuery(
            limit,
            offset,
            sortBy,
            sortDirection,
            filter);

        var result = await _mediator.Send(query, cancellationToken);

        _logger.LogInformation(
            "Securities retrieved - Returned: {Count} of {Total} total securities",
            result.Data.Count, result.TotalCount);
        return Ok(result);
    }

    /// <summary>
    ///     Retrieves a specific security by ID.
    /// </summary>
    [HttpGet("{id}", Name = "GetSecuritiesById")]
    [ProducesResponseType(typeof(SecurityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        _logger.LogInformation("Retrieving security {SecurityId} for user {UserId}", id, userId);

        var query = new GetSecurityByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        _logger.LogInformation("Security retrieved - Name: {Name}, Ticker: {Ticker}, Type: {Type}", result.Name, result.Ticker, result.Type);
        return Ok(result);
    }

    /// <summary>
    ///     Creates a new security.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SecurityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSecurityDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("Authentication failed: 'sub' claim missing from token for endpoint {Endpoint}", HttpContext.Request.Path);
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        _logger.LogInformation(
            "Creating security - Name: {Name}, Ticker: {Ticker}, Isin: {Isin}, Type: {Type}, Currency: {Currency}, CreatedBy: {UserId}",
            dto.Name, dto.Ticker, dto.Isin, dto.Type, dto.Currency, userId);

        var command = new CreateSecurityCommand(
            dto.Name,
            dto.Ticker,
            dto.Isin,
            dto.Type,
            dto.Currency,
            userId);
        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Security created successfully - Id: {SecurityId}, Name: {Name}", result.Id, result.Name);
        return CreatedAtRoute("GetSecuritiesById", new { id = result.Id }, result);
    }

    /// <summary>
    ///     Updates an existing security.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSecurityDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        _logger.LogInformation(
            "Updating security {SecurityId} - Name: {Name}, Ticker: {Ticker}, Type: {Type}, Currency: {Currency}, UpdatedBy: {UserId}",
            id, dto.Name, dto.Ticker, dto.Type, dto.Currency, userId);

        var command = new UpdateSecurityCommand(
            id,
            dto.Name,
            dto.Ticker,
            dto.Isin,
            dto.Type,
            dto.Currency);
        await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Security updated successfully - Id: {SecurityId}", id);
        return NoContent();
    }

    /// <summary>
    ///     Deactivates a security and records the deactivation date.
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var command = new DeactivateSecurityCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}