using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Securities.Commands;
using CoreLedger.Application.UseCases.Securities.Queries;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Models;
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
    private readonly ISecurityRepository _securityRepository;

    public SecuritiesController(
        IMediator mediator,
        ILogger<SecuritiesController> logger,
        ISecurityRepository securityRepository)
    {
        _mediator = mediator;
        _logger = logger;
        _securityRepository = securityRepository;
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
        var (securities, totalCount) = await _securityRepository.GetWithQueryAsync(parameters, cancellationToken);

        var securityDtos = securities.Select(s => new SecurityDto(
            s.Id,
            s.Name,
            s.Ticker,
            s.Isin,
            s.Type,
            s.Type.ToString(),
            s.Currency,
            s.Status,
            s.Status.ToString(),
            s.CreatedAt,
            s.UpdatedAt,
            s.DeactivatedAt
        )).ToList();

        var result = new PagedResult<SecurityDto>(securityDtos, totalCount, parameters.Limit, parameters.Offset);

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
        var query = new GetSecurityByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
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
            _logger.LogWarning("User claim 'sub' not found in token");
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        var command = new CreateSecurityCommand(
            dto.Name,
            dto.Ticker,
            dto.Isin,
            dto.Type,
            dto.Currency,
            userId);
        var result = await _mediator.Send(command, cancellationToken);
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
        var command = new UpdateSecurityCommand(
            id,
            dto.Name,
            dto.Ticker,
            dto.Isin,
            dto.Type,
            dto.Currency);
        await _mediator.Send(command, cancellationToken);
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