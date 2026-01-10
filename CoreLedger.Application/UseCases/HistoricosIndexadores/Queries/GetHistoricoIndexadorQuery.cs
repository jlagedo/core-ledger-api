using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Models;
using MediatR;

namespace CoreLedger.Application.UseCases.HistoricosIndexadores.Queries;

/// <summary>
///     Query to get historical data for an indexador with pagination and filtering.
/// </summary>
public record GetHistoricoIndexadorQuery(
    int IndexadorId,
    QueryParameters Parameters
) : IRequest<PagedResult<HistoricoIndexadorDto>>;
