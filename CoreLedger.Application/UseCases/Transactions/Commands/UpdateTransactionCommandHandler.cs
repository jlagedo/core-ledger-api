using Microsoft.Extensions.Logging;

using MediatR;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IFundRepository _fundRepository;
    private readonly ISecurityRepository _securityRepository;
    private readonly ITransactionSubTypeRepository _transactionSubTypeRepository;
    private readonly ITransactionStatusRepository _transactionStatusRepository;
    private readonly ILogger<UpdateTransactionCommandHandler> _logger;

    public UpdateTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IFundRepository fundRepository,
        ISecurityRepository securityRepository,
        ITransactionSubTypeRepository transactionSubTypeRepository,
        ITransactionStatusRepository transactionStatusRepository,
        ILogger<UpdateTransactionCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _fundRepository = fundRepository;
        _securityRepository = securityRepository;
        _transactionSubTypeRepository = transactionSubTypeRepository;
        _transactionStatusRepository = transactionStatusRepository;
        _logger = logger;
    }

    public async Task Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating transaction with ID: {TransactionId}", request.Id);

        var transaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction == null)
            throw new EntityNotFoundException("Transaction", request.Id);

        // Validate foreign keys
        var fund = await _fundRepository.GetByIdAsync(request.FundId, cancellationToken);
        if (fund == null)
            throw new EntityNotFoundException("Fund", request.FundId);

        if (request.SecurityId.HasValue)
        {
            var security = await _securityRepository.GetByIdAsync(request.SecurityId.Value, cancellationToken);
            if (security == null)
                throw new EntityNotFoundException("Security", request.SecurityId.Value);
        }

        var subType = await _transactionSubTypeRepository.GetByIdAsync(request.TransactionSubTypeId, cancellationToken);
        if (subType == null)
            throw new EntityNotFoundException("TransactionSubType", request.TransactionSubTypeId);

        var status = await _transactionStatusRepository.GetByIdAsync(request.StatusId, cancellationToken);
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

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);

        _logger.LogInformation("Updated transaction with ID: {TransactionId}", request.Id);
    }
}
