using CoreLedger.API.Extensions;
using CoreLedger.API.Models;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.Indexadores.Commands;
using CoreLedger.Application.UseCases.Indexadores.Queries;
using CoreLedger.Application.UseCases.HistoricosIndexadores.Queries;
using CoreLedger.Domain.Models;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing Indexador resources.
/// </summary>
public static class IndexadoresEndpoints
{
    private static readonly string LoggerName = typeof(IndexadoresEndpoints).Name;

    public static IEndpointRouteBuilder MapIndexadoresEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/indexadores")
            .WithTags("Indexadores")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllIndexadores");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetIndexadorById");

        group.MapGet("/{id:int}/historico", GetHistorico)
            .WithName("GetIndexadorHistorico");

        group.MapPost("/", Create)
            .WithName("CreateIndexador");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateIndexador");

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteIndexador");

        group.MapPost("/{id:int}/importar", Importar)
            .WithName("ImportarIndexador");

        return group;
    }

    private static async Task<IResult> GetAll(
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Retrieving indexadores - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}, User: {UserId}",
            pagination.Limit, pagination.Offset, pagination.SortBy ?? "none", pagination.Filter ?? "none", userId);

        var query = new GetIndexadoresWithQueryQuery(
            pagination.Limit,
            pagination.Offset,
            pagination.SortBy,
            pagination.SortDirection,
            pagination.Filter);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Indexadores retrieved - Returned: {Count} of {Total} total indexadores",
            result.Items.Count, result.TotalCount);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        int id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Retrieving indexador {IndexadorId} for user {UserId}", id, userId);

        var query = new GetIndexadorByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Indexador retrieved - Codigo: {Codigo}, Nome: {Nome}, Tipo: {Tipo}",
            result.Codigo, result.Nome, result.TipoDescricao);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetHistorico(
        int id,
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Retrieving historical data for indexador {IndexadorId} - Limit: {Limit}, Offset: {Offset}, User: {UserId}",
            id, pagination.Limit, pagination.Offset, userId);

        var parameters = new QueryParameters
        {
            Limit = pagination.Limit,
            Offset = pagination.Offset,
            SortBy = pagination.SortBy,
            SortDirection = pagination.SortDirection,
            Filter = pagination.Filter
        };

        var query = new GetHistoricoIndexadorQuery(id, parameters);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Historical data retrieved - Returned: {Count} of {Total} total records for indexador {IndexadorId}",
            result.Items.Count, result.TotalCount, id);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateIndexadorDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogError("Authentication failed: 'sub' claim missing from token for endpoint {Endpoint}",
                context.Request.Path);
            return Results.Unauthorized();
        }

        logger.LogInformation(
            "Creating indexador - Codigo: {Codigo}, Nome: {Nome}, Tipo: {Tipo}, CreatedBy: {UserId}",
            dto.Codigo, dto.Nome, dto.Tipo, userId);

        var command = new CreateIndexadorCommand(
            dto.Codigo,
            dto.Nome,
            dto.Tipo,
            dto.Fonte,
            dto.Periodicidade,
            dto.FatorAcumulado,
            dto.DataBase,
            dto.UrlFonte,
            dto.ImportacaoAutomatica,
            dto.Ativo);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Indexador created successfully - Id: {IndexadorId}, Codigo: {Codigo}",
            result.Id, result.Codigo);

        return Results.CreatedAtRoute("GetIndexadorById", new { id = result.Id }, result);
    }

    private static async Task<IResult> Update(
        int id,
        UpdateIndexadorDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Updating indexador {IndexadorId} - Nome: {Nome}, Tipo: {Tipo}, UpdatedBy: {UserId}",
            id, dto.Nome, dto.Tipo, userId);

        var command = new UpdateIndexadorCommand(
            id,
            dto.Nome,
            dto.Tipo,
            dto.Fonte,
            dto.Periodicidade,
            dto.FatorAcumulado,
            dto.DataBase,
            dto.UrlFonte,
            dto.ImportacaoAutomatica,
            dto.Ativo);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Indexador updated successfully - Id: {IndexadorId}", id);

        return Results.Ok(result);
    }

    private static async Task<IResult> Delete(
        int id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Deleting indexador {IndexadorId}, User: {UserId}", id, userId);

        var command = new DeleteIndexadorCommand(id);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Indexador deleted successfully - Id: {IndexadorId}", id);

        return Results.NoContent();
    }

    private static async Task<IResult> Importar(
        int id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();
        var correlationId = context.GetCorrelationId() ?? Guid.NewGuid().ToString();

        logger.LogInformation(
            "Triggering import for indexador {IndexadorId}, CorrelationId: {CorrelationId}, User: {UserId}",
            id, correlationId, userId);

        var command = new ImportarIndexadorCommand(id, correlationId);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation(
            "Import triggered successfully for indexador {IndexadorId}, CorrelationId: {CorrelationId}",
            id, correlationId);

        return Results.Accepted($"/api/indexadores/{id}/historico", new { id, correlationId });
    }
}
