using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using CoreLedger.Domain.Enums;
using CoreLedger.Domain.Models;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    public async Task<(IReadOnlyList<IndexadorListProjection> Indexadores, int TotalCount)> GetWithQueryAsync(
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
                    "fonte" => $"WHERE i.fonte ILIKE {{{sqlParameters.Count}}}",
                    "importacaoautomatica" => $"WHERE i.importacao_automatica = {{{sqlParameters.Count}}}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (field == "codigo" || field == "nome" || field == "fonte")
                        sqlParameters.Add($"%{value}%");
                    else if (field == "tipo" && Enum.TryParse(typeof(IndexadorTipo), value, true, out var tipoEnum))
                        sqlParameters.Add((int)tipoEnum!);
                    else if (field == "ativo" && bool.TryParse(value, out var ativo))
                        sqlParameters.Add(ativo);
                    else if (field == "periodicidade" && Enum.TryParse(typeof(Periodicidade), value, true, out var periodicidadeEnum))
                        sqlParameters.Add((int)periodicidadeEnum!);
                    else if (field == "importacaoautomatica" && bool.TryParse(value, out var importacaoAutomatica))
                        sqlParameters.Add(importacaoAutomatica);
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
            SELECT i.*,
                   h.valor AS ultimo_valor,
                   h.data_referencia AS ultima_data,
                   COALESCE(hc.historico_count, 0) AS historico_count
            FROM indexadores i
            LEFT JOIN LATERAL (
                SELECT hi.valor, hi.data_referencia
                FROM historicos_indexadores hi
                WHERE hi.indexador_id = i.id
                ORDER BY hi.data_referencia DESC
                LIMIT 1
            ) h ON true
            LEFT JOIN LATERAL (
                SELECT COUNT(*)::int AS historico_count
                FROM historicos_indexadores hi2
                WHERE hi2.indexador_id = i.id
            ) hc ON true
            {whereClause}
            {orderByClause}
            LIMIT {{{limitParam}}} OFFSET {{{offsetParam}}}";

        var totalCount = await _context.Database
            .SqlQueryRaw<int>(countSql, sqlParameters.Take(limitParam).ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        // Use raw ADO.NET to map to projection with additional columns
        var indexadores = new List<IndexadorListProjection>();
        var connection = _context.Database.GetDbConnection();

        try
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = dataSql;

            // Add parameters
            for (int i = 0; i < sqlParameters.Count; i++)
            {
                var param = command.CreateParameter();
                param.ParameterName = $"p{i}";
                param.Value = sqlParameters[i];
                command.Parameters.Add(param);
            }

            // Replace parameter placeholders with PostgreSQL-style parameters
            for (int i = sqlParameters.Count - 1; i >= 0; i--)
            {
                command.CommandText = command.CommandText.Replace($"{{{i}}}", $"@p{i}");
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                indexadores.Add(new IndexadorListProjection
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Codigo = reader.GetString(reader.GetOrdinal("codigo")),
                    Nome = reader.GetString(reader.GetOrdinal("nome")),
                    Tipo = (IndexadorTipo)reader.GetInt32(reader.GetOrdinal("tipo")),
                    Fonte = reader.IsDBNull(reader.GetOrdinal("fonte")) ? null : reader.GetString(reader.GetOrdinal("fonte")),
                    Periodicidade = (Periodicidade)reader.GetInt32(reader.GetOrdinal("periodicidade")),
                    FatorAcumulado = reader.IsDBNull(reader.GetOrdinal("fator_acumulado")) ? null : reader.GetDecimal(reader.GetOrdinal("fator_acumulado")),
                    DataBase = reader.IsDBNull(reader.GetOrdinal("data_base")) ? null : reader.GetDateTime(reader.GetOrdinal("data_base")),
                    UrlFonte = reader.IsDBNull(reader.GetOrdinal("url_fonte")) ? null : reader.GetString(reader.GetOrdinal("url_fonte")),
                    ImportacaoAutomatica = reader.GetBoolean(reader.GetOrdinal("importacao_automatica")),
                    Ativo = reader.GetBoolean(reader.GetOrdinal("ativo")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime(reader.GetOrdinal("updated_at")),
                    UltimoValor = reader.IsDBNull(reader.GetOrdinal("ultimo_valor")) ? null : reader.GetDecimal(reader.GetOrdinal("ultimo_valor")),
                    UltimaData = reader.IsDBNull(reader.GetOrdinal("ultima_data")) ? null : reader.GetDateTime(reader.GetOrdinal("ultima_data")),
                    HistoricoCount = reader.GetInt32(reader.GetOrdinal("historico_count"))
                });
            }
        }
        finally
        {
            // Don't close the connection as it's managed by EF Core
        }

        return (indexadores, totalCount);
    }
}
