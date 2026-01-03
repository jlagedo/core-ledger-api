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

public class CreateTransactionCommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ILogger<CreateTransactionCommandHandler> logger)
    : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating transaction for fund {FundId} - SubType: {SubTypeId}, Amount: {Amount}, " +
            "Quantity: {Quantity}, Price: {Price}, Currency: {Currency}, TradeDate: {TradeDate}, " +
            "SettleDate: {SettleDate}, CreatedBy: {UserId}",
            request.FundId, request.TransactionSubTypeId, request.Amount,
            request.Quantity, request.Price, request.Currency, request.TradeDate,
            request.SettleDate, request.CreatedByUserId);

        // Validate foreign keys
        var fund = await context.Funds.FindAsync([request.FundId], cancellationToken);
        if (fund == null)
        {
            logger.LogWarning("Transaction creation failed: Fund {FundId} not found", request.FundId);
            throw new EntityNotFoundException("Fund", request.FundId);
        }

        if (request.SecurityId.HasValue)
        {
            var security = await context.Securities.FindAsync([request.SecurityId.Value], cancellationToken);
            if (security == null)
            {
                logger.LogWarning("Transaction creation failed: Security {SecurityId} not found", request.SecurityId.Value);
                throw new EntityNotFoundException("Security", request.SecurityId.Value);
            }
        }

        var subType = await context.TransactionSubTypes.FindAsync([request.TransactionSubTypeId], cancellationToken);
        if (subType == null)
        {
            logger.LogWarning("Transaction creation failed: TransactionSubType {SubTypeId} not found", request.TransactionSubTypeId);
            throw new EntityNotFoundException("TransactionSubType", request.TransactionSubTypeId);
        }

        var status = await context.TransactionStatuses.FindAsync([request.StatusId], cancellationToken);
        if (status == null)
        {
            logger.LogWarning("Transaction creation failed: TransactionStatus {StatusId} not found", request.StatusId);
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

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var transactionWithNav = await context.Transactions
            .Include(t => t.Fund)
            .Include(t => t.Security)
            .Include(t => t.TransactionSubType!)
                .ThenInclude(st => st.Type)
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id, cancellationToken);

        logger.LogInformation(
            "Created transaction {TransactionId} for fund {FundId} - Amount: {Amount}, " +
            "Status: {StatusId}, SettleDate: {SettleDate}",
            transaction.Id, transaction.FundId, transaction.Amount, transaction.StatusId, transaction.SettleDate);

        // Map to DTO
        var transactionDto = mapper.Map<TransactionDto>(transactionWithNav);

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

        context.AuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);

        return transactionDto;
    }
}