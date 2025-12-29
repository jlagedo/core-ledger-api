using Microsoft.AspNetCore.Mvc;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Models;

namespace CoreLedger.API.Controllers;

/// <summary>
/// Controller for managing CoreJob resources.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CoreJobsController : ControllerBase
{
    private readonly ILogger<CoreJobsController> _logger;
    private readonly ICoreJobRepository _coreJobRepository;

    public CoreJobsController(
        ILogger<CoreJobsController> logger,
        ICoreJobRepository coreJobRepository)
    {
        _logger = logger;
        _coreJobRepository = coreJobRepository;
    }

    /// <summary>
    /// Retrieves all core jobs with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="limit">Maximum number of items to return (max 100)</param>
    /// <param name="offset">Number of items to skip</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortDirection">Sort direction (asc or desc)</param>
    /// <param name="filter">Filter expression (field=value)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CoreJobDto>), StatusCodes.Status200OK)]
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
        var (jobs, totalCount) = await _coreJobRepository.GetWithQueryAsync(parameters, cancellationToken);

        var jobDtos = jobs.Select(j => new CoreJobDto(
            j.Id,
            j.ReferenceId,
            j.Status,
            j.Status.ToString(),
            j.JobDescription,
            j.CreationDate,
            j.RunningDate,
            j.FinishedDate,
            j.CreatedAt,
            j.UpdatedAt
        )).ToList();

        var result = new PagedResult<CoreJobDto>(jobDtos, totalCount, parameters.Limit, parameters.Offset);

        return Ok(result);
    }
}
