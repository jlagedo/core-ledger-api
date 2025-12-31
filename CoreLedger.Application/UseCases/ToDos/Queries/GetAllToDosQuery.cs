using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.ToDos.Queries;

/// <summary>
///     Query to retrieve all ToDo items.
/// </summary>
public record GetAllToDosQuery : IRequest<IReadOnlyList<ToDoDto>>;