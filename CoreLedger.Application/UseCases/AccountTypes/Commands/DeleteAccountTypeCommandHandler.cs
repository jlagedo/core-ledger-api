using MediatR;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
/// Handler for deleting an AccountType.
/// </summary>
public class DeleteAccountTypeCommandHandler : IRequestHandler<DeleteAccountTypeCommand>
{
    private readonly IAccountTypeRepository _repository;
    private readonly ILogger<DeleteAccountTypeCommandHandler> _logger;

    public DeleteAccountTypeCommandHandler(
        IAccountTypeRepository repository,
        ILogger<DeleteAccountTypeCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(
        DeleteAccountTypeCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting AccountType with ID: {AccountTypeId}", request.Id);

        var accountType = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (accountType == null)
        {
            throw new EntityNotFoundException("AccountType", request.Id);
        }

        await _repository.DeleteAsync(accountType, cancellationToken);

        _logger.LogInformation("Deleted AccountType with ID: {AccountTypeId}", request.Id);
    }
}
