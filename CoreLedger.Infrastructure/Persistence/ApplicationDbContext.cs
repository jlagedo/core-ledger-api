using CoreLedger.Domain.Entities;
using CoreLedger.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Persistence;

/// <summary>
///     Application database context with proper configuration for PostgreSQL.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<AccountType> AccountTypes => Set<AccountType>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CoreJob> CoreJobs => Set<CoreJob>();
    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<Security> Securities => Set<Security>();
    public DbSet<TransactionStatus> TransactionStatuses => Set<TransactionStatus>();
    public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();
    public DbSet<TransactionSubType> TransactionSubTypes => Set<TransactionSubType>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<TransactionIdempotency> TransactionIdempotencies => Set<TransactionIdempotency>();
    public DbSet<TransactionCreatedOutboxMessage> TransactionCreatedOutboxMessages => Set<TransactionCreatedOutboxMessage>();
    public DbSet<Calendario> Calendarios => Set<Calendario>();
    public DbSet<Indexador> Indexadores => Set<Indexador>();
    public DbSet<HistoricoIndexador> HistoricosIndexadores => Set<HistoricoIndexador>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}