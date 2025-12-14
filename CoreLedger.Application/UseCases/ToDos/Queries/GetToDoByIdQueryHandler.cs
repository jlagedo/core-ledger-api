using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.ToDos.Queries;

/// <summary>
/// Handler for retrieving a specific ToDo by ID.
/// </summary>
public class GetToDoByIdQueryHandler : IRequestHandler<GetToDoByIdQuery, ToDoDto>
{
    private readonly IToDoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetToDoByIdQueryHandler> _logger;

    public GetToDoByIdQueryHandler(
        IToDoRepository repository,
        IMapper mapper,
        ILogger<GetToDoByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ToDoDto> Handle(
        GetToDoByIdQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving ToDo {TodoId}", request.Id);

        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (todo == null)
        {
            _logger.LogWarning("ToDo {TodoId} not found", request.Id);
            throw new EntityNotFoundException("ToDo", request.Id);
        }

        return _mapper.Map<ToDoDto>(todo);
    }
}
