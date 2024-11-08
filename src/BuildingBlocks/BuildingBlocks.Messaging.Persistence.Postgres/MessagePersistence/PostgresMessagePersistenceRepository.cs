using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class PostgresMessagePersistenceRepository(
    MessagePersistenceDbContext persistenceDbContext,
    ILogger<PostgresMessagePersistenceRepository> logger
) : IMessagePersistenceRepository
{
    private readonly ILogger<PostgresMessagePersistenceRepository> _logger = logger;

    public virtual async Task AddAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        await persistenceDbContext.StoreMessages.AddAsync(storeMessage, cancellationToken);

        await persistenceDbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        persistenceDbContext.StoreMessages.Update(storeMessage);

        await persistenceDbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task ChangeStateAsync(
        Guid messageId,
        MessageStatus status,
        CancellationToken cancellationToken = default
    )
    {
        // tacked entity here by EF
        var message = await persistenceDbContext.StoreMessages.FirstOrDefaultAsync(
            x => x.Id == messageId,
            cancellationToken
        );
        if (message is not null)
        {
            message.ChangeState(status);
            await persistenceDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<IReadOnlyList<StoreMessage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return (await persistenceDbContext.StoreMessages.AsNoTracking().ToListAsync(cancellationToken)).AsReadOnly();
    }

    public virtual async Task<IReadOnlyList<StoreMessage>> GetByFilterAsync(
        Expression<Func<StoreMessage, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await persistenceDbContext.StoreMessages.Where(predicate).AsNoTracking().ToListAsync(cancellationToken)
        ).AsReadOnly();
    }

    public virtual async Task<IReadOnlyList<TResult>> GetSelectorByFilterAsync<TResult>(
        Expression<Func<StoreMessage, bool>> predicate,
        Expression<Func<StoreMessage, TResult>> selector,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await persistenceDbContext
                .StoreMessages.Where(predicate)
                .AsNoTracking()
                .Select(selector)
                .ToListAsync(cancellationToken)
        ).AsReadOnly();
    }

    public virtual async Task<IReadOnlyList<TResult>> GetSelectorAfterGroupingByFilterAsync<TKey, TResult>(
        Expression<Func<StoreMessage, bool>> predicate,
        Expression<Func<StoreMessage, TKey>> grouping,
        Expression<Func<IGrouping<TKey, StoreMessage>, TResult>> selector,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await persistenceDbContext
                .StoreMessages.Where(predicate)
                .AsNoTracking()
                .GroupBy(grouping)
                .Select(selector)
                .ToListAsync(cancellationToken)
        ).AsReadOnly();
    }

    public virtual Task<StoreMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // tacked entity here by EF
        return persistenceDbContext.StoreMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public virtual async Task<bool> RemoveAsync(
        StoreMessage storeMessage,
        CancellationToken cancellationToken = default
    )
    {
        persistenceDbContext.StoreMessages.Remove(storeMessage);
        var res = await persistenceDbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public virtual async Task CleanupMessages()
    {
        if (!await persistenceDbContext.StoreMessages.AnyAsync())
            return;

        persistenceDbContext.StoreMessages.RemoveRange(persistenceDbContext.StoreMessages);

        await persistenceDbContext.SaveChangesAsync();
    }
}
