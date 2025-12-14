using CoreLedger.Domain.Entities;

namespace CoreLedger.Domain.Interfaces;

/// <summary>
/// Repository interface for ToDo-specific operations.
/// </summary>
public interface IToDoRepository : IRepository<ToDo>
{
    Task<IReadOnlyList<ToDo>> GetCompletedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ToDo>> GetIncompleteAsync(CancellationToken cancellationToken = default);
}
