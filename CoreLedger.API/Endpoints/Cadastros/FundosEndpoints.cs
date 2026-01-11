using CoreLedger.API.Extensions;
using CoreLedger.API.Models;
using CoreLedger.Application.DTOs.Fundo;
using CoreLedger.Application.UseCases.Cadastros.Fundos.Commands;
using CoreLedger.Application.UseCases.Cadastros.Fundos.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints.Cadastros;

/// <summary>
///     Minimal API endpoints for managing Fundo resources.
/// </summary>
public static class FundosEndpoints
{
    private static readonly string LoggerName = typeof(FundosEndpoints).Name;

    public static IEndpointRouteBuilder MapFundosEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/fundos")
            .WithTags("Fundos")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllFundos");

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetFundoById");

        group.MapGet("/cnpj/{cnpj}", GetByCnpj)
            .WithName("GetFundoByCnpj");

        group.MapGet("/busca", Search)
            .WithName("SearchFundos");

        group.MapPost("/", Create)
            .WithName("CreateFundo");

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateFundo");

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteFundo");

        return routes;
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
            "Retrieving fundos - Limit: {Limit}, Offset: {Offset}, User: {UserId}",
            pagination.Limit, pagination.Offset, userId);

        var query = new GetFundosQuery(
            pagination.Limit,
            pagination.Offset,
            pagination.SortBy,
            pagination.SortDirection,
            pagination.Filter);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Fundos retrieved - Returned: {Count} of {Total}",
            result.Items.Count, result.TotalCount);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Retrieving fundo {FundoId} for user {UserId}", id, userId);

        var query = new GetFundoByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetByCnpj(
        string cnpj,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Retrieving fundo by CNPJ {Cnpj} for user {UserId}", cnpj, userId);

        var query = new GetFundoByCnpjQuery(cnpj);
        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> Search(
        string termo,
        int limit,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Searching fundos with term '{Termo}' for user {UserId}", termo, userId);

        var query = new SearchFundosQuery(termo, limit > 0 ? limit : 20);
        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        FundoCreateDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation(
            "Creating fundo - CNPJ: {Cnpj}, RazaoSocial: {RazaoSocial}, CreatedBy: {UserId}",
            dto.Cnpj, dto.RazaoSocial, userId);

        var command = new CreateFundoCommand(
            dto.Cnpj,
            dto.RazaoSocial,
            dto.TipoFundo,
            dto.ClassificacaoCVM,
            dto.Prazo,
            dto.PublicoAlvo,
            dto.Tributacao,
            dto.Condominio,
            dto.NomeFantasia,
            dto.NomeCurto,
            dto.DataConstituicao,
            dto.DataInicioAtividade,
            dto.ClassificacaoAnbima,
            dto.CodigoAnbima,
            dto.Exclusivo,
            dto.Reservado,
            dto.PermiteAlavancagem,
            dto.AceitaCripto,
            dto.PercentualExterior,
            userId);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Fundo created - Id: {FundoId}", result.Id);

        return Results.CreatedAtRoute("GetFundoById", new { id = result.Id }, result);
    }

    private static async Task<IResult> Update(
        Guid id,
        FundoUpdateDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Updating fundo {FundoId} by user {UserId}", id, userId);

        var command = new UpdateFundoCommand(
            id,
            dto.RazaoSocial,
            dto.NomeFantasia,
            dto.NomeCurto,
            dto.DataConstituicao,
            dto.DataInicioAtividade,
            dto.ClassificacaoCVM,
            dto.ClassificacaoAnbima,
            dto.CodigoAnbima,
            dto.Prazo,
            dto.PublicoAlvo,
            dto.Tributacao,
            dto.Condominio,
            dto.Exclusivo,
            dto.Reservado,
            dto.PermiteAlavancagem,
            dto.AceitaCripto,
            dto.PercentualExterior,
            userId);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Fundo updated - Id: {FundoId}", id);

        return Results.Ok(result);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerName);
        var userId = context.GetUserId();

        logger.LogInformation("Deleting fundo {FundoId} by user {UserId}", id, userId);

        var command = new DeleteFundoCommand(id, userId);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Fundo deleted - Id: {FundoId}", id);

        return Results.NoContent();
    }
}
