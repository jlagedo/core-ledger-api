using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.AuditLogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Controllers;

/// <summary>
///     Controller for retrieving audit log entries.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly ILogger<AuditLogsController> _logger;
    private readonly IMediator _mediator;

    public AuditLogsController(
        IMediator mediator,
        ILogger<AuditLogsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieves audit log entries with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="limit">Maximum number of items to return (max 100).</param>
    /// <param name="offset">Number of items to skip.</param>
    /// <param name="sortBy">Field to sort by (id, entityName, entityId, eventType, performedAt, source).</param>
    /// <param name="sortDirection">Sort direction (asc or desc). Default: desc.</param>
    /// <param name="filter">Filter expression (field=value). Supported fields: entityName, entityId, eventType, performedByUserId, source, correlationId.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of audit log entries.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "desc",
        [FromQuery] string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsWithQueryQuery(
            limit,
            offset,
            sortBy,
            sortDirection,
            filter);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
