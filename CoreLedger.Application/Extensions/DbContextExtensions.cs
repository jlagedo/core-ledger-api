using CoreLedger.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.Extensions;

/// <summary>
///     Extension methods for DbSet to encapsulate common data access patterns.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    ///     Validates that an entity with the specified ID exists in the database.
    ///     Throws EntityNotFoundException if the entity is not found.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to validate.</typeparam>
    /// <param name="dbSet">The DbSet to search in.</param>
    /// <param name="keyValues">The primary key values to search for.</param>
    /// <param name="entityTypeName">The name of the entity type for error messages.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The found entity.</returns>
    /// <exception cref="EntityNotFoundException">Thrown when the entity is not found.</exception>
    public static async Task<TEntity> ValidateEntityExistsAsync<TEntity>(
        this DbSet<TEntity> dbSet,
        object[] keyValues,
        string entityTypeName,
        ILogger logger,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var entity = await dbSet.FindAsync(keyValues, cancellationToken);

        if (entity == null)
        {
            var keyValue = keyValues.Length == 1 ? keyValues[0] : string.Join(", ", keyValues);
            logger.LogWarning("Validation failed: {EntityType} {KeyValue} not found",
                entityTypeName, keyValue);
            throw new EntityNotFoundException(entityTypeName, keyValue);
        }

        return entity;
    }
}
