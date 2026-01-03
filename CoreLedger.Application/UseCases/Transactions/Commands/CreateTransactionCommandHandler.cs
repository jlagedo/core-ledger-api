using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoreLedger.Application.UseCases.Transactions.Commands;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateTransactionCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateTransactionCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateTransactionCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating transaction for fund {FundId} - SubType: {SubTypeId}, Amount: {Amount}, " +
            "Quantity: {Quantity}, Price: {Price}, Currency: {Currency}, TradeDate: {TradeDate}, " +
            "SettleDate: {SettleDate}, CreatedBy: {UserId}",
            request.FundId, request.TransactionSubTypeId, request.Amount,
            request.Quantity, request.Price, request.Currency, request.TradeDate,
            request.SettleDate, request.CreatedByUserId);

        // Validate foreign keys
        var fund = await _context.Funds.FindAsync([request.FundId], cancellationToken);
        if (fund == null)
        {
            _logger.LogWarning("Transaction creation failed: Fund {FundId} not found", request.FundId);
            throw new EntityNotFoundException("Fund", request.FundId);
        }

        if (request.SecurityId.HasValue)
        {
            var security = await _context.Securities.FindAsync([request.SecurityId.Value], cancellationToken);
            if (security == null)
            {
                _logger.LogWarning("Transaction creation failed: Security {SecurityId} not found", request.SecurityId.Value);
                throw new EntityNotFoundException("Security", request.SecurityId.Value);
            }
        }

        var subType = await _context.TransactionSubTypes.FindAsync([request.TransactionSubTypeId], cancellationToken);
        if (subType == null)
        {
            _logger.LogWarning("Transaction creation failed: TransactionSubType {SubTypeId} not found", request.TransactionSubTypeId);
            throw new EntityNotFoundException("TransactionSubType", request.TransactionSubTypeId);
        }

        var status = await _context.TransactionStatuses.FindAsync([request.StatusId], cancellationToken);
        if (status == null)
        {
            _logger.LogWarning("Transaction creation failed: TransactionStatus {StatusId} not found", request.StatusId);
            throw new EntityNotFoundException("TransactionStatus", request.StatusId);
        }

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

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var transactionWithNav = await _context.Transactions
            .Include(t => t.Fund)
            .Include(t => t.Security)
            .Include(t => t.TransactionSubType!)
                .ThenInclude(st => st.Type)
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id, cancellationToken);

        _logger.LogInformation(
            "Created transaction {TransactionId} for fund {FundId} - Amount: {Amount}, " +
            "Status: {StatusId}, SettleDate: {SettleDate}",
            transaction.Id, transaction.FundId, transaction.Amount, transaction.StatusId, transaction.SettleDate);

        // Map to DTO
        var transactionDto = _mapper.Map<TransactionDto>(transactionWithNav);

        // Create audit log entry
        var transactionDataJson = JsonSerializer.Serialize(transactionDto);
        var dataAfter = JsonDocument.Parse(transactionDataJson);
        var auditLog = AuditLog.Create(
            entityName: "Transaction",
            entityId: transaction.Id.ToString(),
            eventType: "Created",
            performedByUserId: request.CreatedByUserId,
            dataAfter: dataAfter,
            correlationId: request.CorrelationId,
            requestId: request.RequestId,
            source: "API");

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        return transactionDto;
    }
}