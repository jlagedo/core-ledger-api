using MediatR;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
/// Handler for updating an existing Account.
/// </summary>
public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountTypeRepository _accountTypeRepository;
    private readonly ILogger<UpdateAccountCommandHandler> _logger;

    public UpdateAccountCommandHandler(
        IAccountRepository accountRepository,
        IAccountTypeRepository accountTypeRepository,
        ILogger<UpdateAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _accountTypeRepository = accountTypeRepository;
        _logger = logger;
    }

    public async Task Handle(
        UpdateAccountCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Account with ID: {AccountId}", request.Id);

        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (account == null)
        {
            throw new EntityNotFoundException("Account", request.Id);
        }

        // Validate that the account type exists
        var accountType = await _accountTypeRepository.GetByIdAsync(request.TypeId, cancellationToken);
        if (accountType == null)
        {
            throw new EntityNotFoundException("AccountType", request.TypeId);
        }

        // Check if another account with the same code already exists
        var existing = await _accountRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing != null && existing.Id != request.Id)
        {
            throw new DomainValidationException("Account with this code already exists");
        }

        account.Update(
            request.Code,
            request.Name,
            request.TypeId,
            request.Status,
            request.NormalBalance);

        await _accountRepository.UpdateAsync(account, cancellationToken);

        _logger.LogInformation("Updated Account with ID: {AccountId}", request.Id);
    }
}
