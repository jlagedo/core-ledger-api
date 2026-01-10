using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Models;

namespace CoreLedger.Application.Interfaces.QueryServices;

/// <summary>
///     Query service for HistoricoIndexador queries with filtering, sorting, and pagination.
/// </summary>
public interface IHistoricoIndexadorQueryService
{
    /// <summary>
    ///     Gets historical data for a specific indexador with pagination and sorting.
    /// </summary>
    /// <param name="indexadorId">The indexador ID to filter by.</param>
    /// <param name="parameters">Query parameters including sort, limit, and offset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of historicos list and total count for pagination.</returns>
    Task<(IReadOnlyList<HistoricoIndexador> Historicos, int TotalCount)> GetByIndexadorIdAsync(
        int indexadorId,
        QueryParameters parameters,
        CancellationToken cancellationToken = default);
}
