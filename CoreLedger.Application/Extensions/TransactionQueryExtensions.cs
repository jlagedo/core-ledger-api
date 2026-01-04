using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Application.Extensions;

/// <summary>
///     Extension methods for Transaction queries to encapsulate common navigation property loading patterns.
/// </summary>
public static class TransactionQueryExtensions
{
    /// <summary>
    ///     Includes all navigation properties for Transaction entity:
    ///     Fund, Security, TransactionSubType (with Type), and Status.
    /// </summary>
    /// <param name="query">The queryable Transaction collection.</param>
    /// <returns>The queryable with all navigation properties included.</returns>
    public static IQueryable<Transaction> WithNavigationProperties(this IQueryable<Transaction> query) =>
        query
            .Include(t => t.Fund)
            .Include(t => t.Security)
            .Include(t => t.TransactionSubType!)
                .ThenInclude(st => st.Type)
            .Include(t => t.Status);
}
