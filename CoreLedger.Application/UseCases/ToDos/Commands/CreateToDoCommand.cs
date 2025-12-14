using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.ToDos.Commands;

/// <summary>
/// Command to create a new ToDo.
/// </summary>
public record CreateToDoCommand(string Description) : IRequest<ToDoDto>;
