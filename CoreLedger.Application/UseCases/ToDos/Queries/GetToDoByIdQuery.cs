using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.ToDos.Queries;

/// <summary>
///     Query to retrieve a specific ToDo by ID.
/// </summary>
public record GetToDoByIdQuery(int Id) : IRequest<ToDoDto>;