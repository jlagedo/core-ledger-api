using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.ToDos.Queries;

/// <summary>
/// Query to retrieve a specific ToDo by ID.
/// </summary>
public record GetToDoByIdQuery(int Id) : IRequest<ToDoDto>;
