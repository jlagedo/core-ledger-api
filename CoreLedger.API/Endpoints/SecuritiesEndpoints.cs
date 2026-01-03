using CoreLedger.API.Extensions;
using CoreLedger.API.Models;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Application.UseCases.Securities.Commands;
using CoreLedger.Application.UseCases.Securities.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing Security resources.
/// </summary>
public static class SecuritiesEndpoints
{
    public static IEndpointRouteBuilder MapSecuritiesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/securities")
            .WithTags("Securities")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllSecurities")
            .WithSummary("Retrieves all securities with optional filtering, sorting, and pagination")
            .Produces<PagedResult<SecurityDto>>()
            ;

        group.MapGet("/{id:int}", GetById)
            .WithName("GetSecuritiesById")
            .WithSummary("Retrieves a specific security by ID")
            .Produces<SecurityDto>()
            .Produces(StatusCodes.Status404NotFound)
            ;

        group.MapPost("/", Create)
            .WithName("CreateSecurity")
            .WithSummary("Creates a new security")
            .Produces<SecurityDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            ;

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateSecurity")
            .WithSummary("Updates an existing security")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            ;

        group.MapPatch("/{id:int}/deactivate", Deactivate)
            .WithName("DeactivateSecurity")
            .WithSummary("Deactivates a security and records the deactivation date")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            ;

        return routes;
    }

    private static async Task<IResult> GetAll(
        [AsParameters] PaginationParameters pagination,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(SecuritiesEndpoints));
        var userId = context.GetUserId();

        logger.LogInformation(
            "Retrieving securities - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}, User: {UserId}",
            pagination.Limit, pagination.Offset, pagination.SortBy ?? "none", pagination.Filter ?? "none", userId);

        var query = new GetSecuritiesWithQueryQuery(
            pagination.Limit,
            pagination.Offset,
            pagination.SortBy,
            pagination.SortDirection,
            pagination.Filter);

        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation(
            "Securities retrieved - Returned: {Count} of {Total} total securities",
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
        var logger = loggerFactory.CreateLogger(typeof(SecuritiesEndpoints));
        var userId = context.GetUserId();

        logger.LogInformation("Retrieving security {SecurityId} for user {UserId}", id, userId);

        var query = new GetSecurityByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        logger.LogInformation("Security retrieved - Name: {Name}, Ticker: {Ticker}, Type: {Type}",
            result.Name, result.Ticker, result.Type);

        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateSecurityDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(SecuritiesEndpoints));
        var userId = context.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogError("Authentication failed: 'sub' claim missing from token for endpoint {Endpoint}",
                context.Request.Path);
            return Results.Unauthorized();
        }

        logger.LogInformation(
            "Creating security - Name: {Name}, Ticker: {Ticker}, Isin: {Isin}, Type: {Type}, Currency: {Currency}, CreatedBy: {UserId}",
            dto.Name, dto.Ticker, dto.Isin, dto.Type, dto.Currency, userId);

        var command = new CreateSecurityCommand(
            dto.Name,
            dto.Ticker,
            dto.Isin,
            dto.Type,
            dto.Currency,
            userId);

        var result = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Security created successfully - Id: {SecurityId}, Name: {Name}", result.Id, result.Name);

        return Results.CreatedAtRoute("GetSecuritiesById", new { id = result.Id }, result);
    }

    private static async Task<IResult> Update(
        int id,
        UpdateSecurityDto dto,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(SecuritiesEndpoints));
        var userId = context.GetUserId();

        logger.LogInformation(
            "Updating security {SecurityId} - Name: {Name}, Ticker: {Ticker}, Type: {Type}, Currency: {Currency}, UpdatedBy: {UserId}",
            id, dto.Name, dto.Ticker, dto.Type, dto.Currency, userId);

        var command = new UpdateSecurityCommand(
            id,
            dto.Name,
            dto.Ticker,
            dto.Isin,
            dto.Type,
            dto.Currency);

        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Security updated successfully - Id: {SecurityId}", id);

        return Results.NoContent();
    }

    private static async Task<IResult> Deactivate(
        int id,
        IMediator mediator,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(SecuritiesEndpoints));
        var userId = context.GetUserId();

        logger.LogInformation("Deactivating security {SecurityId} - DeactivatedBy: {UserId}", id, userId);

        var command = new DeactivateSecurityCommand(id);
        await mediator.Send(command, cancellationToken);

        logger.LogInformation("Security deactivated successfully - Id: {SecurityId}", id);

        return Results.NoContent();
    }
}
