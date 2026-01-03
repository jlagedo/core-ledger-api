using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for complex Fund queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public class FundQueryService : IFundQueryService
{
    private readonly ApplicationDbContext _context;

    public FundQueryService(ApplicationDbContext context)
    {
        _context = context;
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
