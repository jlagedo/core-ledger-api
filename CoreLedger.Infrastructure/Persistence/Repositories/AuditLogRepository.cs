using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository implementation for AuditLog entity.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<AuditLog> _dbSet;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<AuditLog>();
    }

    public async Task<AuditLog?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<AuditLog> AuditLogs, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var whereClause = string.Empty;
        var sqlParameters = new List<object>();

        if (!string.IsNullOrWhiteSpace(parameters.Filter))
        {
            var filterParts = parameters.Filter.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (filterParts.Length == 2)
            {
                var field = filterParts[0].Trim();
                var value = filterParts[1].Trim().Trim('\'', '"');

                whereClause = field switch
                {
                    "entityName" => $"WHERE a.entity_name ILIKE {{{sqlParameters.Count}}}",
                    "entityId" => $"WHERE a.entity_id = {{{sqlParameters.Count}}}",
                    "eventType" => $"WHERE a.event_type ILIKE {{{sqlParameters.Count}}}",
                    "performedByUserId" => $"WHERE a.performed_by_user_id = {{{sqlParameters.Count}}}",
                    "source" => $"WHERE a.source ILIKE {{{sqlParameters.Count}}}",
                    "correlationId" => $"WHERE a.correlation_id = {{{sqlParameters.Count}}}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (field is "entityName" or "eventType" or "source")
                        sqlParameters.Add($"%{value}%");
                    else
                        sqlParameters.Add(value);
                }
            }
        }

        var orderByClause = "ORDER BY a.performed_at DESC";
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var direction = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? "DESC"
                : "ASC";
            orderByClause = parameters.SortBy.ToLower() switch
            {
                "id" => $"ORDER BY a.id {direction}",
                "entityname" => $"ORDER BY a.entity_name {direction}",
                "entityid" => $"ORDER BY a.entity_id {direction}",
                "eventtype" => $"ORDER BY a.event_type {direction}",
                "performedat" => $"ORDER BY a.performed_at {direction}",
                "performedbyuserid" => $"ORDER BY a.performed_by_user_id {direction}",
                "source" => $"ORDER BY a.source {direction}",
                _ => $"ORDER BY a.performed_at {direction}"
            };
        }

        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM audit_log a
            {whereClause}";

        var dataSql = $@"
            SELECT a.*
            FROM audit_log a
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        var auditLogs = await _dbSet
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (auditLogs, totalCount);
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return auditLog;
    }
}
