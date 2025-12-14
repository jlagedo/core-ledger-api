using MediatR;
using Microsoft.AspNetCore.Mvc;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.ToDos.Commands;
using CoreLedger.Application.UseCases.ToDos.Queries;

namespace CoreLedger.API.Controllers;

/// <summary>
/// Controller for managing ToDo items.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ToDosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ToDosController> _logger;

    public ToDosController(IMediator mediator, ILogger<ToDosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all ToDo items.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ToDoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllToDosQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific ToDo item by ID.
    /// </summary>
    [HttpGet("{id}", Name = "GetTodoById")]
    [ProducesResponseType(typeof(ToDoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetToDoByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new ToDo item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ToDoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateToDoDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateToDoCommand(dto.Description);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtRoute("GetTodoById", new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing ToDo item.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateToDoDto dto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateToDoCommand(id, dto.Description, dto.IsCompleted);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes a ToDo item.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteToDoCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
