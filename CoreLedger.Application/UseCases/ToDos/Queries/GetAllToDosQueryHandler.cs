using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.ToDos.Queries;

/// <summary>
/// Handler for retrieving all ToDo items.
/// </summary>
public class GetAllToDosQueryHandler : IRequestHandler<GetAllToDosQuery, IReadOnlyList<ToDoDto>>
{
    private readonly IToDoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllToDosQueryHandler> _logger;

    public GetAllToDosQueryHandler(
        IToDoRepository repository,
        IMapper mapper,
        ILogger<GetAllToDosQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ToDoDto>> Handle(
        GetAllToDosQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all ToDo items");

        var todos = await _repository.GetAllAsync(cancellationToken);
        var todoDtos = _mapper.Map<IReadOnlyList<ToDoDto>>(todos);

        _logger.LogInformation("Retrieved {Count} ToDo items", todoDtos.Count);

        return todoDtos;
    }
}
