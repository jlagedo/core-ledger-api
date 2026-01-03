using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
///     Handler for deleting an AccountType.
/// </summary>
public class DeleteAccountTypeCommandHandler : IRequestHandler<DeleteAccountTypeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteAccountTypeCommandHandler> _logger;

    public DeleteAccountTypeCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteAccountTypeCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(
        DeleteAccountTypeCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting AccountType with ID: {AccountTypeId}", request.Id);

        var accountType = await _context.AccountTypes
            .FirstOrDefaultAsync(at => at.Id == request.Id, cancellationToken);

        if (accountType == null)
            throw new EntityNotFoundException("AccountType", request.Id);

        _context.AccountTypes.Remove(accountType);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted AccountType with ID: {AccountTypeId}", request.Id);
    }
}
