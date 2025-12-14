using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.ToDos.Commands;

/// <summary>
/// Handler for creating a new ToDo.
/// </summary>
public class CreateToDoCommandHandler : IRequestHandler<CreateToDoCommand, ToDoDto>
{
    private readonly IToDoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateToDoCommandHandler> _logger;

    public CreateToDoCommandHandler(
        IToDoRepository repository,
        IMapper mapper,
        ILogger<CreateToDoCommandHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ToDoDto> Handle(
        CreateToDoCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new ToDo with description: {Description}", 
            request.Description);

        var todo = ToDo.Create(request.Description);
        var created = await _repository.AddAsync(todo, cancellationToken);

        _logger.LogInformation("Created ToDo with ID: {TodoId}", created.Id);

        return _mapper.Map<ToDoDto>(created);
    }
}
