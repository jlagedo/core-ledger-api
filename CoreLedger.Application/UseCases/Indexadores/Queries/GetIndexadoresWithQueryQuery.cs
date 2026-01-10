using CoreLedger.Application.DTOs;
using MediatR;

namespace CoreLedger.Application.UseCases.Indexadores.Queries;

/// <summary>
///     Query to retrieve indexadores with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public record GetIndexadoresWithQueryQuery(
    int Limit,
    int Offset,
    string? SortBy,
    string SortDirection,
    string? Filter
) : IRequest<Application.Models.PagedResult<IndexadorDto>>;
