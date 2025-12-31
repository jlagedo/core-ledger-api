using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.ToDos.Commands;

/// <summary>
///     Command to create a new ToDo.
/// </summary>
public record CreateToDoCommand(string Description, string CreatedByUserId) : IRequest<ToDoDto>;