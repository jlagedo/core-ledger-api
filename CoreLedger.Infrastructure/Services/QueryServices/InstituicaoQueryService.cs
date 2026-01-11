using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services.QueryServices;

/// <summary>
///     Query service implementation for complex Instituicao queries with RFC-8040 filtering, sorting, and pagination.
/// </summary>
public class InstituicaoQueryService : IInstituicaoQueryService
{
    private readonly ApplicationDbContext _context;

    public InstituicaoQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Instituicao> Instituicoes, int TotalCount)> GetWithQueryAsync(
        QueryParameters parameters,
        string? search = null,
        bool? ativo = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Instituicoes.AsNoTracking();

        // Apply search filter (CNPJ, razão social, or nome fantasia)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
            query = query.Where(i =>
                i.Cnpj.Valor.Contains(normalizedSearch) ||
                EF.Functions.ILike(i.RazaoSocial, $"%{search}%") ||
                (i.NomeFantasia != null && EF.Functions.ILike(i.NomeFantasia, $"%{search}%")));
        }

        // Apply ativo filter
        if (ativo.HasValue)
        {
            query = query.Where(i => i.Ativo == ativo.Value);
        }

        // Apply RFC-8040 filter
        if (!string.IsNullOrWhiteSpace(parameters.Filter))
        {
            var filterParts = parameters.Filter.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (filterParts.Length == 2)
            {
                var field = filterParts[0].Trim().ToLower();
                var value = filterParts[1].Trim().Trim('\'', '"');

                query = field switch
                {
                    "cnpj" => query.Where(i => i.Cnpj.Valor == value.Replace(".", "").Replace("/", "").Replace("-", "")),
                    "razaosocial" => query.Where(i => EF.Functions.ILike(i.RazaoSocial, $"%{value}%")),
                    "nomefantasia" => query.Where(i => i.NomeFantasia != null && EF.Functions.ILike(i.NomeFantasia, $"%{value}%")),
                    "ativo" => bool.TryParse(value, out var ativoValue)
                        ? query.Where(i => i.Ativo == ativoValue)
                        : query,
                    _ => query
                };
            }
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        var sortField = parameters.SortBy?.ToLower() ?? "razaosocial";
        var isDescending = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        query = sortField switch
        {
            "razaosocial" => isDescending ? query.OrderByDescending(i => i.RazaoSocial) : query.OrderBy(i => i.RazaoSocial),
            "nomefantasia" => isDescending ? query.OrderByDescending(i => i.NomeFantasia) : query.OrderBy(i => i.NomeFantasia),
            "ativo" => isDescending ? query.OrderByDescending(i => i.Ativo) : query.OrderBy(i => i.Ativo),
            "createdat" => isDescending ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt),
            "updatedat" => isDescending ? query.OrderByDescending(i => i.UpdatedAt) : query.OrderBy(i => i.UpdatedAt),
            _ => isDescending ? query.OrderByDescending(i => i.RazaoSocial) : query.OrderBy(i => i.RazaoSocial)
        };

        // Apply pagination
        var instituicoes = await query
            .Skip(parameters.Offset)
            .Take(parameters.Limit)
            .ToListAsync(cancellationToken);

        return (instituicoes, totalCount);
    }

    public async Task<Instituicao?> GetByCnpjAsync(
        string cnpj,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return null;

        var normalizedCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Trim();

        return await _context.Instituicoes
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Cnpj.Valor == normalizedCnpj, cancellationToken);
    }
}
