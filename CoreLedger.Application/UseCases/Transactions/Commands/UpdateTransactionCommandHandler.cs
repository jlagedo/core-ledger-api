using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateTransactionCommandHandler> _logger;

    public UpdateTransactionCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateTransactionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating transaction with ID: {TransactionId}", request.Id);

        var transaction = await _context.Transactions.FindAsync([request.Id], cancellationToken);
        if (transaction == null)
            throw new EntityNotFoundException("Transaction", request.Id);

        // Validate foreign keys
        var fund = await _context.Funds.FindAsync([request.FundId], cancellationToken);
        if (fund == null)
            throw new EntityNotFoundException("Fund", request.FundId);

        if (request.SecurityId.HasValue)
        {
            var security = await _context.Securities.FindAsync([request.SecurityId.Value], cancellationToken);
            if (security == null)
                throw new EntityNotFoundException("Security", request.SecurityId.Value);
        }

        var subType = await _context.TransactionSubTypes.FindAsync([request.TransactionSubTypeId], cancellationToken);
        if (subType == null)
            throw new EntityNotFoundException("TransactionSubType", request.TransactionSubTypeId);

        var status = await _context.TransactionStatuses.FindAsync([request.StatusId], cancellationToken);
        if (status == null)
            throw new EntityNotFoundException("TransactionStatus", request.StatusId);

        transaction.Update(
            request.FundId,
            request.SecurityId,
            request.TransactionSubTypeId,
            request.TradeDate,
            request.SettleDate,
            request.Quantity,
            request.Price,
            request.Amount,
            request.Currency,
            request.StatusId);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated transaction with ID: {TransactionId}", request.Id);
    }
}