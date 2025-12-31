using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
///     Handler for deleting an Account.
/// </summary>
public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly ILogger<DeleteAccountCommandHandler> _logger;
    private readonly IAccountRepository _repository;

    public DeleteAccountCommandHandler(
        IAccountRepository repository,
        ILogger<DeleteAccountCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(
        DeleteAccountCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting Account with ID: {AccountId}", request.Id);

        var account = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (account == null) throw new EntityNotFoundException("Account", request.Id);

        await _repository.DeleteAsync(account, cancellationToken);

        _logger.LogInformation("Deleted Account with ID: {AccountId}", request.Id);
    }
}