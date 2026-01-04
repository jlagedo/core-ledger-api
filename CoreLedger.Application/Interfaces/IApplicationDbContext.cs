using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CoreLedger.Application.Interfaces;

/// <summary>
///     Interface for the application database context exposing DbSets for direct use in handlers.
/// </summary>
public interface IApplicationDbContext
{
    DatabaseFacade Database { get; }
    DbSet<AccountType> AccountTypes { get; }
    DbSet<Account> Accounts { get; }
    DbSet<CoreJob> CoreJobs { get; }
    DbSet<Fund> Funds { get; }
    DbSet<Security> Securities { get; }
    DbSet<TransactionStatus> TransactionStatuses { get; }
    DbSet<TransactionType> TransactionTypes { get; }
    DbSet<TransactionSubType> TransactionSubTypes { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<TransactionIdempotency> TransactionIdempotencies { get; }
    DbSet<TransactionCreatedOutboxMessage> TransactionCreatedOutboxMessages { get; }
    DbSet<User> Users { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
