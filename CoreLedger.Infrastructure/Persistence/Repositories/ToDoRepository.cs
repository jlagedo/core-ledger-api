using Microsoft.EntityFrameworkCore;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;

namespace CoreLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for ToDo entity with specific queries.
/// </summary>
public class ToDoRepository : Repository<ToDo>, IToDoRepository
{
    public ToDoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ToDo>> GetCompletedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.CompletedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ToDo>> GetIncompleteAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => !t.IsCompleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
