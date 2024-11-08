using System.Linq.Expressions;

namespace BuildingBlocks.Abstractions.Messaging.PersistMessage;

public interface IMessagePersistenceRepository
{
    Task AddAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default);
    Task UpdateAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default);

    Task ChangeStateAsync(Guid messageId, MessageStatus status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoreMessage>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoreMessage>> GetByFilterAsync(
        Expression<Func<StoreMessage, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<TResult>> GetSelectorByFilterAsync<TResult>(
        Expression<Func<StoreMessage, bool>> predicate,
        Expression<Func<StoreMessage, TResult>> selector,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<TResult>> GetSelectorAfterGroupingByFilterAsync<TKey, TResult>(
        Expression<Func<StoreMessage, bool>> predicate,
        Expression<Func<StoreMessage, TKey>> grouping,
        Expression<Func<IGrouping<TKey, StoreMessage>, TResult>> selector,
        CancellationToken cancellationToken = default
    );

    Task<StoreMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default);

    Task CleanupMessages();
}
