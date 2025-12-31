using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Accounts.Commands;
using CoreLedger.Application.UseCases.Accounts.Queries;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Models;
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
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountsController> _logger;
    private readonly IMediator _mediator;

    public AccountsController(
        IMediator mediator,
        ILogger<AccountsController> logger,
        IAccountRepository accountRepository)
    {
        _mediator = mediator;
        _logger = logger;
        _accountRepository = accountRepository;
    }

    /// <summary>
    ///     Gets a report of total active accounts grouped by account type.
    /// </summary>
    [HttpGet("reports/by-type")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountsByTypeReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountsByTypeReport(CancellationToken cancellationToken)
    {
        var query = new GetAccountsByTypeReportQuery();
        var result = await _mediator.Send(query, cancellationToken);
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
        var (accounts, totalCount) = await _accountRepository.GetWithQueryAsync(parameters, cancellationToken);

        var accountDtos = accounts.Select(a => new AccountDto(
            a.Id,
            a.Code,
            a.Name,
            a.TypeId,
            a.Type?.Description ?? string.Empty,
            a.Status,
            a.Status.ToString(),
            a.NormalBalance,
            a.NormalBalance.ToString(),
            a.CreatedAt,
            a.UpdatedAt,
            a.DeactivatedAt
        )).ToList();

        var result = new PagedResult<AccountDto>(accountDtos, totalCount, parameters.Limit, parameters.Offset);

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
        var query = new GetAccountByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
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
            _logger.LogWarning("User claim 'sub' not found in token");
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        var command = new CreateAccountCommand(
            dto.Code,
            dto.Name,
            dto.TypeId,
            dto.Status,
            dto.NormalBalance,
            userId);
        var result = await _mediator.Send(command, cancellationToken);
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
        var command = new UpdateAccountCommand(
            id,
            dto.Code,
            dto.Name,
            dto.TypeId,
            dto.Status,
            dto.NormalBalance);
        await _mediator.Send(command, cancellationToken);
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
        var command = new DeactivateAccountCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}