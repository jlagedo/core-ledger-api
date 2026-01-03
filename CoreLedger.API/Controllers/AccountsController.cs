using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.Accounts.Commands;
using CoreLedger.Application.UseCases.Accounts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Controllers;

/// <summary>
///     Controller for managing Account resources.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly IMediator _mediator;

    public AccountsController(
        IMediator mediator,
        ILogger<AccountsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    ///     Gets a report of total active accounts grouped by account type.
    /// </summary>
    [HttpGet("reports/by-type")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountsByTypeReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountsByTypeReport(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        _logger.LogInformation("Retrieving account type report for user {UserId}", userId);

        var query = new GetAccountsByTypeReportQuery();
        var result = await _mediator.Send(query, cancellationToken);

        _logger.LogInformation("Account type report retrieved - {Count} account types", result.Count);
        return Ok(result);
    }

    /// <summary>
    ///     Retrieves all accounts with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="limit">Maximum number of items to return (max 100)</param>
    /// <param name="offset">Number of items to skip</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortDirection">Sort direction (asc or desc)</param>
    /// <param name="filter">Filter expression (field=value)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AccountDto>), StatusCodes.Status200OK)]
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
            "Retrieving accounts - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}, User: {UserId}",
            limit, offset, sortBy ?? "none", filter ?? "none", userId);

        var query = new GetAccountsWithQueryQuery(
            limit,
            offset,
            sortBy,
            sortDirection,
            filter);

        var result = await _mediator.Send(query, cancellationToken);

        _logger.LogInformation(
            "Accounts retrieved - Returned: {Count} of {Total} total accounts",
            result.Data.Count, result.TotalCount);
        return Ok(result);
    }

    /// <summary>
    ///     Retrieves a specific account by ID.
    /// </summary>
    [HttpGet("{id}", Name = "GetAccountsById")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        _logger.LogInformation("Retrieving account {AccountId} for user {UserId}", id, userId);

        var query = new GetAccountByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        _logger.LogInformation("Account retrieved - Code: {Code}, Name: {Name}", result.Code, result.Name);
        return Ok(result);
    }

    /// <summary>
    ///     Creates a new account.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("Authentication failed: 'sub' claim missing from token for endpoint {Endpoint}", HttpContext.Request.Path);
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        _logger.LogInformation(
            "Creating account - Code: {Code}, Name: {Name}, Type: {TypeId}, NormalBalance: {NormalBalance}, CreatedBy: {UserId}",
            dto.Code, dto.Name, dto.TypeId, dto.NormalBalance, userId);

        var command = new CreateAccountCommand(
            dto.Code,
            dto.Name,
            dto.TypeId,
            dto.Status,
            dto.NormalBalance,
            userId);
        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Account created successfully - Id: {AccountId}, Code: {Code}", result.Id, result.Code);
        return CreatedAtRoute("GetAccountsById", new { id = result.Id }, result);
    }

    /// <summary>
    ///     Updates an existing account.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateAccountDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        _logger.LogInformation(
            "Updating account {AccountId} - Code: {Code}, Name: {Name}, Type: {TypeId}, NormalBalance: {NormalBalance}, UpdatedBy: {UserId}",
            id, dto.Code, dto.Name, dto.TypeId, dto.NormalBalance, userId);

        var command = new UpdateAccountCommand(
            id,
            dto.Code,
            dto.Name,
            dto.TypeId,
            dto.Status,
            dto.NormalBalance);
        await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Account updated successfully - Id: {AccountId}", id);
        return NoContent();
    }

    /// <summary>
    ///     Deactivates an account and records the deactivation date.
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value;
        _logger.LogInformation("Deactivating account {AccountId} - DeactivatedBy: {UserId}", id, userId);

        var command = new DeactivateAccountCommand(id);
        await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Account deactivated successfully - Id: {AccountId}", id);
        return NoContent();
    }
}