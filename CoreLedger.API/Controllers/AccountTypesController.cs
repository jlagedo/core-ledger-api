using MediatR;
using Microsoft.AspNetCore.Mvc;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.AccountTypes.Commands;
using CoreLedger.Application.UseCases.AccountTypes.Queries;

namespace CoreLedger.API.Controllers;

/// <summary>
/// Controller for managing Account Type resources.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountTypesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountTypesController> _logger;

    public AccountTypesController(IMediator mediator, ILogger<AccountTypesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all account types.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllAccountTypesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific account type by ID.
    /// </summary>
    [HttpGet("{id}", Name = "GetAccountTypeById")]
    [ProducesResponseType(typeof(AccountTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetAccountTypeByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new account type.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountTypeDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccountTypeCommand(dto.Description);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtRoute("GetAccountTypeById", new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing account type.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateAccountTypeDto dto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAccountTypeCommand(id, dto.Description);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an account type.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteAccountTypeCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
