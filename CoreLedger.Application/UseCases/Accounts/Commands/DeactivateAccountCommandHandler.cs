using MediatR;
using Microsoft.Extensions.Logging;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
/// Handler for deactivating an existing Account.
/// </summary>
public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<DeactivateAccountCommandHandler> _logger;

    public DeactivateAccountCommandHandler(
        IAccountRepository accountRepository,
        ILogger<DeactivateAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task Handle(
        DeactivateAccountCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating Account with ID: {AccountId}", request.Id);

        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (account == null)
        {
            throw new EntityNotFoundException("Account", request.Id);
        }

        account.Deactivate();

        await _accountRepository.UpdateAsync(account, cancellationToken);

        _logger.LogInformation("Deactivated Account with ID: {AccountId} at {DeactivatedAt}", 
            request.Id, account.DeactivatedAt);
    }
}
