using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.TransactionSubTypes.Queries;
using MediatR;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing TransactionSubType resources.
/// </summary>
public static class TransactionSubTypesEndpoints
{
    public static IEndpointRouteBuilder MapTransactionSubTypesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/transactions/subtypes")
            .WithTags("Transaction SubTypes")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllTransactionSubTypes")
            .WithSummary("Retrieves all transaction subtypes with optional filtering by type ID")
            .Produces<IReadOnlyList<TransactionSubTypeDto>>()
            ;

        group.MapGet("/{id:int}", GetById)
            .WithName("GetTransactionSubTypeById")
            .WithSummary("Retrieves a specific transaction subtype by ID")
            .Produces<TransactionSubTypeDto>()
            .Produces(StatusCodes.Status404NotFound)
            ;

        return routes;
    }

    private static async Task<IResult> GetAll(
        int? typeId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTransactionSubTypesQuery(typeId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetTransactionSubTypeByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
