using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces.QueryServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Indexadores.Queries;

/// <summary>
///     Handler for retrieving indexadores with RFC-8040 compliant filtering, sorting, and pagination.
/// </summary>
public class GetIndexadoresWithQueryQueryHandler
    : IRequestHandler<GetIndexadoresWithQueryQuery, Application.Models.PagedResult<IndexadorDto>>
{
    private readonly IIndexadorQueryService _indexadorQueryService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetIndexadoresWithQueryQueryHandler> _logger;

    public GetIndexadoresWithQueryQueryHandler(
        IIndexadorQueryService indexadorQueryService,
        IMapper mapper,
        ILogger<GetIndexadoresWithQueryQueryHandler> logger)
    {
        _indexadorQueryService = indexadorQueryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Application.Models.PagedResult<IndexadorDto>> Handle(
        GetIndexadoresWithQueryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving indexadores with filters - Limit: {Limit}, Offset: {Offset}, SortBy: {SortBy}, Filter: {Filter}",
            request.Limit, request.Offset, request.SortBy, request.Filter);

        var parameters = new Domain.Models.QueryParameters
        {
            Limit = request.Limit,
            Offset = request.Offset,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filter = request.Filter
        };

        var (indexadores, totalCount) = await _indexadorQueryService.GetWithQueryAsync(
            parameters,
            cancellationToken);

        var indexadorDtos = _mapper.Map<IReadOnlyList<IndexadorDto>>(indexadores);

        return new Application.Models.PagedResult<IndexadorDto>(
            indexadorDtos,
            totalCount,
            parameters.Limit,
            parameters.Offset);
    }
}
