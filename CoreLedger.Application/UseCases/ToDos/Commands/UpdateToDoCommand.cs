using MediatR;

namespace CoreLedger.Application.UseCases.ToDos.Commands;

/// <summary>
/// Command to update an existing ToDo.
/// </summary>
public record UpdateToDoCommand(int Id, string Description, bool IsCompleted) : IRequest;
