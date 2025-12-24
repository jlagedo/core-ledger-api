using MediatR;
using Microsoft.AspNetCore.Mvc;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Accounts.Commands;
using CoreLedger.Application.UseCases.Accounts.Queries;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Models;

namespace CoreLedger.API.Controllers;

/// <summary>
/// Controller for managing Account resources.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountsController> _logger;
    private readonly IAccountRepository _accountRepository;

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
    /// Retrieves all accounts with optional filtering, sorting, and pagination.
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
            a.UpdatedAt
        )).ToList();

        var result = new PagedResult<AccountDto>(accountDtos, totalCount, parameters.Limit, parameters.Offset);
        
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific account by ID.
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
    /// Creates a new account.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccountCommand(
            dto.Code,
            dto.Name,
            dto.TypeId,
            dto.Status,
            dto.NormalBalance);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtRoute("GetAccountsById", new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing account.
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
    /// Deletes an account.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteAccountCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
