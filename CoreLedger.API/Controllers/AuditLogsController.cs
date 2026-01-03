using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Models;
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
    private readonly IAuditLogQueryService _auditLogQueryService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IAuditLogQueryService auditLogQueryService,
        ILogger<AuditLogsController> logger)
    {
        _auditLogQueryService = auditLogQueryService;
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
        _logger.LogInformation(
            "Retrieving audit logs with limit: {Limit}, offset: {Offset}, sortBy: {SortBy}, sortDirection: {SortDirection}, filter: {Filter}",
            limit, offset, sortBy, sortDirection, filter);

        var parameters = new QueryParameters
        {
            Limit = limit,
            Offset = offset,
            SortBy = sortBy,
            SortDirection = sortDirection,
            Filter = filter
        };

        var (auditLogs, totalCount) = await _auditLogQueryService.GetWithQueryAsync(parameters, cancellationToken);

        var auditLogDtos = auditLogs.Select(a => new AuditLogDto(
            a.Id,
            a.EntityName,
            a.EntityId,
            a.EventType,
            a.PerformedByUserId,
            a.PerformedAt,
            a.DataBefore?.RootElement.Clone(),
            a.DataAfter?.RootElement.Clone(),
            a.CorrelationId,
            a.RequestId,
            a.Source
        )).ToList();

        var result = new PagedResult<AuditLogDto>(auditLogDtos, totalCount, parameters.Limit, parameters.Offset);

        _logger.LogInformation("Retrieved {Count} audit logs out of {TotalCount} total", auditLogDtos.Count, totalCount);

        return Ok(result);
    }
}
