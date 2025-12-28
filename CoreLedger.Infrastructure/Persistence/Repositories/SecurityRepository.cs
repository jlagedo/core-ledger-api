using Microsoft.EntityFrameworkCore;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Models;

namespace CoreLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Security entity with specific queries.
/// </summary>
public class SecurityRepository : Repository<Security>, ISecurityRepository
{
    public SecurityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Security?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Ticker == ticker.ToUpperInvariant(), cancellationToken);
    }

    public async Task<(IReadOnlyList<Security> Securities, int TotalCount)> GetWithQueryAsync(
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
                    "name" => $"WHERE s.name ILIKE {{{sqlParameters.Count}}}",
                    "ticker" => $"WHERE s.ticker = {{{sqlParameters.Count}}}",
                    "type" => $"WHERE s.type = {{{sqlParameters.Count}}}",
                    "currency" => $"WHERE s.currency = {{{sqlParameters.Count}}}",
                    "status" => $"WHERE s.status = {{{sqlParameters.Count}}}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (field == "name")
                    {
                        sqlParameters.Add($"%{value}%");
                    }
                    else if (field == "ticker")
                    {
                        sqlParameters.Add(value.ToUpperInvariant());
                    }
                    else if (field == "type" && Enum.TryParse(typeof(CoreLedger.Domain.Enums.SecurityType), value, true, out var typeEnum))
                    {
                        sqlParameters.Add((int)typeEnum!);
                    }
                    else if (field == "currency")
                    {
                        sqlParameters.Add(value.ToUpperInvariant());
                    }
                    else if (field == "status" && Enum.TryParse(typeof(CoreLedger.Domain.Enums.SecurityStatus), value, true, out var statusEnum))
                    {
                        sqlParameters.Add((int)statusEnum!);
                    }
                    else
                    {
                        whereClause = string.Empty;
                    }
                }
            }
        }

        var orderByClause = string.Empty;
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var direction = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
            orderByClause = parameters.SortBy.ToLower() switch
            {
                "name" => $"ORDER BY s.name {direction}",
                "ticker" => $"ORDER BY s.ticker {direction}",
                "type" => $"ORDER BY s.type {direction}",
                "currency" => $"ORDER BY s.currency {direction}",
                "status" => $"ORDER BY s.status {direction}",
                "createdat" => $"ORDER BY s.created_at {direction}",
                "updatedat" => $"ORDER BY s.updated_at {direction}",
                _ => $"ORDER BY s.id {direction}"
            };
        }
        else
        {
            orderByClause = "ORDER BY s.id ASC";
        }

        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM securities s
            {whereClause}";

        var dataSql = $@"
            SELECT s.*
            FROM securities s
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        var securities = await _context.Set<Security>()
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (securities, totalCount);
    }
}
