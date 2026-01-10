using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for complex Indexador queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public class IndexadorQueryService : IIndexadorQueryService
{
    private readonly ApplicationDbContext _context;

    public IndexadorQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Indexador> Indexadores, int TotalCount)> GetWithQueryAsync(
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
                    "codigo" => $"WHERE i.codigo ILIKE {{{sqlParameters.Count}}}",
                    "nome" => $"WHERE i.nome ILIKE {{{sqlParameters.Count}}}",
                    "tipo" => $"WHERE i.tipo = {{{sqlParameters.Count}}}",
                    "ativo" => $"WHERE i.ativo = {{{sqlParameters.Count}}}",
                    "periodicidade" => $"WHERE i.periodicidade = {{{sqlParameters.Count}}}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (field == "codigo" || field == "nome")
                        sqlParameters.Add($"%{value}%");
                    else if (field == "tipo" && Enum.TryParse(typeof(IndexadorTipo), value, true, out var tipoEnum))
                        sqlParameters.Add((int)tipoEnum!);
                    else if (field == "ativo" && bool.TryParse(value, out var ativo))
                        sqlParameters.Add(ativo);
                    else if (field == "periodicidade" && Enum.TryParse(typeof(Periodicidade), value, true, out var periodicidadeEnum))
                        sqlParameters.Add((int)periodicidadeEnum!);
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
                "codigo" => $"ORDER BY i.codigo {direction}",
                "nome" => $"ORDER BY i.nome {direction}",
                "tipo" => $"ORDER BY i.tipo {direction}",
                "periodicidade" => $"ORDER BY i.periodicidade {direction}",
                "ativo" => $"ORDER BY i.ativo {direction}",
                "createdat" => $"ORDER BY i.created_at {direction}",
                "updatedat" => $"ORDER BY i.updated_at {direction}",
                _ => $"ORDER BY i.id {direction}"
            };
        }
        else
        {
            orderByClause = "ORDER BY i.id ASC";
        }

        var limitParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Limit);
        var offsetParam = sqlParameters.Count;
        sqlParameters.Add(parameters.Offset);

        var countSql = $@"
            SELECT COUNT(*)::int AS ""Value""
            FROM indexadores i
            {whereClause}";

        var dataSql = $@"
            SELECT i.*
            FROM indexadores i
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        var indexadores = await _context.Set<Indexador>()
            .FromSqlRaw(dataSql, sqlParameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (indexadores, totalCount);
    }
}
