using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Securities.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Controllers;

/// <summary>
///     Controller for retrieving SecurityType enum values.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SecurityTypesController : ControllerBase
{
    private readonly ILogger<SecurityTypesController> _logger;
    private readonly IMediator _mediator;

    public SecurityTypesController(IMediator mediator, ILogger<SecurityTypesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieves all SecurityType enum values.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SecurityTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllSecurityTypesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}