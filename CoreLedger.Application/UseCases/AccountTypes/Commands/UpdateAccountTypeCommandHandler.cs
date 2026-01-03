using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.AccountTypes.Commands;

/// <summary>
///     Handler for updating an existing AccountType.
/// </summary>
public class UpdateAccountTypeCommandHandler : IRequestHandler<UpdateAccountTypeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateAccountTypeCommandHandler> _logger;

    public UpdateAccountTypeCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateAccountTypeCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(
        UpdateAccountTypeCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating AccountType with ID: {AccountTypeId}", request.Id);

        var accountType = await _context.AccountTypes
            .FirstOrDefaultAsync(at => at.Id == request.Id, cancellationToken);

        if (accountType == null)
            throw new EntityNotFoundException("AccountType", request.Id);

        // Check if another account type with the same description already exists
        var existing = await _context.AccountTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(at => at.Description.ToLower() == request.Description.ToLower(), cancellationToken);

        if (existing != null && existing.Id != request.Id)
            throw new DomainValidationException("Account type with this description already exists");

        accountType.UpdateDescription(request.Description);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated AccountType with ID: {AccountTypeId}", request.Id);
    }
}
