using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.Interfaces;

/// <summary>
///     Interface for the application database context exposing DbSets for direct use in handlers.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<AccountType> AccountTypes { get; }
    DbSet<Account> Accounts { get; }
    DbSet<CoreJob> CoreJobs { get; }
    DbSet<Fund> Funds { get; }
    DbSet<Security> Securities { get; }
    DbSet<TransactionStatus> TransactionStatuses { get; }
    DbSet<TransactionType> TransactionTypes { get; }
    DbSet<TransactionSubType> TransactionSubTypes { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<User> Users { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
