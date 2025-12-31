using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly IFundRepository _fundRepository;
    private readonly ILogger<CreateTransactionCommandHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ISecurityRepository _securityRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionStatusRepository _transactionStatusRepository;
    private readonly ITransactionSubTypeRepository _transactionSubTypeRepository;

    public CreateTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IFundRepository fundRepository,
        ISecurityRepository securityRepository,
        ITransactionSubTypeRepository transactionSubTypeRepository,
        ITransactionStatusRepository transactionStatusRepository,
        IMapper mapper,
        ILogger<CreateTransactionCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _fundRepository = fundRepository;
        _securityRepository = securityRepository;
        _transactionSubTypeRepository = transactionSubTypeRepository;
        _transactionStatusRepository = transactionStatusRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new transaction for fund {FundId}", request.FundId);

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

        // Create transaction
        var transaction = Transaction.Create(
            request.FundId,
            request.SecurityId,
            request.TransactionSubTypeId,
            request.TradeDate,
            request.SettleDate,
            request.Quantity,
            request.Price,
            request.Amount,
            request.Currency,
            request.StatusId,
            request.CreatedByUserId);

        var created = await _transactionRepository.AddAsync(transaction, cancellationToken);

        // Reload with navigation properties
        var transactionWithNav = await _transactionRepository.GetByIdWithNavigationAsync(created.Id, cancellationToken);

        _logger.LogInformation("Created transaction with ID: {TransactionId}", created.Id);

        return _mapper.Map<TransactionDto>(transactionWithNav);
    }
}