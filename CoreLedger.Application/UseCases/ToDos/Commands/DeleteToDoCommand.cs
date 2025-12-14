using MediatR;

namespace CoreLedger.Application.UseCases.ToDos.Commands;

/// <summary>
/// Command to delete a ToDo.
/// </summary>
public record DeleteToDoCommand(int Id) : IRequest;
