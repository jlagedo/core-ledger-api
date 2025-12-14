using MediatR;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Application.UseCases.ToDos.Commands;

/// <summary>
/// Handler for updating an existing ToDo.
/// </summary>
public class UpdateToDoCommandHandler : IRequestHandler<UpdateToDoCommand>
{
    private readonly IToDoRepository _repository;
    private readonly ILogger<UpdateToDoCommandHandler> _logger;

    public UpdateToDoCommandHandler(
        IToDoRepository repository,
        ILogger<UpdateToDoCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(UpdateToDoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating ToDo {TodoId}", request.Id);

        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (todo == null)
        {
            _logger.LogWarning("ToDo {TodoId} not found for update", request.Id);
            throw new EntityNotFoundException("ToDo", request.Id);
        }

        todo.UpdateDescription(request.Description);

        if (request.IsCompleted && !todo.IsCompleted)
        {
            todo.MarkAsCompleted();
        }
        else if (!request.IsCompleted && todo.IsCompleted)
        {
            todo.MarkAsIncomplete();
        }

        await _repository.UpdateAsync(todo, cancellationToken);

        _logger.LogInformation("Updated ToDo {TodoId}", request.Id);
    }
}
