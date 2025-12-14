using MediatR;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Application.UseCases.ToDos.Commands;

/// <summary>
/// Handler for deleting a ToDo.
/// </summary>
public class DeleteToDoCommandHandler : IRequestHandler<DeleteToDoCommand>
{
    private readonly IToDoRepository _repository;
    private readonly ILogger<DeleteToDoCommandHandler> _logger;

    public DeleteToDoCommandHandler(
        IToDoRepository repository,
        ILogger<DeleteToDoCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(DeleteToDoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting ToDo {TodoId}", request.Id);

        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (todo == null)
        {
            _logger.LogWarning("ToDo {TodoId} not found for deletion", request.Id);
            throw new EntityNotFoundException("ToDo", request.Id);
        }

        await _repository.DeleteAsync(todo, cancellationToken);

        _logger.LogInformation("Deleted ToDo {TodoId}", request.Id);
    }
}
