using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository implementation for Fund entity with specific queries.
/// </summary>
public class FundRepository : Repository<Fund>, IFundRepository
{
    public FundRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Fund?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Name == name, cancellationToken);
    }

    public async Task<(IReadOnlyList<Fund> Funds, int TotalCount)> GetWithQueryAsync(
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
                    "name" => $"WHERE f.name ILIKE {{{sqlParameters.Count}}}",
                    "baseCurrency" => $"WHERE f.base_currency = {{{sqlParameters.Count}}}",
                    "valuationFrequency" => $"WHERE f.valuation_frequency = {{{sqlParameters.Count}}}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (field == "name")
                        sqlParameters.Add($"%{value}%");
                    else if (field == "baseCurrency")
                        sqlParameters.Add(value.ToUpperInvariant());
                    else if (field == "valuationFrequency" &&
                             Enum.TryParse(typeof(ValuationFrequency), value, true, out var frequencyEnum))
                        sqlParameters.Add((int)frequencyEnum!);
                    else
                        whereClause = string.Empty;
                }
            }
        }

        var orderByClause = string.Empty;
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var direction = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? "DESC"
                : "ASC";
            orderByClause = parameters.SortBy.ToLower() switch
            {
                "name" => $"ORDER BY f.name {direction}",
                "basecurrency" => $"ORDER BY f.base_currency {direction}",
                "inceptiondate" => $"ORDER BY f.inception_date {direction}",
                "valuationfrequency" => $"ORDER BY f.valuation_frequency {direction}",
                "createdat" => $"ORDER BY f.created_at {direction}",
                "updatedat" => $"ORDER BY f.updated_at {direction}",
                _ => $"ORDER BY f.id {direction}"
            };
        }
        else
        {
            orderByClause = "ORDER BY f.id ASC";
        }

        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM funds f
            {whereClause}";

        var dataSql = $@"
            SELECT f.*
            FROM funds f
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        var funds = await _context.Set<Fund>()
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (funds, totalCount);
    }
}