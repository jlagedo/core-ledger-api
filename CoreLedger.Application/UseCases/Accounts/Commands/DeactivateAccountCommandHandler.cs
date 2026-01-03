using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Accounts.Commands;

/// <summary>
///     Handler for deactivating an existing Account.
/// </summary>
public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateAccountCommandHandler> _logger;

    public DeactivateAccountCommandHandler(
        IApplicationDbContext context,
        ILogger<DeactivateAccountCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(
        DeactivateAccountCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating Account with ID: {AccountId}", request.Id);

        var account = await _context.Accounts.FindAsync([request.Id], cancellationToken);
        if (account == null) throw new EntityNotFoundException("Account", request.Id);

        account.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated Account with ID: {AccountId} at {DeactivatedAt}",
            request.Id, account.DeactivatedAt);
    }
}