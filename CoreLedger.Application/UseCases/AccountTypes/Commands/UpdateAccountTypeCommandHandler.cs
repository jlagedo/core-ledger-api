using MediatR;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
/// Handler for updating an existing AccountType.
/// </summary>
public class UpdateAccountTypeCommandHandler : IRequestHandler<UpdateAccountTypeCommand>
{
    private readonly IAccountTypeRepository _repository;
    private readonly ILogger<UpdateAccountTypeCommandHandler> _logger;

    public UpdateAccountTypeCommandHandler(
        IAccountTypeRepository repository,
        ILogger<UpdateAccountTypeCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(
        UpdateAccountTypeCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating AccountType with ID: {AccountTypeId}", request.Id);

        var accountType = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (accountType == null)
        {
            throw new EntityNotFoundException("AccountType", request.Id);
        }

        // Check if another account type with the same description already exists
        var existing = await _repository.GetByDescriptionAsync(request.Description, cancellationToken);
        if (existing != null && existing.Id != request.Id)
        {
            throw new DomainValidationException("Account type with this description already exists");
        }

        accountType.UpdateDescription(request.Description);
        await _repository.UpdateAsync(accountType, cancellationToken);

        _logger.LogInformation("Updated AccountType with ID: {AccountTypeId}", request.Id);
    }
}
