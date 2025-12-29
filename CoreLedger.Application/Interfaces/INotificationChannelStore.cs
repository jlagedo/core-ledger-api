using CoreLedger.Application.Models;

namespace CoreLedger.Application.Interfaces;

/// <summary>
/// Port interface for managing per-user SSE notification channels.
/// Thread-safe store for active SSE connections.
/// </summary>
public interface INotificationChannelStore
{
    /// <summary>
    /// Gets an existing channel or creates a new one for the specified user.
    /// </summary>
    /// <param name="userId">The user ID for which to get or create a channel.</param>
    /// <returns>The UserChannel for the specified user.</returns>
    UserChannel GetOrCreate(int userId);

    /// <summary>
    /// Attempts to get an existing channel for the specified user.
    /// </summary>
    /// <param name="userId">The user ID to look up.</param>
    /// <param name="channel">The UserChannel if found, null otherwise.</param>
    /// <returns>True if the channel exists, false otherwise.</returns>
    bool TryGet(int userId, out UserChannel? channel);

    /// <summary>
    /// Removes a channel for the specified user.
    /// </summary>
    /// <param name="userId">The user ID whose channel should be removed.</param>
    /// <returns>True if the channel was removed, false if it didn't exist.</returns>
    bool Remove(int userId);

    /// <summary>
    /// Gets all active channels for iteration.
    /// </summary>
    /// <returns>An enumerable of user ID and UserChannel pairs.</returns>
    IEnumerable<KeyValuePair<int, UserChannel>> GetAll();

    /// <summary>
    /// Gets the count of active channels.
    /// </summary>
    int ActiveCount { get; }

    /// <summary>
    /// Indicates whether the service is shutting down.
    /// </summary>
    bool IsShuttingDown { get; }

    /// <summary>
    /// Marks the store as shutting down to reject new connections.
    /// </summary>
    void MarkShuttingDown();
}
