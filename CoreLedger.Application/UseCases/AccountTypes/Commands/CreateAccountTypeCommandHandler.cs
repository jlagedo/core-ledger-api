using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
/// Handler for creating a new AccountType.
/// </summary>
public class CreateAccountTypeCommandHandler : IRequestHandler<CreateAccountTypeCommand, AccountTypeDto>
{
    private readonly IAccountTypeRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAccountTypeCommandHandler> _logger;

    public CreateAccountTypeCommandHandler(
        IAccountTypeRepository repository,
        IMapper mapper,
        ILogger<CreateAccountTypeCommandHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountTypeDto> Handle(
        CreateAccountTypeCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new AccountType with description: {Description}", 
            request.Description);

        // Check if account type with same description already exists
        var existing = await _repository.GetByDescriptionAsync(request.Description, cancellationToken);
        if (existing != null)
        {
            throw new Domain.Exceptions.DomainValidationException("Account type with this description already exists");
        }

        var accountType = AccountType.Create(request.Description);
        var created = await _repository.AddAsync(accountType, cancellationToken);

        _logger.LogInformation("Created AccountType with ID: {AccountTypeId}", created.Id);

        return _mapper.Map<AccountTypeDto>(created);
    }
}
